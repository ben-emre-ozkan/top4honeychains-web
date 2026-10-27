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
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ApiariesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public ApiariesController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tOP4HoneyChainsContext = _context.Apiaries.Include(a => a.ApiaryBeekeeper);
            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Index";
            return View(await tOP4HoneyChainsContext.ToListAsync());
        }

        public IActionResult Create(string beekeeperId)
        {
            if (string.IsNullOrEmpty(beekeeperId))
                return NotFound();

            var apiary = new Apiary
            {
                ApiaryBeekeeperId = beekeeperId
            };

            ViewBag.BeekeeperId = beekeeperId;

            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Create";
            return View(apiary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Apiary apiary)
        {
            // D-78 (F5): the title is generated from province – district – locality, never typed
            ModelState.Remove(nameof(Apiary.ApiaryTitle));
            var (province, district, placeError) = await ApiaryTitles.ResolveAsync(_context, apiary.Province, apiary.DistrictId, apiary.Locality);
            if (placeError != null)
                ModelState.AddModelError("", placeError);
            if (ModelState.IsValid)
            {
                apiary.Locality = apiary.Locality!.Trim();
                apiary.ApiaryTitle = await ApiaryTitles.UniqueAsync(_context, province!, district!, apiary.Locality);
                apiary.TitleSource = ApiaryTitles.Generated;

                // QR Code otomatik üret
                apiary.ApiaryQRCode = Guid.NewGuid();

                _context.Add(apiary);
                await _context.SaveChangesAsync();

                return RedirectToAction("ByBeekeeper",
                    new { beekeeperId = apiary.ApiaryBeekeeperId });
            }
            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Create";
            return View(apiary);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var apiary = await _context.Apiaries
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApiaryId == id);

            if (apiary == null)
                return NotFound();
            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Edit";
            return View(apiary);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Apiary apiary)
        {
            if (id != apiary.ApiaryId)
                return NotFound();

            var existingApiary = await _context.Apiaries
                .FirstOrDefaultAsync(a => a.ApiaryId == id);

            if (existingApiary == null)
                return NotFound();

            // D-78 (F5): a generated title follows its parts; a typed (legacy) title stays until province, district and locality are given
            ModelState.Remove(nameof(Apiary.ApiaryTitle));
            var anyPart = !string.IsNullOrWhiteSpace(apiary.Province) || apiary.DistrictId != null || !string.IsNullOrWhiteSpace(apiary.Locality);
            string? province = null, district = null;
            if (anyPart || existingApiary.TitleSource == ApiaryTitles.Generated)
            {
                string? placeError;
                (province, district, placeError) = await ApiaryTitles.ResolveAsync(_context, apiary.Province, apiary.DistrictId, apiary.Locality);
                if (placeError != null)
                    ModelState.AddModelError("", placeError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 🔒 Değişmemesi gereken alanlar korunur
                    apiary.ApiaryBeekeeperId = existingApiary.ApiaryBeekeeperId;
                    apiary.ApiaryQRCode = existingApiary.ApiaryQRCode;

                    // Manuel update (daha güvenli)
                    if (province != null)
                    {
                        existingApiary.Province = apiary.Province;
                        existingApiary.DistrictId = apiary.DistrictId;
                        existingApiary.Locality = apiary.Locality!.Trim();
                        existingApiary.ApiaryTitle = await ApiaryTitles.UniqueAsync(_context, province, district!, existingApiary.Locality, existingApiary.ApiaryId);
                        existingApiary.TitleSource = ApiaryTitles.Generated;
                    }
                    existingApiary.ApiaryBeehivesCount = apiary.ApiaryBeehivesCount;
                    existingApiary.ApiaryLocationLatitude = apiary.ApiaryLocationLatitude;
                    existingApiary.ApiaryLocationLongitude = apiary.ApiaryLocationLongitude;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiaryExists(apiary.ApiaryId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction("ByBeekeeper",
                    new { beekeeperId = existingApiary.ApiaryBeekeeperId });
            }

            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Edit";
            apiary.ApiaryTitle = existingApiary.ApiaryTitle;
            apiary.TitleSource = existingApiary.TitleSource;

            return View(apiary);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var apiary = await _context.Apiaries.FindAsync(id);
            if (apiary == null)
                return NotFound();

            apiary.ApiaryIsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("ByBeekeeper",
                new { beekeeperId = apiary.ApiaryBeekeeperId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var apiary = await _context.Apiaries.FindAsync(id);
            if (apiary == null)
                return NotFound();

            apiary.ApiaryIsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("ByBeekeeper",
                new { beekeeperId = apiary.ApiaryBeekeeperId });
        }




        public async Task<IActionResult> ByBeekeeper(string beekeeperId)
        {
            if (string.IsNullOrEmpty(beekeeperId))
                return NotFound();

            var beekeeper = await _context.Beekeepers
                .FirstOrDefaultAsync(b => b.BeekeeperId == beekeeperId);

            if (beekeeper == null)
                return NotFound();

            var apiaries = await _context.Apiaries
                .Where(a => a.ApiaryBeekeeperId == beekeeperId)
                .Include(a => a.ApiaryPhotos)
                .ToListAsync();

            ViewBag.BeekeeperName = beekeeper.BeekeeperFirstName + " " + beekeeper.BeekeeperLastName;

            ViewBag.BeekeeperId = beekeeperId;

            ViewBag.Controller = "Apiaries";
            ViewBag.Action = "Apiaries by beekeeper";

            return View(apiaries);
        }




        private bool ApiaryExists(int id)
        {
            return _context.Apiaries.Any(e => e.ApiaryId == id);
        }
    }
}
