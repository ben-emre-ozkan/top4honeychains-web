using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Versioning;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.ViewModels.HoneyStory;

namespace TOP4HoneyChains.Web.Controllers
{
    [Route("honeystory")]
    public class HoneyStoryController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public HoneyStoryController(TOP4HoneyChainsContext context)
        {
            _context = context;
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

            if (batch == null)
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


            var rawIds = details.Select(x => x.RawHoneyProductionId).Distinct().ToList();

            var rawCcpLogs = await _context.CriticalControlPointMonitoringLogs
                .AsNoTracking()
                .Where(l => l.RawHoneyProductionId != null && rawIds.Contains(l.RawHoneyProductionId.Value))
                .Where(l => l.CCPId == 2) // CCP1
                .Include(l => l.Parameter)
                .Include(l => l.CCP)
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
                    Latitude = apiary?.ApiaryLocationLatitude,
                    Longitude = apiary?.ApiaryLocationLongitude,
                    BeekeeperPhotoBase64 = beekeeper?.BeekeeperProfilePhoto != null
        ? $"data:image/jpeg;base64,{Convert.ToBase64String(beekeeper.BeekeeperProfilePhoto)}"
        : null
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

                // Raw CCP rows (CCP0-CCP3) for THIS raw
                source.RawCCPs = rawCcpLogs
                    .Where(l => l.RawHoneyProductionId == d.RawHoneyProductionId)
                    .Select(l => new HoneyStoryCCPRow
                    {
                        CCPId = l.CCPId,
                        CCPCode = l.CCP?.Code,
                        CCPTitle = l.CCP?.Title,
                        ParameterId = l.ParameterId,
                        ParameterName = l.Parameter?.ParameterName,
                        Unit = l.Parameter?.ParameterUnit,
                        NumericValue = l.MeasuredNumericValue,
                        TextValue = l.MeasuredTextValue,
                        MeasurementDate = l.MeasurementDate,
                        Notes = l.Notes
                    })
                    .OrderBy(x => x.CCPId)
                    .ThenBy(x => x.ParameterName)
                    .ToList();

                vm.RawSources.Add(source);
            }

            return View(vm);
        }


        [HttpGet("qrcode")]
        [SupportedOSPlatform("windows")]
        public IActionResult BatchQRCode(string code)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);

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

            if (batch == null)
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

            // 2b) Raw CCP (CCP0-CCP3) monitoring loglarını raw honey production id’lerine göre çek
            // Not: Senin sistemde RawHoney tarafı loglar RawHoneyProductionId ile tutuluyor.
            var rawIds = details.Select(x => x.RawHoneyProductionId).Distinct().ToList();

            var rawCcpLogs = await _context.CriticalControlPointMonitoringLogs
                .AsNoTracking()
                .Where(l => l.RawHoneyProductionId != null && rawIds.Contains(l.RawHoneyProductionId.Value))
                .Where(l => l.CCPId >= 2 && l.CCPId <= 4) // CCP0-CCP3
                .Include(l => l.Parameter)
                .Include(l => l.CCP)
                .ToListAsync();

            // 3) Final CCP (CCP4-CCP10) monitoring loglarını batch’e göre çek
            var finalCcpLogs = await _context.CriticalControlPointMonitoringLogs
                .AsNoTracking()
                .Where(l => l.FinalHoneyProductionId == batch.FinalHoneyProductionId)
                .Where(l => l.CCPId >= 5 && l.CCPId <= 11)
                .Include(l => l.Parameter)
                .Include(l => l.CCP)
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

                // Raw CCP rows (CCP0-CCP3) for THIS raw
                source.RawCCPs = rawCcpLogs
                    .Where(l => l.RawHoneyProductionId == d.RawHoneyProductionId)
                    .Select(l => new HoneyStoryCCPRow
                    {
                        CCPId = l.CCPId,
                        CCPCode = l.CCP?.Code,
                        CCPTitle = l.CCP?.Title,
                        ParameterId = l.ParameterId,
                        ParameterName = l.Parameter?.ParameterName,
                        Unit = l.Parameter?.ParameterUnit,
                        NumericValue = l.MeasuredNumericValue,
                        TextValue = l.MeasuredTextValue,
                        MeasurementDate = l.MeasurementDate,
                        Notes = l.Notes
                    })
                    .OrderBy(x => x.CCPId)
                    .ThenBy(x => x.ParameterName)
                    .ToList();

                vm.RawSources.Add(source);
            }

            // Final CCP rows (CCP4-CCP10)
            vm.FinalCCPs = finalCcpLogs
                .Select(l => new HoneyStoryCCPRow
                {
                    CCPId = l.CCPId,
                    CCPCode = l.CCP?.Code,
                    CCPTitle = l.CCP?.Title,
                    ParameterId = l.ParameterId,
                    ParameterName = l.Parameter?.ParameterName,
                    Unit = l.Parameter?.ParameterUnit,
                    NumericValue = l.MeasuredNumericValue,
                    TextValue = l.MeasuredTextValue,
                    MeasurementDate = l.MeasurementDate,
                    Notes = l.Notes
                })
                .OrderBy(x => x.CCPId)
                .ThenBy(x => x.ParameterName)
                .ToList();

            return View(vm);
        }





    }
}
