using TOP4HoneyChains.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CriticalControlPointParameterContentsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CriticalControlPointParameterContentsController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? parameterId)
        {
            var query = _context.CriticalControlPointParameterContents
                .Include(c => c.Parameter)
                .AsNoTracking();

            if (parameterId != null)
            {
                query = query.Where(c => c.ParameterId == parameterId);
                ViewBag.ParameterId = parameterId;
            }

            var contents = await query
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return View(contents);
        }

        public IActionResult Create(int parameterId)
        {
            var parameter = _context.CriticalControlPointParameters
                .FirstOrDefault(p => p.ParameterId == parameterId);

            if (parameter == null)
                return NotFound();

            var model = new CriticalControlPointParameterContent
            {
                ParameterId = parameterId
            };

            ViewBag.ParameterName = parameter.ParameterName;

            ViewBag.ParameterId = parameterId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CriticalControlPointParameterContent content,
    IFormFile? videoFile)
        {
            // The video is shown in Honey Story, so a new content item needs one
            if (videoFile == null || videoFile.Length == 0)
                ModelState.AddModelError("videoFile", "A video file is required.");
            else if (await UploadValidator.ValidateVideoAsync(videoFile) is string videoError)
                ModelState.AddModelError("videoFile", videoError);

            if (ModelState.IsValid)
            {
                if (videoFile != null && videoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + UploadValidator.SafeVideoExtension(videoFile);
                    var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/assets/videos/parameters", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }

                    content.VideoPath = "/assets/videos/parameters/" + fileName;
                }

                _context.Add(content);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParameterName = _context.CriticalControlPointParameters
                .Where(p => p.ParameterId == content.ParameterId)
                .Select(p => p.ParameterName)
                .FirstOrDefault();
            ViewBag.ParameterId = content.ParameterId;

            return View(content);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var content = await _context.CriticalControlPointParameterContents
                .Include(c => c.Parameter) // 🔥 KRİTİK NOKTA
                .FirstOrDefaultAsync(c => c.ContentId == id);

            if (content == null)
                return NotFound();

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     CriticalControlPointParameterContent content,
     IFormFile? videoFile)
        {
            if (id != content.ContentId)
                return NotFound();

            if (videoFile != null && videoFile.Length > 0 && await UploadValidator.ValidateVideoAsync(videoFile) is string videoError)
                ModelState.AddModelError("videoFile", videoError);

            if (ModelState.IsValid)
            {
                var existing = await _context.CriticalControlPointParameterContents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ContentId == id);

                if (existing == null)
                    return NotFound();

                // 🔥 VIDEO UPDATE
                if (videoFile != null && videoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + UploadValidator.SafeVideoExtension(videoFile);
                    var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/assets/videos/parameters", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }

                    content.VideoPath = "/assets/videos/parameters/" + fileName;
                }
                else
                {
                    // 🔥 eski videoyu koru
                    content.VideoPath = existing.VideoPath;
                }

                _context.Update(content);
                await _context.SaveChangesAsync();

                // The replaced video is no longer referenced
                if (content.VideoPath != existing.VideoPath)
                    DeleteStoredVideo(existing.VideoPath);

                return RedirectToAction("Index",
                    new { parameterId = content.ParameterId });
            }

            content.Parameter = await _context.CriticalControlPointParameters.FindAsync(content.ParameterId);
            return View(content);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var content = await _context.CriticalControlPointParameterContents
                .FindAsync(id);

            if (content != null)
            {
                _context.CriticalControlPointParameterContents.Remove(content);
                await _context.SaveChangesAsync();
                DeleteStoredVideo(content.VideoPath);

                TempData["success"] = "Content deleted successfully";
                return RedirectToAction(nameof(Index), new { parameterId = content.ParameterId });
            }

            return RedirectToAction(nameof(Index));
        }

        // Removes an uploaded video file (only inside the uploads folder) once no content uses it
        private void DeleteStoredVideo(string? videoPath)
        {
            const string prefix = "/assets/videos/parameters/";
            if (string.IsNullOrEmpty(videoPath) || !videoPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return;
            if (_context.CriticalControlPointParameterContents.Any(c => c.VideoPath == videoPath))
                return;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/videos/parameters", Path.GetFileName(videoPath));
            try
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch (IOException)
            {
                // A file that cannot be deleted now is only wasted space; the record is already gone
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private bool CriticalControlPointParameterContentExists(int id)
        {
            return _context.CriticalControlPointParameterContents.Any(e => e.ContentId == id);
        }
    }
}
