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
                .Where(r => r.Status == LabReportStatus.Approved && rawIds.Contains(r.Sample!.RawHoneyProductionId)
                            && r.LabOrg!.Status == OrgStatus.Verified)
                .Select(r => new { r.Sample!.RawHoneyProductionId, r.IsAccredited, r.AccreditationNumber, r.InHouse, r.ReportCode })
                .ToListAsync();
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
        public async Task<IActionResult> ParameterDetails(int id)
        {
            var parameter = await _context.CriticalControlPointParameters
                .Include(p => p.CCP)
                .Include(p => p.CriticalControlPointParameterContents
                    .Where(c => c.IsActive))
                .FirstOrDefaultAsync(p => p.ParameterId == id);

            if (parameter == null)
                return NotFound();

            var content = parameter.CriticalControlPointParameterContents
                .OrderBy(c => c.SortOrder)
                .FirstOrDefault();

            return View(content);
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


            // 4) VM doldur
            foreach (var d in details)
            {
                var raw = d.Raw;
                if (raw == null) continue;

                var apiary = raw.RawHoneyProductionApiary;
                var beekeeper = apiary?.ApiaryBeekeeper;

                var source = new HoneyStoryRawSource
                {
                    RawHoneyProductionId = d.RawHoneyProductionId,
                    ApiaryId = raw.RawHoneyProductionApiaryId,
                    ApiaryName = apiary?.ApiaryTitle,
                    BeekeeperName = (beekeeper?.BeekeeperFirstName + " " + beekeeper?.BeekeeperLastName)?.Trim(),
                    HoneyType = raw.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                    UsedAmount = d.UsedAmount,
                    // D-12: only an approximate location leaves the server (0.1° ≈ 10 km, district level);
                    // the beekeeper's photo and phone are not shown publicly
                    Latitude = ApproximateCoordinate(apiary?.ApiaryLocationLatitude),
                    Longitude = ApproximateCoordinate(apiary?.ApiaryLocationLongitude)
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
                            Description = p.ApiaryPhotoDesc
                        })
                        .ToList();
                }

                vm.RawSources.Add(source);
            }

            await FillCcpBlocksAsync(vm);
            await FillBadgesAsync(vm);

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


        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Image(int id)
        {
            var photo = await _context.ApiaryPhotos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApiaryPhotoId == id);

            if (photo?.ApiaryPhotoData == null || string.IsNullOrEmpty(photo.ApiaryPhotoContentType))
                return NotFound();

            // D-12: public only for apiaries that supplied a finalized batch (ids are sequential,
            // so without this check every apiary photo could be downloaded)
            var isPublic = await _context.FinalHoneyProductionDetails.AnyAsync(d =>
                d.FinalHoneyProduction.FinalHoneyProductionIsFinalized &&
                d.RawHoneyProduction.RawHoneyProductionApiaryId == photo.ApiaryPhotoApiaryId);
            if (!isPublic)
                return NotFound();

            return File(photo.ApiaryPhotoData, photo.ApiaryPhotoContentType);
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

            // 4) VM doldur
            foreach (var d in details)
            {
                var raw = d.Raw;
                if (raw == null) continue;

                var apiary = raw.RawHoneyProductionApiary;
                var beekeeper = apiary?.ApiaryBeekeeper;

                var source = new HoneyStoryRawSource
                {
                    RawHoneyProductionId = d.RawHoneyProductionId,
                    ApiaryId = raw.RawHoneyProductionApiaryId,
                    ApiaryName = apiary?.ApiaryTitle,
                    BeekeeperName = (beekeeper?.BeekeeperFirstName + " " + beekeeper?.BeekeeperLastName)?.Trim(),
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
                            Description = p.ApiaryPhotoDesc
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
                if (log != null && p.LimitVerified)
                {
                    // The result stored at entry time; values recorded before limits existed are checked now
                    var result = log.IsInRange != null
                        ? new CcpLimitEvaluator.Result(log.IsInRange, log.AppliedLimit)
                        : CcpLimitEvaluator.Evaluate(p, log.MeasuredNumericValue, siblings, honey);
                    compliant = result.IsInRange;
                    limitText = result.AppliedLimit;
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
                    LimitText = limitText
                });
            }
            return block;
        }

        private static decimal? ApproximateCoordinate(decimal? value) =>
            value.HasValue ? Math.Round(value.Value, 1, MidpointRounding.AwayFromZero) : null;
    }
}
