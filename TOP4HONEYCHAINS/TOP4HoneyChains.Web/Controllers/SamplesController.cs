using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Samples (ROLE-MODEL section 7, D-42): the SampleAcceptanceOfficer of the lot's owner takes a sealed
    // sample at intake, keeps a sealed counter-sample and sends the sample to a verified laboratory; the
    // laboratory records receipt and checks the seal (a broken seal rejects the sample with a reason).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class SamplesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly LabService _lab;
        private LabCustodyService LabCustody => HttpContext.RequestServices.GetRequiredService<LabCustodyService>();
        // D-56: rights of organization roles (editable matrix)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();

        public SamplesController(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, LabService lab)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _lab = lab;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Controller = "Samples";
            ViewBag.Action = "Index";
            IQueryable<Sample> q = _context.Samples.AsNoTracking()
                .Include(s => s.TakingOrg).Include(s => s.LabOrg).Include(s => s.Report).Include(s => s.Container)
                .Include(s => s.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation);
            if (_current.IsPlatformAdmin)
            {
                ViewBag.Sent = await q.OrderByDescending(s => s.SampleId).Take(300).ToListAsync();
                ViewBag.Received = new List<Sample>();
                ViewBag.IsAdmin = true;
                return View();
            }
            var active = await _current.GetAsync();
            if (active == null)
                return NotFound();
            if (!Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, active.OrgRole))
            {
                TempData["Error"] = $"Your role ({active.OrgRole}) cannot open this page; roles: {string.Join(", ", Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read))}.";
                return RedirectToAction("Index", "MyWork");
            }
            ViewBag.Sent = await q.Where(s => s.TakingOrgId == active.OrgId).OrderByDescending(s => s.SampleId).Take(200).ToListAsync();
            ViewBag.Received = await q.Where(s => s.LabOrgId == active.OrgId).OrderBy(s => s.Status == SampleStatus.Sent ? 0 : 1)
                .ThenByDescending(s => s.SampleId).Take(200).ToListAsync();
            ViewBag.CanReceive = await _lab.LabDeniedAsync(active.OrgId, Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve)) == null;
            // D-78 P2: lots whose container jars are all received here and wait for the composite
            var ready = new List<int>();
            if (LabCustody.Mode == LabAnalysisMode.Composite)
                foreach (var lotId in await _context.Samples.Where(s => s.LabOrgId == active.OrgId && s.Kind == SampleKind.Container && s.Status == SampleStatus.Received)
                             .Select(s => s.RawHoneyProductionId).Distinct().ToListAsync())
                    if ((await LabCustody.CompositeSourcesAsync(lotId, active.OrgId)).Error == null)
                        ready.Add(lotId);
            ViewBag.CompositeReady = ready;
            ViewBag.AnalysisMode = LabCustody.Mode;
            return View();
        }

        private async Task<string?> TakeDeniedAsync(int lotId)
        {
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null || await _resp.LotOwnerAsync(lotId) != active.OrgId)
                return "Only the organization that holds the lot takes its samples.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot take samples; roles: {string.Join(", ", Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter))}.";
            return null;
        }

        private Task<List<Organization>> LabsAsync() =>
            _context.Organizations.AsNoTracking()
                .Where(o => !o.IsPlatform && o.Status == OrgStatus.Verified && o.Capabilities.Any(c => c.Capability == Capabilities.Laboratory))
                .Include(o => o.Capabilities)
                .OrderBy(o => o.DisplayName).ToListAsync();

        public async Task<IActionResult> Create(int lotId)
        {
            var lot = await _context.RawHoneyProductions.AsNoTracking()
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == lotId);
            if (lot == null)
                return NotFound();
            if (await TakeDeniedAsync(lotId) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            // D-78 P2: a delivery with containers has a jar per container; samples are taken on the delivery page
            if (await _context.LotIntakes.Where(i => i.RawHoneyProductionId == lotId && i.ContainersRecorded == true).Select(i => (int?)i.IntakeId).FirstOrDefaultAsync() is int intakeId)
            {
                TempData["Info"] = "Samples of this lot are taken per container on the delivery page.";
                return RedirectToAction("Details", "Intake", new { id = intakeId });
            }
            ViewBag.Lot = lot;
            ViewBag.Labs = await LabsAsync();
            ViewBag.Existing = await _context.Samples.AsNoTracking().Include(s => s.LabOrg).Where(s => s.RawHoneyProductionId == lotId).ToListAsync();
            ViewBag.Controller = "Samples";
            ViewBag.Action = "Create";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int lotId, int labOrgId, string? sealNumber, string? counterSealNumber, string? counterLocation, DateOnly? retainUntil)
        {
            if (!await _context.RawHoneyProductions.AnyAsync(r => r.RawHoneyProductionId == lotId))
                return NotFound();
            if (await _context.LotIntakes.AnyAsync(i => i.RawHoneyProductionId == lotId && i.ContainersRecorded == true))
            {
                TempData["Error"] = "Samples of this lot are taken per container on the delivery page.";
                return RedirectToAction(nameof(Index));
            }
            if (await TakeDeniedAsync(lotId) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            sealNumber = sealNumber?.Trim();
            counterSealNumber = counterSealNumber?.Trim();
            counterLocation = counterLocation?.Trim();
            var labs = await LabsAsync();
            string? error = null;
            if (labs.All(l => l.OrgId != labOrgId))
                error = "Choose a verified laboratory.";
            else if (string.IsNullOrEmpty(sealNumber) || string.IsNullOrEmpty(counterSealNumber) || sealNumber.Length > 50 || counterSealNumber.Length > 50)
                error = "Enter the seal numbers of the sample and of the counter-sample (at most 50 characters).";
            else if (sealNumber == counterSealNumber)
                error = "The sample and the counter-sample need different seals.";
            else if (string.IsNullOrEmpty(counterLocation) || counterLocation.Length > 200)
                error = "Enter where the counter-sample is kept (at most 200 characters).";
            else if (retainUntil == null || retainUntil <= DateOnly.FromDateTime(DateTime.Today))
                error = "Enter until when the counter-sample is kept (a future date).";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Create), new { lotId });
            }

            var active = (await _current.GetAsync())!;
            var count = await _context.Samples.CountAsync(s => s.RawHoneyProductionId == lotId);
            var sample = new Sample
            {
                SampleCode = $"S-{lotId}-{count + 1}",
                RawHoneyProductionId = lotId,
                TakingOrgId = active.OrgId,
                LabOrgId = labOrgId,
                SealNumber = sealNumber!,
                CounterSealNumber = counterSealNumber!,
                CounterLocation = counterLocation!,
                RetainUntil = retainUntil!.Value,
                TakenByUserId = _current.UserId!,
                TakenAt = DateTime.UtcNow
            };
            sample.Events.Add(new SampleCustodyEvent
            {
                Event = SampleStatus.Sent,
                FromOrgId = active.OrgId,
                ToOrgId = labOrgId,
                UserId = _current.UserId!,
                At = DateTime.UtcNow
            });
            _context.Samples.Add(sample);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Sample {sample.SampleCode} sent to the laboratory.";
            TempData["Scene"] = "sample";   // D-78 P4: the jar fills and its seal label wraps it
            return RedirectToAction(nameof(Index));
        }

        // The laboratory records receipt; a broken seal rejects the sample (reason required). D-78 P2: a container jar needs
        // three answers (seal number matches, label intact, cap intact); any "no" rejects it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, bool sealIntact, string? note, bool? sealMatches = null, bool? labelIntact = null, bool? capIntact = null)
        {
            var sample = await _context.Samples.FirstOrDefaultAsync(s => s.SampleId == id);
            if (sample == null)
                return NotFound();
            var active = await _current.GetAsync();
            if (!_current.IsPlatformAdmin && active?.OrgId != sample.LabOrgId)
                return NotFound();
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            string? error = await _lab.LabDeniedAsync(sample.LabOrgId, Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve));
            if (error == null && sample.Status != SampleStatus.Sent)
                error = "This sample was already received or rejected.";
            else if (error == null && sample.Kind == SampleKind.Container)
            {
                error = LabCustodyService.CheckReception(sealMatches, labelIntact, capIntact, note);
                sealIntact = sealMatches == true && labelIntact == true && capIntact == true;
            }
            else if (error == null && !sealIntact && note == null)
                error = "A broken seal rejects the sample: write the reason.";
            else if (error == null && note?.Length > 500)
                error = "The reason can be at most 500 characters.";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }
            sample.Status = sealIntact ? SampleStatus.Received : SampleStatus.Rejected;
            sample.SealIntact = sealIntact;
            sample.ReceivedByUserId = _current.UserId;
            sample.ReceivedAt = DateTime.UtcNow;
            sample.RejectReason = sealIntact ? null : note;
            if (sample.Kind == SampleKind.Container)
            {
                sample.SealNumberMatches = sealMatches;
                sample.LabelIntactAtReception = labelIntact;
                sample.JarCapIntactAtReception = capIntact;
            }
            _context.SampleCustodyEvents.Add(new SampleCustodyEvent
            {
                SampleId = sample.SampleId,
                Event = sample.Status,
                FromOrgId = sample.TakingOrgId,
                ToOrgId = sample.LabOrgId,
                UserId = _current.UserId!,
                At = DateTime.UtcNow,
                SealIntact = sealIntact,
                Note = note
            });
            await _context.SaveChangesAsync();
            if (sealIntact)
                TempData["Scene"] = "reception";   // D-78 P4: seal number, label and cap checked
            TempData[sealIntact ? "Success" : "Error"] = sample.Kind == SampleKind.Container
                ? (sealIntact ? "Jar received; seal number, label and cap are in order." : "Jar rejected: the plant takes a new jar from the same container.")
                : sealIntact ? "Sample received; the seal is intact." : "Sample rejected: the seal is broken.";
            return RedirectToAction(nameof(Index));
        }

        // D-78 P2 (D78-SPEC 4.6 step 4): the laboratory mixes the received jars of a lot into one composite sample
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Composite(int lotId)
        {
            var active = await _current.GetAsync();
            if (_current.IsPlatformAdmin || active == null)
                return NotFound();
            var (jars, error) = await LabCustody.CompositeSourcesAsync(lotId, active.OrgId);
            error ??= await _lab.LabDeniedAsync(active.OrgId, Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve));
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }
            var composite = await LabCustody.CreateCompositeAsync(lotId, jars, active.OrgId, _current.UserId!);
            TempData["Success"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Composite sample {0} made from {1} jars; enter its results.", composite.SampleCode, jars.Count);
            return RedirectToAction("Edit", "LabReports", new { id = composite.SampleId });
        }

        // D-78 P2 (D78-SPEC 4.6 step 3): after a rejected jar the plant takes a new jar from the same container
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resample(int containerId, string? jarSeal, string? counterSeal, bool capIntact, string? reseal)
        {
            var container = await _context.Containers.AsNoTracking().FirstOrDefaultAsync(c => c.ContainerId == containerId);
            if (container == null)
                return NotFound();
            if (await TakeDeniedAsync(container.RawHoneyProductionId) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var (jar, error) = await LabCustody.ResampleAsync(containerId, jarSeal, counterSeal, capIntact, reseal, _current.UserId!);
            if (error != null)
                TempData["Error"] = error;
            else
            {
                TempData["Success"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, "New jar {0} sent to the laboratory; the container was sealed again.", jar!.SampleCode);
                TempData["Scene"] = "sample";
            }
            return RedirectToAction("Details", "Intake", new { id = container.IntakeId });
        }
    }
}
