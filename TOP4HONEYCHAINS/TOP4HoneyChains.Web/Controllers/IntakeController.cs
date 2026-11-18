using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Lot intake (ROLE-MODEL section 6, D-41): the SampleAcceptanceOfficer of a verified Processing
    // organization records a delivery from a beekeeper record with the photo of the signed delivery
    // form. The intake creates the raw lot, owned by that organization.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public partial class IntakeController : Controller
    {
        public const string IntakeForms = "App_Data/intake-forms";

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly ISmsSender _sms;
        private readonly IWebHostEnvironment _env;
        // D-56: rights of organization roles (editable matrix)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();

        public IntakeController(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, ISmsSender sms, IWebHostEnvironment env)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _sms = sms;
            _env = env;
        }

        private async Task<string?> DeniedAsync()
        {
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null)
                return "No active membership.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!active.Has(Capabilities.Processing))
                return "Only a Processing organization records deliveries.";
            if (!Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Intake, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot record deliveries; roles: {string.Join(", ", Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Intake, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter))}.";
            return null;
        }

        // Beekeeper by business number or verified registry number (exact match only, no browsing)
        private async Task<Beekeeper?> FindBeekeeperAsync(string? number)
        {
            number = number?.Trim();
            if (string.IsNullOrEmpty(number) || number.Length > 50)
                return null;
            var viaRegistry = _context.BeekeeperRegistry.Where(r => r.RegistryNumber == number && r.Status == RegistryStatus.Verified).Select(r => r.BeekeeperId);
            // D-78 (D78-SPEC 4.1 step 8): a beekeeper whose latest registration waits for (or failed) the review of its signed photo cannot deliver
            var notCleared = _context.BeekeeperRegistrations
                .Where(g => g.RegistrationId == _context.BeekeeperRegistrations.Where(x => x.BeekeeperId == g.BeekeeperId).Max(x => x.RegistrationId)
                            && g.EvidenceStatus != EvidenceStatus.Accepted)
                .Select(g => g.BeekeeperId);
            return await _context.Beekeepers.Include(b => b.Apiaries)
                .FirstOrDefaultAsync(b => b.IsActive && (b.BeekeeperBusinessNumber == number || viaRegistry.Contains(b.BeekeeperId)) && !notCleared.Contains(b.BeekeeperId));
        }

        public async Task<IActionResult> Index()
        {
            if (_current.IsPlatformAdmin)
                return RedirectToAction("Index", "OrgWorkspace");
            var active = await _current.GetAsync();
            if (active == null)
                return NotFound();
            if (!Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Intake, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, active.OrgRole))
            {
                TempData["Error"] = $"Your role ({active.OrgRole}) cannot open this page; roles: {string.Join(", ", Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Intake, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read))}.";
                return RedirectToAction("Index", "MyWork");
            }
            ViewBag.Intakes = await _context.LotIntakes.AsNoTracking()
                .Where(i => i.ReceivingOrgId == active.OrgId)
                .Include(i => i.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionApiary)
                .Include(i => i.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .OrderByDescending(i => i.IntakeId).Take(200).ToListAsync();
            // D-78 P3 (O-6): the beekeepers' asking prices for the roles allowed to read them (OA, CO by default)
            var prices = HttpContext.RequestServices.GetRequiredService<LotPriceReader>();
            if (await prices.RoleMayReadAsync())
                ViewBag.Prices = await prices.CurrentAsync(((List<LotIntake>)ViewBag.Intakes).Select(i => i.RawHoneyProductionId));
            ViewBag.Denied = await DeniedAsync();
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Index";
            return View();
        }

        public async Task<IActionResult> Create(string? number)
        {
            if (await DeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                var bk = await FindBeekeeperAsync(number);
                if (bk == null)
                    TempData["Error"] = "No active beekeeper with this number.";
                ViewBag.Beekeeper = bk;
                // D-52: the province comes from the registry number (TR + 2-digit plate code), typed or on record
                var places = HttpContext.RequestServices.GetRequiredService<Places>();
                var registry = bk == null ? null : await _context.BeekeeperRegistry.Where(r => r.BeekeeperId == bk.BeekeeperId && r.Status == RegistryStatus.Verified)
                    .Select(r => r.RegistryNumber).FirstOrDefaultAsync();
                ViewBag.Province = (places.FromRegistryNumber(number.Trim()) ?? places.FromRegistryNumber(registry))?.Name;
            }
            // D-78 P2: the delivery with sealed containers (Features:CustodyIntake)
            if (CustodyOn)
                return await CustodyFormAsync(new IntakeForm
                {
                    Number = number, DeliveryDate = DateOnly.FromDateTime(DateTime.Today), Province = ViewBag.Province as string,
                    OriginCountry = Origin.Default, RetainUntil = DateOnly.FromDateTime(DateTime.Today).AddYears(2),
                    Containers = { new ContainerRow() }
                }, ViewBag.Beekeeper as Beekeeper, codeSent: false);
            await FillListsAsync();
            ViewBag.Number = number;
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Create";
            return View();
        }

        // D-52: the province must be one of the 81 provinces and the district one of its districts
        private string? PlaceError(string province, string district)
        {
            var places = HttpContext.RequestServices.GetRequiredService<Places>();
            if (!places.Available)
                return null;
            var p = places.Province(province);
            if (p == null)
                return "Choose a province from the list.";
            return places.DistrictOf(p, district) == null ? "Choose a district of the selected province." : null;
        }

        private async Task FillListsAsync()
        {
            ViewBag.HoneyTypes = new SelectList(await _context.HoneyTypes.OrderBy(h => h.HoneyTypeTitle).ToListAsync(), "HoneyTypeId", "HoneyTypeTitle").Translated(HttpContext, "HoneyType", "Title");
            var today = DateOnly.FromDateTime(DateTime.Today);
            var periods = await _context.RawHoneyProductionPeriods.OrderByDescending(p => p.RawHoneyProductionPeriodStartDate).ToListAsync();
            var current = periods.FirstOrDefault(p => p.RawHoneyProductionPeriodStartDate <= today && (p.RawHoneyProductionPeriodEndDate == null || p.RawHoneyProductionPeriodEndDate >= today));
            ViewBag.Periods = new SelectList(periods, "RawHoneyProductionPeriodId", "RawHoneyProductionPeriodTitle", current?.RawHoneyProductionPeriodId).Translated(HttpContext, "ProductionPeriod", "Title");
            ViewBag.Origins = new SelectList(Origin.Countries.Select(c => new { c.Code, Name = Origin.Name(c.Code) + " (" + c.Code + ")" }), "Code", "Name", Origin.Default);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Create(string number, int apiaryId, int honeyTypeId, int periodId, DateOnly? deliveryDate,
            decimal? quantity, string? province, string? district, IFormFile? deliveryForm, bool sendSms, string? originCountry)
        {
            // D-78 P2: with sealed containers the delivery is recorded with Record (containers, jars, SMS confirmation)
            if (CustodyOn)
                return NotFound();
            if (await DeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var active = (await _current.GetAsync())!;
            var bk = await FindBeekeeperAsync(number);
            province = province?.Trim();
            district = district?.Trim();

            var fileError = deliveryForm == null ? null : await UploadValidator.ValidateDocumentAsync(deliveryForm);
            string? error = null;
            if (bk == null)
                error = "No active beekeeper with this number.";
            else if (bk.Apiaries.All(a => a.ApiaryId != apiaryId))
                error = "Choose one of the beekeeper's apiaries.";
            else if (!await _context.HoneyTypes.AnyAsync(h => h.HoneyTypeId == honeyTypeId))
                error = "Unknown honey type.";
            else if (!await _context.RawHoneyProductionPeriods.AnyAsync(p => p.RawHoneyProductionPeriodId == periodId))
                error = "The production period is required.";
            else if (deliveryDate == null || deliveryDate > DateOnly.FromDateTime(DateTime.Today))
                error = "Enter the delivery date (not in the future).";
            else if (quantity == null || quantity <= 0 || quantity > 1_000_000)
                error = "The amount must be greater than 0.";
            else if (string.IsNullOrEmpty(province) || string.IsNullOrEmpty(district) || province.Length > 100 || district.Length > 100)
                error = "Enter the province and district of origin.";
            else if (PlaceError(province, district) is string placeError)
                error = placeError;
            else if (!Origin.IsKnown(originCountry ?? Origin.Default))
                error = "Choose the country of origin.";
            else if (deliveryForm == null)
                error = "Attach the photo of the signed delivery form.";
            else if (fileError != null)
                error = fileError;

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Create), new { number });
            }

            // D-88 follow-up: several deliveries per apiary and period; the same honey type on the same day is pointed out after saving
            var sameDay = await SameDayLotAsync(apiaryId, honeyTypeId, deliveryDate!.Value);
            var (stored, sha) = await FileStore.SaveAsync(_env, IntakeForms, deliveryForm!);
            int newLotId;
            var org = await _context.Organizations.AsNoTracking().FirstAsync(o => o.OrgId == active.OrgId);
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                var lot = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = apiaryId,
                    RawHoneyProductionPeriodId = periodId,
                    RawHoneyProductionHoneyType = honeyTypeId,
                    RawHoneyProductionAmount = quantity,
                    RawHoneyProductionDate = deliveryDate,
                    RawHoneyProductionWholesalerId = org.LegacyWholesalerId,
                    OwnerOrgId = active.OrgId,
                    OriginCountry = originCountry ?? Origin.Default
                };
                _context.RawHoneyProductions.Add(lot);
                await _context.SaveChangesAsync();

                // Every control point that applies to raw lots is assigned at intake
                foreach (var ccpId in await _context.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.RawLot).Select(c => c.Id).ToListAsync())
                    _context.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                    {
                        RawHoneyProductionId = lot.RawHoneyProductionId,
                        RawHoneyCCPAssignmentCCP = ccpId,
                        RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned,
                        RawHoneyCCPAssignmentStartDate = DateTime.Now
                    });

                var intake = new LotIntake
                {
                    RawHoneyProductionId = lot.RawHoneyProductionId,
                    ReceivingOrgId = active.OrgId,
                    ReceivedByUserId = _current.UserId!,
                    DeliveryDate = deliveryDate!.Value,
                    Quantity = quantity!.Value,
                    Province = province!,
                    District = district!,
                    FormStoredFileName = stored,
                    FormOriginalFileName = Path.GetFileName(deliveryForm!.FileName),
                    FormSha256 = sha
                };
                if (sendSms)
                {
                    intake.SmsAt = DateTime.UtcNow;
                    var honey = HttpContext.RequestServices.GetRequiredService<RefText>().HoneyType(honeyTypeId,
                        await _context.HoneyTypes.Where(h => h.HoneyTypeId == honeyTypeId).Select(h => h.HoneyTypeTitle).FirstOrDefaultAsync());
                    intake.SmsStatus = string.IsNullOrWhiteSpace(bk!.BeekeeperPhoneNumber)
                        ? "NoPhoneNumber"
                        : await _sms.SendAsync(bk.BeekeeperPhoneNumber, $"TOP4HoneyChains: {quantity:0.##} kg {honey} {deliveryDate:dd.MM.yyyy} - {org.DisplayName}");
                }
                _context.LotIntakes.Add(intake);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = $"Delivery recorded as lot #{lot.RawHoneyProductionId}.";
                if (sameDay != null)
                    TempData["Info"] = $"Lot #{sameDay} is a delivery of the same honey type from this apiary on the same day. Check that this is another delivery.";
                newLotId = lot.RawHoneyProductionId;
            }
            catch
            {
                var path = FileStore.PathOf(_env, IntakeForms, stored);
                if (path != null)
                    System.IO.File.Delete(path);
                throw;
            }
            // D-42: the sample is taken at intake
            return RedirectToAction("Create", "Samples", new { lotId = newLotId });
        }

        // The delivery form: the receiving organization, owners of batches made from the lot, and
        // platform administrators
        public async Task<IActionResult> Form(int id)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == id);
            if (intake == null || !await MaySeeIntakeAsync(intake))
                return NotFound();
            // C2 (D-84): the signed form only for the receiving organization's sample officer, Compliance Officer and admin
            var access = HttpContext.RequestServices.GetRequiredService<EvidenceAccess>();
            var result = await access.CheckAsync(intake.ReceivingOrgId, OrgRoles.SampleAcceptanceOfficer);
            if (result != EvidenceAccess.Result.Allowed)
                return EvidenceAccess.Refuse(result == EvidenceAccess.Result.NotFound ? EvidenceAccess.Result.Forbidden : result);
            var path = FileStore.PathOf(_env, IntakeForms, intake.FormStoredFileName);
            if (path == null)
                return NotFound();
            await access.AuditAsync("LotIntakes", id.ToString(), intake.FormOriginalFileName);
            return PhysicalFile(path, FileStore.ContentTypeOf(path), intake.FormOriginalFileName);
        }

        private async Task<bool> MaySeeIntakeAsync(LotIntake intake)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            if (active == null)
                return false;
            if (intake.ReceivingOrgId == active.OrgId)
                return true;
            var batchIds = await _context.FinalHoneyProductionDetails.Where(d => d.RawHoneyProductionId == intake.RawHoneyProductionId)
                .Select(d => d.FinalHoneyProductionId).Distinct().ToListAsync();
            foreach (var b in batchIds)
                if (await _resp.BatchOwnerAsync(b) == active.OrgId)
                    return true;
            return false;
        }
    }
}
