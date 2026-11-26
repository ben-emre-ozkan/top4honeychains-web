using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Versioning;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Models.ViewModels.HoneyStory;

namespace TOP4HoneyChains.Web.Controllers
{
    // Public pages (D-47)
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [Route("honeystory")]
    public class HoneyStoryController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly Microsoft.Extensions.Options.IOptions<Services.OrgModel.FeatureFlags> _flags;
        private readonly CcpIntegrityService _ccp;
        private readonly Services.OrgModel.StorefrontService _storefronts;

        public HoneyStoryController(TOP4HoneyChainsContext context, Microsoft.Extensions.Options.IOptions<Services.OrgModel.FeatureFlags> flags,
            CcpIntegrityService ccp, Services.OrgModel.StorefrontService storefronts)
        {
            _context = context;
            _flags = flags;
            _ccp = ccp;
            _storefronts = storefronts;
        }

        // P4 badges (D-42), only with the organization model and only from approved, verified data:
        // every lot tested by an accredited, verified laboratory; an in-house lab (the lab belongs to the
        // producer); every CCP approved; every value with a verified limit within it
        private async Task FillBadgesAsync(HoneyStoryViewModel vm)
        {
            if (!_flags.Value.OrgRoleModel)
                return;
            var rawIds = vm.RawSources.Select(s => s.RawHoneyProductionId).Distinct().ToList();
            var reports = await _context.LabReports.AsNoTracking()
                .Where(r => r.Status == LabReportStatus.Approved && rawIds.Contains(r.Sample!.RawHoneyProductionId) && r.Sample.FinalHoneyProductionId == null
                            && r.LabOrg!.Status == OrgStatus.Verified)
                .Select(r => new { r.Sample!.RawHoneyProductionId, r.IsAccredited, r.AccreditationNumber, r.InHouse, r.ReportCode, Lab = r.LabOrg!.DisplayName, r.ApprovedAt })
                .ToListAsync();
            // the latest approved report of each lot, linked to the verification page
            foreach (var source in vm.RawSources)
            {
                var report = reports.Where(r => r.RawHoneyProductionId == source.RawHoneyProductionId).OrderByDescending(r => r.ApprovedAt).FirstOrDefault();
                source.LabReportCode = report?.ReportCode;
                source.LabName = report?.Lab;
                source.LabAccredited = report?.IsAccredited == true;
                source.LabApprovedAt = report?.ApprovedAt;
            }
            // D-98: the final honey result of the batch (latest approved report of a batch sample)
            var final = await _context.LabReports.AsNoTracking()
                .Where(r => r.Status == LabReportStatus.Approved && r.Sample!.FinalHoneyProductionId == vm.FinalHoneyProductionId && r.LabOrg!.Status == OrgStatus.Verified)
                .Include(r => r.LabOrg).Include(r => r.Results).ThenInclude(x => x.Parameter)
                .OrderByDescending(r => r.ApprovedAt).FirstOrDefaultAsync();
            if (final != null)
                vm.FinalResult = new HoneyStoryFinalResult
                {
                    ReportCode = final.ReportCode ?? "",
                    LabName = final.LabOrg?.DisplayName,
                    ApprovedAt = final.ApprovedAt,
                    Accredited = final.IsAccredited,
                    Values = final.Results.OrderBy(x => x.ParameterId)
                        .Select(x => (x.ParameterId, x.Parameter?.ParameterName, x.MeasuredNumericValue, x.MeasuredTextValue, x.Parameter?.ParameterUnit,
                            x.Parameter?.LimitVerified == true ? x.IsInRange : null, x.Parameter?.LimitVerified == true ? x.AppliedLimit : null))
                        .ToList()
                };
            var rows = vm.RawSources.SelectMany(s => s.CcpBlocks).Concat(vm.BatchCcpBlocks).SelectMany(b => b.Parameters)
                .Where(r => r.Compliant != null).ToList();
            vm.Badges = new HoneyStoryBadges
            {
                AccreditedLab = rawIds.Count > 0 && rawIds.All(id => reports.Any(r => r.RawHoneyProductionId == id && r.IsAccredited)),
                AccreditationNumbers = reports.Where(r => r.IsAccredited && r.AccreditationNumber != null).Select(r => r.AccreditationNumber!).Distinct().ToList(),
                InHouseLab = reports.Any(r => r.InHouse),
                AllCcpsApproved = (await _ccp.GetFinalizeBlockersAsync(vm.FinalHoneyProductionId)).Count == 0,
                LimitsCompliant = rows.Count > 0 && rows.All(r => r.Compliant == true),
                ReportCodes = reports.Select(r => r.ReportCode).Distinct().ToList()
            };

            // P5 (D-31): stored origin shares; batches finalized before P5 are computed from their composition
            var stored = await _context.BatchOriginShares.AsNoTracking().Where(s => s.FinalHoneyProductionId == vm.FinalHoneyProductionId).ToListAsync();
            var shares = stored.Count > 0
                ? stored.Select(s => new Origin.Share(s.CountryCode, s.Amount, s.Percentage)).ToList()
                : Origin.Compute(await _context.FinalHoneyProductionDetails.AsNoTracking().Where(d => d.FinalHoneyProductionId == vm.FinalHoneyProductionId)
                    .Select(d => new ValueTuple<string?, decimal>(d.RawHoneyProduction.OriginCountry, d.UsedAmount)).ToListAsync());
            vm.Badges.Origins = shares.OrderByDescending(s => s.Percentage).Select(s => new HoneyStoryOrigin(Origin.Name(s.Country), s.Percentage)).ToList();

            // P5 (D-43): links to the published storefronts of the contributing beekeepers
            var owners = await _context.RawHoneyProductions.AsNoTracking().Where(r => rawIds.Contains(r.RawHoneyProductionId) && r.RawHoneyProductionApiary != null)
                .Select(r => new { r.RawHoneyProductionId, r.RawHoneyProductionApiary!.ApiaryBeekeeperId }).ToListAsync();
            var codes = await _storefronts.PublishedCodesAsync(owners.Select(o => o.ApiaryBeekeeperId));
            foreach (var source in vm.RawSources)
            {
                var owner = owners.FirstOrDefault(o => o.RawHoneyProductionId == source.RawHoneyProductionId);
                source.StorefrontCode = owner == null ? null : codes.GetValueOrDefault(owner.ApiaryBeekeeperId);
            }
        }


        [HttpGet("parameter/{id}")]
        public async Task<IActionResult> ParameterDetails(int id, Guid? qr, int? lot)
        {
            var parameter = await _context.CriticalControlPointParameters.AsNoTracking()
                .Include(p => p.CCP)
                .Include(p => p.ParameterCategoryNavigation)
                .Include(p => p.Limits)
                .Include(p => p.CriticalControlPointParameterContents.Where(c => c.IsActive))
                .FirstOrDefaultAsync(p => p.ParameterId == id);
            if (parameter == null)
                return NotFound();

            var vm = new HoneyStoryParameterViewModel
            {
                Parameter = parameter,
                Content = parameter.CriticalControlPointParameterContents.OrderBy(c => c.SortOrder).FirstOrDefault()
            };

            // "Read together" (mobile): the paired parameter of the same CCP
            var pair = HoneyStoryLab.Pairs.Where(x => string.Equals(x.A, parameter.ParameterName, StringComparison.OrdinalIgnoreCase)
                                                      || string.Equals(x.B, parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                .Select(x => string.Equals(x.A, parameter.ParameterName, StringComparison.OrdinalIgnoreCase) ? x.B : x.A).ToList();
            if (pair.Count > 0)
                vm.Related = await _context.CriticalControlPointParameters.AsNoTracking()
                    .Where(p => p.CCPId == parameter.CCPId && pair.Contains(p.ParameterName!)).ToListAsync();

            // D-112: opened from the Honey Story of a test batch
            if (qr != null && await _context.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionQR == qr && _context.Organizations.Any(o => o.OrgId == f.OwnerOrgId && o.IsTestData)))
                Services.OrgModel.TestData.MarkPage(HttpContext);

            // "In this jar": the value of one lot of a published batch, only through its Honey Story link (D-12)
            if (qr != null && lot != null)
            {
                var inBatch = await _context.FinalHoneyProductionDetails.AsNoTracking()
                    .Where(d => d.FinalHoneyProduction.FinalHoneyProductionQR == qr && d.FinalHoneyProduction.FinalHoneyProductionIsFinalized
                                && d.RawHoneyProductionId == lot)
                    .Select(d => new { d.RawHoneyProduction.RawHoneyProductionHoneyType, d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation!.HoneyClass,
                                       Apiary = d.RawHoneyProduction.RawHoneyProductionApiary!.ApiaryTitle,
                                       d.RawHoneyProduction.RawHoneyProductionApiary!.TitleSource,
                                       d.RawHoneyProduction.RawHoneyProductionApiary!.ApiaryBeekeeperId })
                    .FirstOrDefaultAsync();
                if (inBatch != null)
                {
                    vm.Qr = qr;
                    vm.Lot = lot;
                    var jarNames = await HttpContext.RequestServices.GetRequiredService<Services.OrgModel.PublicNameService>().NamesAsync(new[] { inBatch.ApiaryBeekeeperId });
                    vm.JarSource = Services.OrgModel.PublicNameService.PublicApiaryTitle(inBatch.Apiary, inBatch.TitleSource,
                        jarNames.TryGetValue(inBatch.ApiaryBeekeeperId, out var jarName) && jarName.IsName);
                    var logs = await _context.CriticalControlPointMonitoringLogs.AsNoTracking()
                        .Where(l => l.RawHoneyProductionId == lot && l.CCPId == parameter.CCPId).ToListAsync();
                    var log = logs.FirstOrDefault(l => l.ParameterId == id);
                    if (log?.MeasuredNumericValue != null)
                    {
                        var row = new HoneyStoryCCPRow { ParameterId = id, ParameterName = parameter.ParameterName, Unit = parameter.ParameterUnit, NumericValue = log.MeasuredNumericValue };
                        if (parameter.LimitVerified)
                        {
                            var siblings = logs.Where(l => l.ParameterId != null && l.MeasuredNumericValue != null)
                                .GroupBy(l => l.ParameterId!.Value).ToDictionary(g => g.Key, g => g.First().MeasuredNumericValue!.Value);
                            var honey = new HoneyProfile(inBatch.RawHoneyProductionHoneyType, inBatch.HoneyClass);
                            var result = log.IsInRange != null ? new CcpLimitEvaluator.Result(log.IsInRange, log.AppliedLimit)
                                : CcpLimitEvaluator.Evaluate(parameter, log.MeasuredNumericValue, siblings, honey);
                            var bounds = CcpLimitEvaluator.MainBounds(parameter, siblings, honey);
                            row.Compliant = result.IsInRange;
                            row.LimitText = result.AppliedLimit;
                            row.LimitMin = bounds?.Min;
                            row.LimitMax = bounds?.Max;
                        }
                        vm.InJar = row;
                    }
                }
            }
            return View(vm);
        }

        [HttpGet("{qr}")]
        public async Task<IActionResult> Index(Guid qr)
        {
            // 1) Batch
            var batch = await _context.FinalHoneyProductions
                .AsNoTracking()
                .Include(f => f.FinalHoneyProductionWholesaler)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionApiary)
                            .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionQR == qr);

            // D-12: only finalized (published) batches have a public page
            if (batch == null || !batch.FinalHoneyProductionIsFinalized)
                return NotFound();
            // D-112: a batch of the test set shows the TEST banner
            if (await Services.OrgModel.TestData.IsTestOrgAsync(_context, batch.OwnerOrgId))
                Services.OrgModel.TestData.MarkPage(HttpContext);

            var vm = new HoneyStoryViewModel
            {
                FinalHoneyProductionId = batch.FinalHoneyProductionId,
                BatchTitle = batch.FinalHoneyProductionTitle,
                BatchQR = batch.FinalHoneyProductionQR,
                ProductionDate = batch.FinalHoneyProductionDate,
                TotalAmount = batch.FinalHoneyProductionTotalAmount,
                WholesalerName = batch.FinalHoneyProductionWholesaler?.WholesalerTitle
            };

            // 2) Raw sources
            // Aynı raw honey birden fazla detay olmasın diye (genelde zaten olmaz ama garanti)
            var details = batch.FinalHoneyProductionDetails
                .GroupBy(d => d.RawHoneyProductionId)
                .Select(g => new
                {
                    RawHoneyProductionId = g.Key,
                    UsedAmount = g.Sum(x => x.UsedAmount),
                    Raw = g.First().RawHoneyProduction
                })
                .ToList();

            // 2a) Apiary photo’larını tek seferde çekelim
            var apiaryIds = details
                .Select(x => x.Raw?.RawHoneyProductionApiaryId)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var photos = await _context.ApiaryPhotos
                .AsNoTracking()
                .Where(p => apiaryIds.Contains(p.ApiaryPhotoApiaryId))
                .OrderByDescending(p => p.ApiaryPhotoId)
                .Select(p => new
                {
                    p.ApiaryPhotoId,
                    p.ApiaryPhotoApiaryId,
                    p.ApiaryPhotoFileName,
                    p.ApiaryPhotoDesc
                })
                .ToListAsync();


            // D-78 P3 (C5, O-8): the name only with the beekeeper's consent, otherwise the neutral code
            var publicNames = await HttpContext.RequestServices.GetRequiredService<Services.OrgModel.PublicNameService>()
                .NamesAsync(details.Select(d => d.Raw?.RawHoneyProductionApiary?.ApiaryBeekeeperId).OfType<string>());

            // 4) VM doldur
            foreach (var d in details)
            {
                var raw = d.Raw;
                if (raw == null) continue;

                var apiary = raw.RawHoneyProductionApiary;
                var beekeeper = apiary?.ApiaryBeekeeper;
                var nameShown = beekeeper?.BeekeeperId is string shownId && publicNames.TryGetValue(shownId, out var publicName) && publicName.IsName;

                var source = new HoneyStoryRawSource
                {
                    RawHoneyProductionId = d.RawHoneyProductionId,
                    ApiaryId = raw.RawHoneyProductionApiaryId,
                    ApiaryName = Services.OrgModel.PublicNameService.PublicApiaryTitle(apiary?.ApiaryTitle, apiary?.TitleSource, nameShown),
                    BeekeeperName = beekeeper?.BeekeeperId is string bkId && publicNames.TryGetValue(bkId, out var shown) ? shown.Text : null,
                    HoneyType = raw.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                    UsedAmount = d.UsedAmount,
                    HarvestedOn = raw.RawHoneyProductionDate,
                    // D-12, D-72: only the province leaves the server, computed here from the apiary's position;
                    // the beekeeper's photo and phone are not shown publicly
                    ProvincePlate = apiary == null ? 0 : TurkeyMap.PlateAt((double)apiary.ApiaryLocationLatitude, (double)apiary.ApiaryLocationLongitude)
                };

                // Photos
                if (raw.RawHoneyProductionApiaryId != null)
                {
                    source.Photos = photos
                        .Where(p => p.ApiaryPhotoApiaryId == raw.RawHoneyProductionApiaryId.Value)
                        .Select(p => new HoneyStoryPhoto
                        {
                            ApiaryPhotoId = p.ApiaryPhotoId,
                            FileName = p.ApiaryPhotoFileName,
                            Description = Services.OrgModel.PublicNameService.PublicPhotoText(p.ApiaryPhotoDesc, nameShown)
                        })
                        .ToList();
                }

                vm.RawSources.Add(source);
            }

            await FillCcpBlocksAsync(vm);
            await FillBadgesAsync(vm);
            await FillJourneyAsync(vm, batch);
            await FillDistrictsAsync(vm);
            await FillProtectionAsync(vm);
            await FillParameterContentsAsync(vm);

            return View(vm);
        }


        [HttpGet("qrcode")]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> BatchQRCode(string code)
        {
            // Only for real batches (not a general QR generator for any text)
            if (!Guid.TryParse(code, out var qr) || !await _context.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionQR == qr))
                return NotFound();

            // D-11: the QR code holds the full public address of the Honey Story page, so a phone opens it directly
            var url = PublicUrl.Build(HttpContext, Url.Action(nameof(Index), new { qr })!)
                ?? throw new InvalidOperationException("App:PublicBaseUrl is not configured; QR codes need the public site address.");

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(
                pixelsPerModule: 20,
                darkColor: Color.Black,
                lightColor: Color.White,
                drawQuietZones: true);
            using var ms = new MemoryStream();
            qrCodeImage.Save(ms, ImageFormat.Png);

            ms.Position = 0;

            return File(ms.ToArray(), "image/png");
        }


        // D-12, D-75: an apiary photo for the public Honey Story of one finalized batch: only photos of that batch's source
        // apiaries (the photos the page shows), and re-encoded without metadata, so no GPS position or camera data leaves
        // the server. Any other photo answers 404.
        [HttpGet("photo/{id:int}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Image(int id, Guid qr, [FromServices] Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            var apiaryId = await _context.ApiaryPhotos.AsNoTracking().Where(x => x.ApiaryPhotoId == id)
                .Select(x => (int?)x.ApiaryPhotoApiaryId).FirstOrDefaultAsync();
            if (apiaryId == null || qr == Guid.Empty)
                return NotFound();
            var isSource = await _context.FinalHoneyProductionDetails.AnyAsync(d =>
                d.FinalHoneyProduction.FinalHoneyProductionQR == qr &&
                d.FinalHoneyProduction.FinalHoneyProductionIsFinalized &&
                d.RawHoneyProduction.RawHoneyProductionApiaryId == apiaryId);
            if (!isSource)
                return NotFound();

            var clean = await Microsoft.Extensions.Caching.Memory.CacheExtensions.GetOrCreateAsync(cache, $"hs-photo:{id}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(6);
                var data = await _context.ApiaryPhotos.AsNoTracking().Where(x => x.ApiaryPhotoId == id).Select(x => x.ApiaryPhotoData).FirstOrDefaultAsync();
                return data == null ? null : PublicPhoto(data);
            });
            return clean == null ? NotFound() : File(clean, "image/jpeg");
        }

        // a JPEG copy without EXIF, IPTC, XMP or ICC data; null when the stored bytes are not an image
        public static byte[]? PublicPhoto(byte[] data)
        {
            return ImageTools.CleanJpeg(data, 85);
        }


        [HttpGet("details/{qr}")]
        public async Task<IActionResult> Details(Guid qr)
        {
            // 1) Batch
            var batch = await _context.FinalHoneyProductions
                .AsNoTracking()
                .Include(f => f.FinalHoneyProductionWholesaler)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionApiary)
                            .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionQR == qr);

            // D-12: only finalized (published) batches have a public page
            if (batch == null || !batch.FinalHoneyProductionIsFinalized)
                return NotFound();
            // D-112: a batch of the test set shows the TEST banner
            if (await Services.OrgModel.TestData.IsTestOrgAsync(_context, batch.OwnerOrgId))
                Services.OrgModel.TestData.MarkPage(HttpContext);

            var vm = new HoneyStoryViewModel
            {
                FinalHoneyProductionId = batch.FinalHoneyProductionId,
                BatchTitle = batch.FinalHoneyProductionTitle,
                BatchQR = batch.FinalHoneyProductionQR,
                ProductionDate = batch.FinalHoneyProductionDate,
                TotalAmount = batch.FinalHoneyProductionTotalAmount,
                WholesalerName = batch.FinalHoneyProductionWholesaler?.WholesalerTitle
            };

            // 2) Raw sources
            // Aynı raw honey birden fazla detay olmasın diye (genelde zaten olmaz ama garanti)
            var details = batch.FinalHoneyProductionDetails
                .GroupBy(d => d.RawHoneyProductionId)
                .Select(g => new
                {
                    RawHoneyProductionId = g.Key,
                    UsedAmount = g.Sum(x => x.UsedAmount),
                    Raw = g.First().RawHoneyProduction
                })
                .ToList();

            // 2a) Apiary photo’larını tek seferde çekelim
            var apiaryIds = details
                .Select(x => x.Raw?.RawHoneyProductionApiaryId)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var photos = await _context.ApiaryPhotos
                .AsNoTracking()
                .Where(p => apiaryIds.Contains(p.ApiaryPhotoApiaryId))
                .OrderByDescending(p => p.ApiaryPhotoId)
                .Select(p => new
                {
                    p.ApiaryPhotoId,
                    p.ApiaryPhotoApiaryId,
                    p.ApiaryPhotoFileName,
                    p.ApiaryPhotoDesc
                })
                .ToListAsync();

            // D-78 P3 (C5, O-8): the name only with the beekeeper's consent, otherwise the neutral code
            var publicNames = await HttpContext.RequestServices.GetRequiredService<Services.OrgModel.PublicNameService>()
                .NamesAsync(details.Select(d => d.Raw?.RawHoneyProductionApiary?.ApiaryBeekeeperId).OfType<string>());

            // 4) VM doldur
            foreach (var d in details)
            {
                var raw = d.Raw;
                if (raw == null) continue;

                var apiary = raw.RawHoneyProductionApiary;
                var beekeeper = apiary?.ApiaryBeekeeper;
                var nameShown = beekeeper?.BeekeeperId is string shownId && publicNames.TryGetValue(shownId, out var publicName) && publicName.IsName;

                var source = new HoneyStoryRawSource
                {
                    RawHoneyProductionId = d.RawHoneyProductionId,
                    ApiaryId = raw.RawHoneyProductionApiaryId,
                    ApiaryName = Services.OrgModel.PublicNameService.PublicApiaryTitle(apiary?.ApiaryTitle, apiary?.TitleSource, nameShown),
                    BeekeeperName = beekeeper?.BeekeeperId is string bkId && publicNames.TryGetValue(bkId, out var shown) ? shown.Text : null,
                    HoneyType = raw.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                    UsedAmount = d.UsedAmount
                };

                // Photos
                if (raw.RawHoneyProductionApiaryId != null)
                {
                    source.Photos = photos
                        .Where(p => p.ApiaryPhotoApiaryId == raw.RawHoneyProductionApiaryId.Value)
                        .Select(p => new HoneyStoryPhoto
                        {
                            ApiaryPhotoId = p.ApiaryPhotoId,
                            FileName = p.ApiaryPhotoFileName,
                            Description = Services.OrgModel.PublicNameService.PublicPhotoText(p.ApiaryPhotoDesc, nameShown)
                        })
                        .ToList();
                }

                vm.RawSources.Add(source);
            }

            await FillCcpBlocksAsync(vm);
            await FillBadgesAsync(vm);

            return View(vm);
        }





    
        // Every CCP that applies to the lots and to the batch, with its state; missing CCPs and
        // parameters are listed as "not recorded" instead of being hidden (D-16). Compliance is shown
        // only for parameters whose limits are verified (D-18).
        private async Task FillCcpBlocksAsync(HoneyStoryViewModel vm)
        {
            var rawIds = vm.RawSources.Select(s => s.RawHoneyProductionId).Distinct().ToList();
            var batchId = vm.FinalHoneyProductionId;

            var ccps = await _context.CriticalControlPoints.AsNoTracking()
                .Include(c => c.CriticalControlPointParameters).ThenInclude(p => p.Limits).ThenInclude(l => l.ConditionParameter)
                .Include(c => c.CriticalControlPointParameters).ThenInclude(p => p.Limits).ThenInclude(l => l.SumWithParameter)
                .Include(c => c.CriticalControlPointParameters).ThenInclude(p => p.Limits).ThenInclude(l => l.RatioWithParameter)
                .Include(c => c.CriticalControlPointParameters).ThenInclude(p => p.Limits).ThenInclude(l => l.HoneyType)
                .OrderBy(c => c.Id)
                .ToListAsync();
            var rawAssignments = await _context.RawHoneyCCPAssignments.AsNoTracking()
                .Where(a => rawIds.Contains(a.RawHoneyProductionId))
                .ToListAsync();
            var batchAssignments = await _context.FinalHoneyCCPAssignments.AsNoTracking()
                .Where(a => a.FinalHoneyProductionId == batchId)
                .ToListAsync();
            var logs = await _context.CriticalControlPointMonitoringLogs.AsNoTracking()
                .Where(l => (l.RawHoneyProductionId != null && rawIds.Contains(l.RawHoneyProductionId.Value))
                            || l.FinalHoneyProductionId == batchId)
                .ToListAsync();
            var classes = await _context.RawHoneyProductions.AsNoTracking()
                .Where(r => rawIds.Contains(r.RawHoneyProductionId))
                .Select(r => new { r.RawHoneyProductionId, r.RawHoneyProductionHoneyType, r.RawHoneyProductionHoneyTypeNavigation!.HoneyClass })
                .ToListAsync();

            foreach (var source in vm.RawSources)
            {
                var lot = classes.FirstOrDefault(c => c.RawHoneyProductionId == source.RawHoneyProductionId);
                var honey = lot == null ? HoneyProfile.Unknown : new HoneyProfile(lot.RawHoneyProductionHoneyType, lot.HoneyClass);
                var own = rawAssignments.Where(a => a.RawHoneyProductionId == source.RawHoneyProductionId).ToList();
                source.CcpBlocks = ccps
                    .Where(c => c.AppliesTo == CcpAppliesTo.RawLot || own.Any(a => a.RawHoneyCCPAssignmentCCP == c.Id))
                    // D-98: control points that do not apply on the association path are not shown
                    .Where(c => !own.Any(a => a.RawHoneyCCPAssignmentCCP == c.Id && a.NotApplicableReason == TOP4HoneyChains.Web.Services.OrgModel.CcpPaths.Reason))
                    .Select(c =>
                    {
                        var a = own.FirstOrDefault(x => x.RawHoneyCCPAssignmentCCP == c.Id);
                        return BuildCcpBlock(c, a?.RawHoneyCCPAssignmentStatus, a?.NotApplicableReason,
                            logs.Where(l => l.RawHoneyProductionId == source.RawHoneyProductionId && l.CCPId == c.Id).ToList(), honey);
                    })
                    .ToList();
            }

            var batchTypes = classes.Select(c => c.RawHoneyProductionHoneyType).Distinct().ToList();
            var batchClasses = classes.Select(c => c.HoneyClass).Distinct().ToList();
            var batchHoney = new HoneyProfile(batchTypes.Count == 1 ? batchTypes[0] : null, batchClasses.Count == 1 ? batchClasses[0] : null);
            vm.BatchCcpBlocks = ccps
                .Where(c => c.AppliesTo == CcpAppliesTo.Batch || batchAssignments.Any(a => a.FinalHoneyCCPAssignmentCCP == c.Id))
                .Where(c => !batchAssignments.Any(a => a.FinalHoneyCCPAssignmentCCP == c.Id && a.NotApplicableReason == TOP4HoneyChains.Web.Services.OrgModel.CcpPaths.Reason))
                .Select(c =>
                {
                    var a = batchAssignments.FirstOrDefault(x => x.FinalHoneyCCPAssignmentCCP == c.Id);
                    return BuildCcpBlock(c, a?.FinalHoneyCCPAssignmentStatus, a?.NotApplicableReason,
                        logs.Where(l => l.FinalHoneyProductionId == batchId && l.CCPId == c.Id).ToList(), batchHoney);
                })
                .ToList();
        }

        private static HoneyStoryCcpBlock BuildCcpBlock(CriticalControlPoint ccp, int? status, string? notApplicableReason,
            List<CriticalControlPointMonitoringLog> logs, HoneyProfile honey)
        {
            var block = new HoneyStoryCcpBlock
            {
                CCPId = ccp.Id,
                CCPCode = ccp.Code,
                CCPTitle = ccp.Title,
                ProcessingStep = ccp.ProcessingStep
            };

            if (status == AssignmentStatusIds.NotApplicable)
            {
                block.State = HoneyStoryCcpBlock.NotApplicable;
                block.NotApplicableReason = notApplicableReason;
                return block;
            }
            if (status == null || logs.Count == 0)
            {
                block.State = HoneyStoryCcpBlock.NotRecorded;
                return block;
            }

            block.State = status == AssignmentStatusIds.Completed ? HoneyStoryCcpBlock.Recorded : HoneyStoryCcpBlock.Incomplete;
            var siblings = logs.Where(l => l.ParameterId != null && l.MeasuredNumericValue != null)
                .GroupBy(l => l.ParameterId!.Value)
                .ToDictionary(g => g.Key, g => g.First().MeasuredNumericValue!.Value);

            foreach (var p in ccp.CriticalControlPointParameters.OrderBy(p => p.ParameterId))
            {
                var log = logs.FirstOrDefault(l => l.ParameterId == p.ParameterId);

                // D-12/D-20: without PublicTextValues only Yes/No and numbers are published; free text
                // (typed or untyped) and files are left out entirely
                if (!ccp.PublicTextValues)
                {
                    var isText = p.DataType is CcpDataType.Text or CcpDataType.File
                                 || (p.DataType == null && log?.MeasuredTextValue != null);
                    if (isText)
                        continue;
                }
                bool? compliant = null;
                string? limitText = null;
                (decimal? Min, decimal? Max)? bounds = null;
                if (log != null && p.LimitVerified)
                {
                    // The result stored at entry time; values recorded before limits existed are checked now
                    var result = log.IsInRange != null
                        ? new CcpLimitEvaluator.Result(log.IsInRange, log.AppliedLimit)
                        : CcpLimitEvaluator.Evaluate(p, log.MeasuredNumericValue, siblings, honey);
                    compliant = result.IsInRange;
                    limitText = result.AppliedLimit;
                    bounds = CcpLimitEvaluator.MainBounds(p, siblings, honey);
                }

                var isFile = p.DataType == CcpDataType.File;
                block.Parameters.Add(new HoneyStoryCCPRow
                {
                    CCPId = ccp.Id,
                    CCPCode = ccp.Code,
                    CCPTitle = ccp.Title,
                    ParameterId = p.ParameterId,
                    ParameterName = p.ParameterName,
                    Unit = p.ParameterUnit == "None" ? null : p.ParameterUnit,
                    NumericValue = log?.MeasuredNumericValue,
                    TextValue = isFile ? null : log?.MeasuredTextValue,   // documents are not published
                    IsFile = isFile && log != null,
                    MeasurementDate = log?.MeasurementDate,
                    Recorded = log != null,
                    Compliant = compliant,
                    LimitText = limitText,
                    LimitMin = bounds?.Min,
                    LimitMax = bounds?.Max
                });
            }
            return block;
        }

        // D-72: the batch's real steps for the journey (harvest, delivery, laboratory, processing, shelf) and the province of
        // each source: the province declared at delivery, else the apiary's (computed above), else the enterprise registry's.
        // Deliveries, laboratory reports and the shelf exist only with the organization model; without it they show empty states.
        // D-98: the district of each source, within its province: the apiary's district (DistrictId), else the district
        // declared at delivery, matched by name on the district map; unknown otherwise (the page then shows the province)
        private async Task FillDistrictsAsync(HoneyStoryViewModel vm)
        {
            var sources = vm.RawSources.Where(s => s.ProvincePlate > 0).ToList();
            if (sources.Count == 0)
                return;
            var apiaryIds = sources.Select(s => s.ApiaryId).OfType<int>().Distinct().ToList();
            var apiaryDistricts = await _context.Apiaries.AsNoTracking()
                .Where(a => apiaryIds.Contains(a.ApiaryId) && a.DistrictId != null)
                .Join(_context.Districts, a => a.DistrictId, d => d.DistrictId, (a, d) => new { a.ApiaryId, d.ProvinceCode, d.Name })
                .ToListAsync();
            var rawIds = sources.Select(s => s.RawHoneyProductionId).ToList();
            var declared = await _context.LotIntakes.AsNoTracking().Where(i => rawIds.Contains(i.RawHoneyProductionId))
                .Select(i => new { i.RawHoneyProductionId, i.Province, i.District }).ToListAsync();
            foreach (var s in sources)
            {
                var own = apiaryDistricts.FirstOrDefault(a => a.ApiaryId == s.ApiaryId);
                var fromApiary = own != null && int.TryParse(own.ProvinceCode, out var plate) && plate == s.ProvincePlate
                    ? TurkeyDistricts.Find(plate, own.Name) : null;
                var atDelivery = declared.Where(i => i.RawHoneyProductionId == s.RawHoneyProductionId && Provinces.PlateOf(i.Province) == s.ProvincePlate)
                    .Select(i => TurkeyDistricts.Find(s.ProvincePlate, i.District)).FirstOrDefault(d => d != null);
                s.District = (fromApiary ?? atDelivery)?.Name;
            }
        }

        // D-108: how the honey was protected, as counts (never codes): the delivered containers and how many were sealed, the
        // sealed sample jars (lots and the final blend) and how many arrived at the laboratory with the seal intact, the deliveries
        // and how many the beekeeper confirmed with the SMS code. Only with the organization model (the records exist only there).
        private async Task FillProtectionAsync(HoneyStoryViewModel vm)
        {
            if (!_flags.Value.OrgRoleModel)
                return;
            var rawIds = vm.RawSources.Select(s => s.RawHoneyProductionId).Distinct().ToList();
            var batchId = vm.FinalHoneyProductionId;
            var containers = await _context.Containers.AsNoTracking().Where(c => rawIds.Contains(c.RawHoneyProductionId))
                .Select(c => new { Sealed = c.OriginSealNumber != null || c.Closure != null }).ToListAsync();
            var jars = await _context.Samples.AsNoTracking()
                .Where(s => (rawIds.Contains(s.RawHoneyProductionId) && s.FinalHoneyProductionId == null) || s.FinalHoneyProductionId == batchId)
                .Select(s => new { s.SealIntact }).ToListAsync();
            var intakes = await _context.LotIntakes.AsNoTracking().Where(i => rawIds.Contains(i.RawHoneyProductionId))
                .Select(i => new { i.ConfirmationMethod, i.ConfirmedAt }).ToListAsync();
            vm.Protection = new HoneyStoryProtection
            {
                Containers = containers.Count,
                SealedContainers = containers.Count(c => c.Sealed),
                Jars = jars.Count,
                JarsCheckedAtLab = jars.Count(j => j.SealIntact == true),
                Deliveries = intakes.Count,
                SmsConfirmedDeliveries = intakes.Count(i => i.ConfirmationMethod == ConfirmationMethod.SmsCode && i.ConfirmedAt != null)
            };
        }

        // D-108: the explanation of every lab parameter the page shows, for the value sheet
        private async Task FillParameterContentsAsync(HoneyStoryViewModel vm)
        {
            var ids = vm.RawSources.SelectMany(HoneyStoryLab.Values).Select(v => v.ParameterId)
                .Concat(vm.FinalResult?.Values.Select(v => (int?)v.ParameterId) ?? Enumerable.Empty<int?>())
                .OfType<int>().Distinct().ToList();
            if (ids.Count == 0)
                return;
            var contents = await _context.CriticalControlPointParameterContents.AsNoTracking()
                .Where(c => ids.Contains(c.ParameterId) && c.IsActive).OrderBy(c => c.SortOrder).ThenBy(c => c.ContentId).ToListAsync();
            vm.ParameterContents = contents.GroupBy(c => c.ParameterId).ToDictionary(g => g.Key, g => g.First());
        }

        // D-108: the video of a lab parameter for the value sheet: only known keys and languages (no path from the request reaches
        // the disk), range requests for the video player, cached by the browser for a day
        [HttpGet("video/{lang}/{key}")]
        public IActionResult Video(string lang, string key, [FromServices] ParameterVideos videos)
        {
            var path = videos.File(lang, key);
            if (path == null)
                return NotFound();
            var info = new FileInfo(path);
            Response.Headers.CacheControl = "public, max-age=86400";
            var tag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{info.Length:x}-{info.LastWriteTimeUtc.Ticks:x}\"");
            return PhysicalFile(path, "video/mp4", new DateTimeOffset(info.LastWriteTimeUtc), tag, enableRangeProcessing: true);
        }

        private async Task FillJourneyAsync(HoneyStoryViewModel vm, FinalHoneyProduction batch)
        {
            var j = vm.Journey;
            var sources = vm.RawSources;
            var harvested = sources.Where(s => s.HarvestedOn != null).Select(s => s.HarvestedOn!.Value).ToList();
            j.HarvestFrom = harvested.Count > 0 ? harvested.Min() : null;
            j.HarvestTo = harvested.Count > 0 ? harvested.Max() : null;
            var blocks = sources.SelectMany(s => s.CcpBlocks.Where(b => b.CCPCode != HoneyStoryLab.LabCcp)).Concat(vm.BatchCcpBlocks)
                .Where(b => b.State != HoneyStoryCcpBlock.NotApplicable).ToList();
            j.ControlPointsTotal = blocks.Count;
            j.ControlPointsRecorded = blocks.Count(b => b.State == HoneyStoryCcpBlock.Recorded);
            j.FinalizedAt = batch.FinalHoneyProductionFinalizedDate;
            j.LabValuesRecorded = sources.Any(s => s.CcpBlocks.Any(b => b.CCPCode == HoneyStoryLab.LabCcp && b.State != HoneyStoryCcpBlock.NotRecorded));
            j.Labs = sources.Where(s => s.LabReportCode != null).Select(s => s.LabName).OfType<string>().Distinct().ToList();
            j.ApprovedReports = sources.Select(s => s.LabReportCode).OfType<string>().Distinct().Count();
            j.LabApprovedAt = sources.Max(s => s.LabApprovedAt);

            if (!_flags.Value.OrgRoleModel)
                return;
            j.Tracked = true;
            var rawIds = sources.Select(s => s.RawHoneyProductionId).ToList();
            var intakes = await _context.LotIntakes.AsNoTracking().Where(i => rawIds.Contains(i.RawHoneyProductionId))
                .Select(i => new { i.RawHoneyProductionId, i.DeliveryDate, i.Province, Receiver = i.ReceivingOrg!.DisplayName }).ToListAsync();
            j.Deliveries = intakes.Count;
            j.DeliveredOn = intakes.Count > 0 ? intakes.Max(i => i.DeliveryDate) : null;
            j.Receivers = intakes.Select(i => i.Receiver).Distinct().ToList();
            var shelf = await _context.Shipments.AsNoTracking()
                .Where(s => s.FinalHoneyProductionId == vm.FinalHoneyProductionId && s.Status == ShipmentStatus.Received)
                .Select(s => new { Retailer = s.ToOrg!.DisplayName, s.DecidedAt }).ToListAsync();
            j.Retailers = shelf.Select(s => s.Retailer).Distinct().ToList();
            j.OnShelfSince = shelf.Count > 0 ? shelf.Min(s => s.DecidedAt) : null;

            var owners = await _context.RawHoneyProductions.AsNoTracking().Where(r => rawIds.Contains(r.RawHoneyProductionId) && r.RawHoneyProductionApiary != null)
                .Select(r => new { r.RawHoneyProductionId, r.RawHoneyProductionApiary!.ApiaryBeekeeperId }).ToListAsync();
            var ownerIds = owners.Select(o => o.ApiaryBeekeeperId).ToList();
            var registry = await _context.BeekeeperRegistry.AsNoTracking().Where(r => ownerIds.Contains(r.BeekeeperId))
                .Select(r => new { r.BeekeeperId, r.Province, r.RegistryNumber }).ToListAsync();
            foreach (var s in sources)
            {
                var declared = Provinces.PlateOf(intakes.FirstOrDefault(i => i.RawHoneyProductionId == s.RawHoneyProductionId)?.Province);
                if (declared > 0)
                    s.ProvincePlate = declared;
                else if (s.ProvincePlate == 0)
                {
                    var owner = owners.FirstOrDefault(o => o.RawHoneyProductionId == s.RawHoneyProductionId)?.ApiaryBeekeeperId;
                    var r = registry.FirstOrDefault(x => x.BeekeeperId == owner);
                    s.ProvincePlate = r == null ? 0 : Provinces.PlateOf(r.Province ?? Provinces.FromRegistryNumber(r.RegistryNumber));
                }
            }
        }
    }
}
