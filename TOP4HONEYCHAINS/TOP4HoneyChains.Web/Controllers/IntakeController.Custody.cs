using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P2 (D78-SPEC 4.4-4.5, D-81): the delivery with sealed containers. Behind Features:CustodyIntake; while it is off the
    // intake works as before. One container row each, a jar from every container, the beekeeper confirms with an SMS code (M3)
    // that binds the whole content (all seal numbers); a signed photo is the fallback and waits for the Compliance Officer.
    public partial class IntakeController
    {
        public const string OpSend = "send";
        public const string OpConfirm = "confirm";
        public const string OpPhoto = "photo";

        // read per request, so the switch can change without a restart
        private bool CustodyOn => HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<FeatureFlags>>().Value.CustodyIntake;
        private CustodyIntakeService Custody => HttpContext.RequestServices.GetRequiredService<CustodyIntakeService>();
        private IMessageService Messages => HttpContext.RequestServices.GetRequiredService<IMessageService>();

        private Task<List<Organization>> LabsAsync() =>
            _context.Organizations.AsNoTracking()
                .Where(o => !o.IsPlatform && o.Status == OrgStatus.Verified && o.Capabilities.Any(c => c.Capability == Capabilities.Laboratory))
                .OrderBy(o => o.DisplayName).ToListAsync();

        private async Task<IActionResult> CustodyFormAsync(IntakeForm form, Beekeeper? bk, bool codeSent, string? sentTo = null)
        {
            await FillListsAsync();
            ViewBag.Beekeeper = bk;
            ViewBag.Labs = await LabsAsync();
            ViewBag.CodeSent = codeSent;
            ViewBag.HasPhone = !string.IsNullOrWhiteSpace(bk?.BeekeeperPhoneNumber);
            var sms = HttpContext.RequestServices.GetRequiredService<IOptions<SmsOptions>>().Value;
            ViewBag.DemoMessage = sms.IsFake && sms.DemoPanel && sentTo != null
                ? HttpContext.RequestServices.GetRequiredService<FakeSmsStore>().LastTo(sentTo)?.Text : null;
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Create";
            return View("CreateCustody", form);
        }

        // the delivery fields, as for the legacy intake, plus the laboratory and the counter-samples
        private async Task<List<string>> CheckDeliveryAsync(IntakeForm f, Beekeeper? bk)
        {
            var errors = new List<string>();
            f.Province = f.Province?.Trim();
            f.District = f.District?.Trim();
            f.CounterLocation = f.CounterLocation?.Trim();
            if (bk == null)
                errors.Add("No active beekeeper with this number.");
            else if (bk.Apiaries.All(a => a.ApiaryId != f.ApiaryId))
                errors.Add("Choose one of the beekeeper's apiaries.");
            if (!await _context.HoneyTypes.AnyAsync(h => h.HoneyTypeId == f.HoneyTypeId))
                errors.Add("Unknown honey type.");
            if (!await _context.RawHoneyProductionPeriods.AnyAsync(p => p.RawHoneyProductionPeriodId == f.PeriodId))
                errors.Add("The production period is required.");
            if (f.DeliveryDate == null || f.DeliveryDate > DateOnly.FromDateTime(DateTime.Today))
                errors.Add("Enter the delivery date (not in the future).");
            if (string.IsNullOrEmpty(f.Province) || string.IsNullOrEmpty(f.District) || f.Province.Length > 100 || f.District.Length > 100)
                errors.Add("Enter the province and district of origin.");
            else if (PlaceError(f.Province, f.District) is string placeError)
                errors.Add(placeError);
            if (!Origin.IsKnown(f.OriginCountry ?? Origin.Default))
                errors.Add("Choose the country of origin.");
            if ((await LabsAsync()).All(l => l.OrgId != f.LabOrgId))
                errors.Add("Choose a verified laboratory.");
            if (string.IsNullOrEmpty(f.CounterLocation) || f.CounterLocation.Length > 200)
                errors.Add("Enter where the counter-sample is kept (at most 200 characters).");
            if (f.RetainUntil == null || f.RetainUntil <= DateOnly.FromDateTime(DateTime.Today))
                errors.Add("Enter until when the counter-sample is kept (a future date).");
            if (await _context.RawHoneyProductions.AnyAsync(r => r.RawHoneyProductionApiaryId == f.ApiaryId && r.RawHoneyProductionPeriodId == f.PeriodId && r.RawHoneyProductionHoneyType == f.HoneyTypeId))
                errors.Add("This apiary already has a production record for the selected period and honey type.");
            errors.AddRange(Custody.CheckContainers(f));
            return errors;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Record(IntakeForm form, string op, IFormFile? deliveryForm)
        {
            if (!CustodyOn)
                return NotFound();
            if (await DeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrEmpty(form.DraftKey) || !Guid.TryParse(form.DraftKey, out _))
                form.DraftKey = Guid.NewGuid().ToString("N");
            var bk = await FindBeekeeperAsync(form.Number);
            var errors = await CheckDeliveryAsync(form, bk);
            var photo = op == OpPhoto;
            if (photo)
            {
                if (string.IsNullOrWhiteSpace(form.FallbackReason))
                    errors.Add("Give the reason why the SMS code cannot be used.");
                if (deliveryForm == null)
                    errors.Add("Attach the photo of the signed delivery form.");
                else if (await UploadValidator.ValidateDocumentAsync(deliveryForm) is string fileError)
                    errors.Add(fileError);
            }
            else if (bk != null && string.IsNullOrWhiteSpace(bk.BeekeeperPhoneNumber))
                errors.Add("The beekeeper has no mobile phone: confirm the delivery with the photo of the signed form.");
            if (errors.Count > 0)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);
                return await CustodyFormAsync(form, bk, codeSent: op == OpConfirm);
            }

            var active = (await _current.GetAsync())!;
            var content = CustodyIntakeService.ContentOf(form, bk!.BeekeeperId!);
            switch (op)
            {
                case OpSend:
                {
                    var values = await DeliveryValuesAsync(content, active.OrgName, bk.PreferredLanguage);
                    var sent = await Messages.SendCodeAsync(new CodeRequest(SmsPurpose.IntakeConfirm, "Intake", form.DraftKey!, bk.BeekeeperPhoneNumber!,
                        bk.PreferredLanguage, content.Hash(), values, _current.UserId, active.OrgId));
                    if (!sent.Ok)
                        ModelState.AddModelError("", sent.Error!);
                    else
                        TempData["Success"] = "A code was sent to the beekeeper's phone. Enter the code the beekeeper reads out (in person or by phone).";
                    return await CustodyFormAsync(form, bk, codeSent: sent.Ok, sentTo: sent.Ok ? PhoneNumberPolicy.Normalize(bk.BeekeeperPhoneNumber, out _) : null);
                }
                case OpConfirm:
                {
                    var verified = await Messages.VerifyCodeAsync(SmsPurpose.IntakeConfirm, "Intake", form.DraftKey!, form.Code, content.Hash(), _current.UserId);
                    if (!verified.Ok)
                    {
                        ModelState.AddModelError("", verified.Error!);
                        return await CustodyFormAsync(form, bk, codeSent: true);
                    }
                    var org = await _context.Organizations.AsNoTracking().FirstAsync(o => o.OrgId == active.OrgId);
                    var saved = await Custody.SaveAsync(form, content, active.OrgId, org.LegacyWholesalerId, _current.UserId!, verified.CodeId, null);
                    TempData["Success"] = $"Delivery confirmed by the beekeeper and recorded as lot #{saved.LotId}.";
                    return RedirectToAction(nameof(Details), new { id = saved.IntakeId });
                }
                case OpPhoto:
                {
                    var (stored, sha) = await FileStore.SaveAsync(_env, IntakeForms, deliveryForm!);
                    try
                    {
                        var org = await _context.Organizations.AsNoTracking().FirstAsync(o => o.OrgId == active.OrgId);
                        var saved = await Custody.SaveAsync(form, content, active.OrgId, org.LegacyWholesalerId, _current.UserId!, null,
                            (stored, Path.GetFileName(deliveryForm!.FileName), sha));
                        TempData["Success"] = $"Delivery recorded as lot #{saved.LotId} with the signed photo; it waits for the Compliance Officer.";
                        return RedirectToAction(nameof(Details), new { id = saved.IntakeId });
                    }
                    catch
                    {
                        if (FileStore.PathOf(_env, IntakeForms, stored) is string path)
                            System.IO.File.Delete(path);
                        throw;
                    }
                }
                default:
                    return BadRequest();
            }
        }

        // M3 placeholders in the beekeeper's language (honey type name, container words, numbers)
        private async Task<Dictionary<string, string>> DeliveryValuesAsync(IntakeContent content, string orgName, string? language)
        {
            var lang = SmsTemplates.Language(language);
            var culture = new CultureInfo(lang);
            var title = await _context.HoneyTypes.Where(h => h.HoneyTypeId == content.HoneyTypeId).Select(h => h.HoneyTypeTitle).FirstOrDefaultAsync();
            var previous = CultureInfo.CurrentUICulture;
            string honey;
            try
            {
                CultureInfo.CurrentUICulture = culture;
                honey = HttpContext.RequestServices.GetRequiredService<RefText>().HoneyType(content.HoneyTypeId, title);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
            return new Dictionary<string, string>
            {
                ["org"] = orgName,
                ["date"] = content.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                ["containers"] = SmsTemplates.ContainersText(content.Containers.Select(c => c.Kind), lang),
                ["kg"] = content.TotalKg.ToString("0.##", culture),
                ["honey"] = honey,
                ["samples"] = content.Containers.Count.ToString(CultureInfo.InvariantCulture)
            };
        }

        // ---------------------------------------------------------------- one delivery

        public async Task<IActionResult> Details(int id)
        {
            var intake = await _context.LotIntakes.AsNoTracking()
                .Include(i => i.ReceivingOrg)
                .Include(i => i.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionApiary!).ThenInclude(a => a.ApiaryBeekeeper)
                .Include(i => i.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(i => i.Containers).ThenInclude(c => c.SealEvents)
                .Include(i => i.Containers).ThenInclude(c => c.Samples).ThenInclude(s => s.LabOrg)
                .FirstOrDefaultAsync(i => i.IntakeId == id);
            if (intake == null || !await MaySeeIntakeAsync(intake))
                return NotFound();
            ViewBag.ContentMatches = await Custody.ContentMatchesAsync(id);
            ViewBag.MayReview = intake.EvidenceStatus == EvidenceStatus.PendingReview && await MayReviewIntakeAsync(intake);
            // D-78 P2: the plant that holds the lot takes a new jar after the laboratory rejected one
            ViewBag.MayResample = !_current.IsPlatformAdmin && await _current.GetAsync() is { } m && m.OrgId == intake.ReceivingOrgId
                && Perm.Allows(PermissionAreas.Samples, PermissionAreas.Enter, m.OrgRole);
            ViewBag.IsRecorder = intake.ReceivedByUserId == _current.UserId;
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Details";
            return View(intake);
        }

        // the receiving organization's Compliance Officer (Evidence/Approve), never the officer who recorded the delivery
        private async Task<bool> MayReviewIntakeAsync(LotIntake intake)
        {
            if (_current.IsPlatformAdmin || intake.ReceivedByUserId == _current.UserId)
                return false;
            var active = await _current.GetAsync();
            return active != null && active.IsVerified && active.OrgId == intake.ReceivingOrgId
                   && Perm.Allows(PermissionAreas.Evidence, PermissionAreas.Approve, active.OrgRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, bool accept, string? reason)
        {
            var intake = await _context.LotIntakes.FirstOrDefaultAsync(i => i.IntakeId == id);
            if (intake == null || !await MaySeeIntakeAsync(intake))
                return NotFound();
            if (intake.EvidenceStatus != EvidenceStatus.PendingReview)
                TempData["Error"] = "This delivery was already reviewed.";
            else if (intake.ReceivedByUserId == _current.UserId)
                TempData["Error"] = "You recorded this delivery; another person must review its signed photo.";
            else if (!await MayReviewIntakeAsync(intake))
                return Forbid();
            else if (!accept && string.IsNullOrWhiteSpace(reason))
                TempData["Error"] = "Give the reason for the rejection.";
            else
            {
                await Custody.ReviewAsync(intake, accept, reason, _current.UserId!);
                TempData["Success"] = accept ? "Signed photo accepted; the lot can be finalized when its CCPs are done."
                    : "Signed photo rejected; the lot cannot be finalized.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- labels (browser print: A4 sheet or 100 x 50 mm labels)

        public async Task<IActionResult> Labels(int id, string? size)
        {
            var intake = await _context.LotIntakes.AsNoTracking()
                .Include(i => i.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(i => i.Containers).ThenInclude(c => c.Samples)
                .Include(i => i.Containers).ThenInclude(c => c.SealEvents)
                .FirstOrDefaultAsync(i => i.IntakeId == id);
            if (intake == null || intake.IsLegacy || !await MaySeeIntakeAsync(intake))
                return NotFound();
            ViewBag.Size = size == "label" ? "label" : "a4";
            return View(intake);
        }

        // a QR code as inline SVG (no System.Drawing)
        [NonAction]
        public static string QrSvg(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            return new SvgQRCode(data).GetGraphic(3, "#000000", "#ffffff", drawQuietZones: true);
        }
    }
}
