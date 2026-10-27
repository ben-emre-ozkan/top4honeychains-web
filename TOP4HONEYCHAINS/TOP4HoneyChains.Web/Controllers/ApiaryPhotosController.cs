using TOP4HoneyChains.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ApiaryPhotosController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public ApiaryPhotosController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // /ApiaryPhotos/Index?apiaryId=17&periodId=3
        public async Task<IActionResult> Index(int apiaryId, int periodId)
        {
            var apiary = await _context.Apiaries
                .Include(a => a.ApiaryBeekeeper)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApiaryId == apiaryId);

            var period = await _context.RawHoneyProductionPeriods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.RawHoneyProductionPeriodId == periodId);

            if (apiary == null || period == null)
                return NotFound();

            ViewBag.ApiaryId = apiaryId;
            ViewBag.PeriodId = periodId;
            ViewBag.ApiaryTitle = apiary.ApiaryTitle;
            ViewBag.PeriodTitle = period.RawHoneyProductionPeriodTitle;
            ViewBag.BeekeeperName = apiary.ApiaryBeekeeper?.BeekeeperFirstName + " " + apiary.ApiaryBeekeeper?.BeekeeperLastName;

            var photos = await _context.ApiaryPhotos
                .Where(x => x.ApiaryPhotoApiaryId == apiaryId && x.ApiaryPhotoProductionPeriodId == periodId)
                .OrderByDescending(x => x.ApiaryPhotoCreatedDate)
                .ToListAsync();

            ViewBag.Controller = "Apiary Photos";
            ViewBag.Action = "Index";

            return View(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSingle(int apiaryId, int periodId, IFormFile file, string? desc)
        {
            if (apiaryId <= 0 || periodId <= 0)
                return BadRequest("Missing apiaryId/periodId.");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Index), new { apiaryId, periodId });
            }

            // Extension, real image signature, dimensions and size (the posted Content-Type can be forged)
            var uploadError = await UploadValidator.ValidateImageAsync(file);
            if (uploadError != null)
            {
                TempData["Error"] = uploadError;
                return RedirectToAction(nameof(Index), new { apiaryId, periodId });
            }

            // Upload sırasında 800x530 crop + auto orient + jpeg encode
            const int targetWidth = 800;
            const int targetHeight = 530;

            await using var stream = file.OpenReadStream();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            var jpeg = ImageTools.CropToJpeg(buffer, targetWidth, targetHeight, 85);
            if (jpeg == null)
            {
                TempData["Error"] = $"The file is not a valid image ({UploadValidator.ImageRule}).";
                return RedirectToAction(nameof(Index), new { apiaryId, periodId });
            }
            using var ms = new MemoryStream(jpeg);

            _context.ApiaryPhotos.Add(new ApiaryPhoto
            {
                ApiaryPhotoApiaryId = apiaryId,
                ApiaryPhotoProductionPeriodId = periodId,
                ApiaryPhotoDesc = desc,
                ApiaryPhotoCreatedDate = DateTime.Now,
                ApiaryPhotoApproved = true,
                ApiaryPhotoFileName = Path.GetFileName(file.FileName),
                ApiaryPhotoContentType = "image/jpeg",
                ApiaryPhotoData = ms.ToArray()
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Photo uploaded.";
            return RedirectToAction(nameof(Index), new { apiaryId, periodId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDesc(int id, int apiaryId, int periodId, string? desc)
        {
            var photo = await _context.ApiaryPhotos.FirstOrDefaultAsync(x => x.ApiaryPhotoId == id);
            if (photo == null) return NotFound();

            photo.ApiaryPhotoDesc = desc;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Description updated.";
            return RedirectToAction(nameof(Index), new { apiaryId, periodId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int apiaryId, int periodId)
        {
            var photo = await _context.ApiaryPhotos.FirstOrDefaultAsync(x => x.ApiaryPhotoId == id);
            if (photo == null) return NotFound();

            _context.ApiaryPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Photo deleted.";
            return RedirectToAction(nameof(Index), new { apiaryId, periodId });
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
    }
}
