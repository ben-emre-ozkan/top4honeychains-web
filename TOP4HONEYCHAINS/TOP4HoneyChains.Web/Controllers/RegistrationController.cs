using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P1 (D78-SPEC 4.1, 4.2, F2-F5): beekeeper registration by a district directorate's registration officer (or a
    // platform administrator): registry number check, information notice and consents, the officer's attestation, confirmed by
    // an SMS code to the beekeeper's phone, or by a signed photo that the organization's Compliance Officer accepts.
    // An officer sees only the registrations of their own organization.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class RegistrationController : Controller
    {
        public const string OpSend = "send";
        public const string OpConfirm = "confirm";
        public const string OpPhoto = "photo";

        public record ListRow(BeekeeperRegistration Registration, string Name, string BusinessNumber);

        private readonly TOP4HoneyChainsContext _context;
        private readonly RegistrationService _registration;
        private readonly IMessageService _messages;
        private readonly ICurrentOrg _current;
        private readonly IWebHostEnvironment _env;
        private readonly SmsOptions _sms;
        private readonly FakeSmsStore _fake;
        private readonly BeekeeperSignIn _signIn;

        public RegistrationController(TOP4HoneyChainsContext context, RegistrationService registration, IMessageService messages, ICurrentOrg current,
            IWebHostEnvironment env, IOptions<SmsOptions> sms, FakeSmsStore fake, BeekeeperSignIn signIn)
        {
            _context = context;
            _registration = registration;
            _messages = messages;
            _current = current;
            _env = env;
            _sms = sms.Value;
            _fake = fake;
            _signIn = signIn;
        }

        // ---------------------------------------------------------------- list and find

        public async Task<IActionResult> Index()
        {
            var registrar = await _registration.RegistrarAsync(PermissionAreas.Read) ?? await _registration.RegistrarAsync();
            if (registrar == null)
                return NotFound();
            var q = _context.BeekeeperRegistrations.AsNoTracking().Include(r => r.Beekeeper).Include(r => r.RegisteringOrg).AsQueryable();
            if (!registrar.IsPlatformAdmin)
                q = q.Where(r => r.RegisteringOrgId == registrar.OrgId);
            var rows = await q.OrderByDescending(r => r.RegistrationId).Take(300).ToListAsync();
            ViewBag.Registrar = registrar;
            ViewBag.CanRegister = await _registration.RegistrarAsync() != null;
            ViewBag.Controller = "Registration";
            return View(rows);
        }

        // by registry number or business number: an existing beekeeper is re-verified, otherwise a new registration starts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Find(string? number)
        {
            var registrar = await _registration.RegistrarAsync();
            if (registrar == null)
                return NotFound();
            var normalized = RegistryNumber.Normalize(number);
            if (string.IsNullOrEmpty(normalized))
            {
                TempData["Error"] = "Enter the registry number (or the business number of a registered beekeeper).";
                return RedirectToAction(nameof(Index));
            }
            var beekeeperId = await _context.BeekeeperRegistry.Where(r => r.RegistryNumber == normalized).Select(r => r.BeekeeperId).FirstOrDefaultAsync()
                              ?? await _context.Beekeepers.Where(b => b.BeekeeperBusinessNumber == number!.Trim()).Select(b => b.BeekeeperId).FirstOrDefaultAsync();
            if (beekeeperId == null)
            {
                if (!RegistryNumber.IsWellFormed(normalized))
                {
                    TempData["Error"] = "The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890).";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(New), new { registryNumber = normalized });
            }
            // a beekeeper registered by another directorate is not shown to this officer
            if (!registrar.IsPlatformAdmin && await _context.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == beekeeperId && r.RegisteringOrgId != registrar.OrgId))
            {
                TempData["Error"] = "This beekeeper is registered by another directorate.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Reverify), new { id = beekeeperId });
        }

        // ---------------------------------------------------------------- the form

        [HttpGet]
        public async Task<IActionResult> New(string? registryNumber)
        {
            var registrar = await _registration.RegistrarAsync();
            if (registrar == null)
                return NotFound();
            return await FormAsync(new RegistrationForm { RegistryNumber = registryNumber, Language = NoticeService.Language }, registrar, codeSent: false);
        }

        [HttpGet]
        public async Task<IActionResult> Reverify(string id)
        {
            var registrar = await _registration.RegistrarAsync();
            if (registrar == null)
                return NotFound();
            var bk = await _context.Beekeepers.AsNoTracking().FirstOrDefaultAsync(b => b.BeekeeperId == id);
            if (bk == null || (!registrar.IsPlatformAdmin && await _context.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == id && r.RegisteringOrgId != registrar.OrgId)))
                return NotFound();
            var registry = await _context.BeekeeperRegistry.AsNoTracking().FirstOrDefaultAsync(r => r.BeekeeperId == id);
            var form = new RegistrationForm
            {
                BeekeeperId = bk.BeekeeperId, RegistryNumber = registry?.RegistryNumber, FirstName = bk.BeekeeperFirstName, LastName = bk.BeekeeperLastName,
                BirthDate = bk.BeekeeperBirthDate, Phone = PhoneNumberPolicy.Display(bk.BeekeeperPhoneNumber), EducationLevelId = bk.BeekeeperEducationLevelId,
                PurposeTypeId = bk.BeekeeperBeekeepingPurposeTypeId, BeekeepingTypeId = bk.BeekeeperBeekeepingTypeId, Language = bk.PreferredLanguage ?? NoticeService.Language
            };
            return await FormAsync(form, registrar, codeSent: false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Submit(RegistrationForm form, string op, IFormFile? evidence)
        {
            var registrar = await _registration.RegistrarAsync();
            if (registrar == null)
                return NotFound();
            if (!form.IsNew)
            {
                if (!await _context.Beekeepers.AnyAsync(b => b.BeekeeperId == form.BeekeeperId)
                    || (!registrar.IsPlatformAdmin && await _context.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == form.BeekeeperId && r.RegisteringOrgId != registrar.OrgId)))
                    return NotFound();
            }
            else if (string.IsNullOrEmpty(form.DraftKey) || !Guid.TryParse(form.DraftKey, out _))
                form.DraftKey = Guid.NewGuid().ToString("N");

            var photo = op == OpPhoto;
            var (data, errors) = await _registration.CheckAsync(form, registrar, photo);
            if (photo)
            {
                if (!form.NoPhone && string.IsNullOrWhiteSpace(form.FallbackReason))
                    errors.Add("Give the reason why the SMS code cannot be used.");
                if (evidence == null || evidence.Length == 0)
                    errors.Add("Attach the photo of the signed registration form.");
                else if (await UploadValidator.ValidateDocumentAsync(evidence) is string fileError)
                    errors.Add(fileError);
            }
            if (errors.Count > 0)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);
                return await FormAsync(form, registrar, codeSent: op == OpConfirm);
            }

            var officer = _current.UserId!;
            switch (op)
            {
                case OpSend:
                {
                    var link = PublicUrl.Build(HttpContext, registrar.IsPlatformAdmin ? "/privacy" : $"/privacy?org={registrar.OrgId}")
                               ?? $"{Request.Scheme}://{Request.Host}/privacy";
                    var sent = await _messages.SendCodeAsync(new CodeRequest(SmsPurpose.Registration, "Registration", RegistrationService.SubjectKey(form),
                        data!.PhoneE164!, form.Language, RegistrationService.ContentHash(form, data),
                        new Dictionary<string, string> { ["org"] = registrar.OrgName, ["link"] = link, ["name"] = SmsTemplates.YesNoWord(form.PublicName, form.Language) },
                        officer, registrar.OrgId));
                    if (!sent.Ok)
                        ModelState.AddModelError("", sent.Error!);
                    else
                        TempData["Success"] = "A code was sent to the beekeeper's phone. Enter the code the beekeeper reads out.";
                    return await FormAsync(form, registrar, codeSent: sent.Ok, sentTo: sent.Ok ? data.PhoneE164 : null);
                }
                case OpConfirm:
                {
                    var verified = await _messages.VerifyCodeAsync(SmsPurpose.Registration, "Registration", RegistrationService.SubjectKey(form), form.Code,
                        RegistrationService.ContentHash(form, data!), officer);
                    if (!verified.Ok)
                    {
                        ModelState.AddModelError("", verified.Error!);
                        return await FormAsync(form, registrar, codeSent: true);
                    }
                    var saved = await _registration.SaveAsync(form, data!, registrar, officer, verified.CodeId, null);
                    TempData["Success"] = "The beekeeper is registered and verified.";
                    if (form.IssueSignIn && _signIn.Enabled)
                    {
                        var bk = await _context.Beekeepers.FirstAsync(b => b.BeekeeperId == saved.BeekeeperId);
                        var issued = await _signIn.IssueAsync(bk, officer, CredentialService.SignInLink(HttpContext));
                        TempData[issued.Ok ? "Success" : "Error"] = issued.Ok
                            ? "The beekeeper is registered and verified; the sign-in details were sent by SMS."
                            : issued.Error;
                    }
                    return RedirectToAction(nameof(Details), new { id = saved.RegistrationId });
                }
                case OpPhoto:
                {
                    var (stored, sha) = await FileStore.SaveAsync(_env, RegistrationService.EvidenceFolder, evidence!);
                    var saved = await _registration.SaveAsync(form, data!, registrar, officer, null, (stored, Path.GetFileName(evidence!.FileName), sha));
                    TempData["Success"] = "The registration is saved with the signed photo. It waits for the Compliance Officer's review.";
                    return RedirectToAction(nameof(Details), new { id = saved.RegistrationId });
                }
                default:
                    return BadRequest();
            }
        }

        private async Task<IActionResult> FormAsync(RegistrationForm form, Registrar registrar, bool codeSent, string? sentTo = null)
        {
            ViewBag.Registrar = registrar;
            ViewBag.CodeSent = codeSent;
            ViewBag.SignInEnabled = _signIn.Enabled;
            ViewBag.Education = new SelectList(await _context.BeekeeperEducationLevels.AsNoTracking().ToListAsync(), "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", form.EducationLevelId).Translated(HttpContext, "EducationLevel", "Title");
            ViewBag.Purposes = new SelectList(await _context.BeekeepingPurposeTypes.AsNoTracking().ToListAsync(), "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", form.PurposeTypeId).Translated(HttpContext, "BeekeepingPurposeType", "Title");
            ViewBag.Types = new SelectList(await _context.BeekeepingTypes.AsNoTracking().ToListAsync(), "BeekeepingTypeId", "BeekeepingTypeTitle", form.BeekeepingTypeId).Translated(HttpContext, "BeekeepingType", "Title");
            ViewBag.Apiaries = form.IsNew ? new List<Apiary>() : await _context.Apiaries.AsNoTracking().Where(a => a.ApiaryBeekeeperId == form.BeekeeperId).ToListAsync();
            ViewBag.Notice = await HttpContext.RequestServices.GetRequiredService<NoticeService>().CurrentAsync(NoticeKind.PrivacyNotice, NoticeAudience.Beekeeper, null, null, form.Language);
            ViewBag.Attestation = await HttpContext.RequestServices.GetRequiredService<NoticeService>().CurrentAsync(NoticeKind.Attestation, NoticeAudience.Member, null, null);
            // demonstrations without a phone: the message the fake provider "sent" (Sms:DemoPanel)
            ViewBag.DemoMessage = _sms.IsFake && _sms.DemoPanel && sentTo != null ? _fake.LastTo(sentTo)?.Text : null;
            ViewBag.Controller = "Registration";
            return View("Form", form);
        }

        // ---------------------------------------------------------------- one registration, its evidence and its review

        public async Task<IActionResult> Details(int id)
        {
            var r = await _context.BeekeeperRegistrations.AsNoTracking().Include(x => x.Beekeeper).ThenInclude(b => b!.Apiaries).Include(x => x.RegisteringOrg)
                .FirstOrDefaultAsync(x => x.RegistrationId == id);
            if (r == null || !await _registration.MayReadAsync(r))
                return NotFound();
            ViewBag.Registry = await _context.BeekeeperRegistry.AsNoTracking().FirstOrDefaultAsync(x => x.BeekeeperId == r.BeekeeperId);
            ViewBag.Consents = await _context.Consents.AsNoTracking().Where(c => c.BeekeeperId == r.BeekeeperId && c.RegisteringOrgId != null)
                .OrderByDescending(c => c.ConsentId).ToListAsync();
            ViewBag.MayReview = r.EvidenceStatus == EvidenceStatus.PendingReview && await _registration.MayReviewAsync(r);
            ViewBag.IsOfficer = r.OfficerUserId == _current.UserId;
            ViewBag.CanRegister = await _registration.RegistrarAsync() is Registrar reg && (reg.IsPlatformAdmin || reg.OrgId == r.RegisteringOrgId);
            ViewBag.SignInEnabled = _signIn.Enabled;
            ViewBag.Controller = "Registration";
            return View(r);
        }

        // the signed photo: the registering organization's registrars and reviewers, and platform administrators
        public async Task<IActionResult> Evidence(int id)
        {
            var r = await _context.BeekeeperRegistrations.AsNoTracking().FirstOrDefaultAsync(x => x.RegistrationId == id);
            if (r == null || r.EvidenceStoredFileName == null || !await _registration.MayReadAsync(r))
                return NotFound();
            var path = FileStore.PathOf(_env, RegistrationService.EvidenceFolder, r.EvidenceStoredFileName);
            if (path == null)
                return NotFound();
            await HttpContext.RequestServices.GetRequiredService<AuditWriter>().WriteAsync("Download", "BeekeeperRegistrations", id.ToString());
            return PhysicalFile(path, FileStore.ContentTypeOf(path), r.EvidenceOriginalFileName ?? Path.GetFileName(path));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, bool accept, string? reason)
        {
            var r = await _context.BeekeeperRegistrations.FirstOrDefaultAsync(x => x.RegistrationId == id);
            if (r == null || !await _registration.MayReadAsync(r))
                return NotFound();
            if (r.EvidenceStatus != EvidenceStatus.PendingReview)
                TempData["Error"] = "This registration was already reviewed.";
            else if (r.OfficerUserId == _current.UserId)
                TempData["Error"] = "You recorded this registration; another person must review its evidence.";
            else if (!await _registration.MayReviewAsync(r))
                return Forbid();
            else if (!accept && string.IsNullOrWhiteSpace(reason))
                TempData["Error"] = "Give the reason for the rejection.";
            else
            {
                await _registration.ReviewAsync(r, accept, reason, _current.UserId!);
                TempData["Success"] = accept ? "Evidence accepted; the registry number is verified." : "Evidence rejected; the beekeeper cannot deliver until registered again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // more apiaries of a registered beekeeper (the officer of the registering organization)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddApiary(int id, decimal? apiaryLatitude, decimal? apiaryLongitude, string? apiaryProvince, int? apiaryDistrictId,
            string? apiaryLocality, short? apiaryHives)
        {
            var r = await _context.BeekeeperRegistrations.AsNoTracking().FirstOrDefaultAsync(x => x.RegistrationId == id);
            var registrar = await _registration.RegistrarAsync();
            if (r == null || registrar == null || (!registrar.IsPlatformAdmin && registrar.OrgId != r.RegisteringOrgId))
                return NotFound();
            var (province, district, placeError) = await ApiaryTitles.ResolveAsync(_context, apiaryProvince, apiaryDistrictId, apiaryLocality);
            string? error = placeError;
            if (apiaryLatitude is not (>= -90 and <= 90) || apiaryLongitude is not (>= -180 and <= 180) || (apiaryLatitude == 0 && apiaryLongitude == 0))
                error ??= "Set the apiary location on the map or type its coordinates.";
            if (apiaryHives is not (>= 1 and <= 10000))
                error ??= "The beehive count must be between 1 and 10000.";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Details), new { id });
            }
            var apiary = await _registration.NewApiaryAsync(r.BeekeeperId, apiaryLatitude!.Value, apiaryLongitude!.Value, apiaryProvince!, apiaryDistrictId!.Value,
                apiaryLocality!, apiaryHives!.Value, province!, district!);
            _context.Apiaries.Add(apiary);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Apiary {apiary.ApiaryTitle} added.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // O-15: the beekeeper's sign-in, later than the registration (only with Features:BeekeeperAccounts)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueSignIn(int id)
        {
            var r = await _context.BeekeeperRegistrations.AsNoTracking().Include(x => x.Beekeeper).FirstOrDefaultAsync(x => x.RegistrationId == id);
            var registrar = await _registration.RegistrarAsync();
            if (r == null || registrar == null || (!registrar.IsPlatformAdmin && registrar.OrgId != r.RegisteringOrgId) || !_signIn.Enabled)
                return NotFound();
            if (r.Method != AcceptanceMethod.SmsCode || r.EvidenceStatus != EvidenceStatus.Accepted)
                TempData["Error"] = "A sign-in needs a registration confirmed by the SMS code (O-17: no account without a mobile phone).";
            else
            {
                var issued = await _signIn.IssueAsync(r.Beekeeper!, _current.UserId!, CredentialService.SignInLink(HttpContext));
                TempData[issued.Ok ? "Success" : "Error"] = issued.Ok ? "The sign-in details were sent by SMS." : issued.Error;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
