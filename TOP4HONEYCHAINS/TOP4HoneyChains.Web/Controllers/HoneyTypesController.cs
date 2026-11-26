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
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class HoneyTypesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly HoneyTypeCatalog _catalog;

        public HoneyTypesController(TOP4HoneyChainsContext context, HoneyTypeCatalog catalog)
        {
            _context = context;
            _catalog = catalog;
        }

        // D-98: a title must not stand for another type (same name ignoring case, letters, spaces and the word "honey")
        private async Task CheckTitleAsync(HoneyType honeyType)
        {
            if (await _catalog.MatchAsync(honeyType.HoneyTypeTitle, honeyType.HoneyTypeId == 0 ? null : honeyType.HoneyTypeId) is HoneyType other)
                ModelState.AddModelError(nameof(HoneyType.HoneyTypeTitle), $"This honey type already exists as {other.HoneyTypeTitle}: choose it from the list.");
        }

        // GET: HoneyTypes
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Index";

            return View(await _context.HoneyTypes.ToListAsync());
        }

        // GET: HoneyTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var honeyType = await _context.HoneyTypes
                .FirstOrDefaultAsync(m => m.HoneyTypeId == id);
            if (honeyType == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Details";

            return View(honeyType);
        }

        // GET: HoneyTypes/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: HoneyTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoneyTypeTitle,HoneyTypeDescription,HoneyClass")] HoneyType honeyType)
        {
            await CheckTitleAsync(honeyType);
            if (ModelState.IsValid)
            {
                honeyType.CreatedAt = DateTime.UtcNow;
                honeyType.CreatedByUserId = HttpContext.RequestServices.GetRequiredService<ICurrentOrg>().UserId;
                _context.Add(honeyType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Create";

            return View(honeyType);
        }

        // GET: HoneyTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var honeyType = await _context.HoneyTypes.FindAsync(id);
            if (honeyType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Edit";

            return View(honeyType);
        }

        // POST: HoneyTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HoneyTypeId,HoneyTypeTitle,HoneyTypeDescription,HoneyClass")] HoneyType honeyType)
        {
            if (id != honeyType.HoneyTypeId)
            {
                return NotFound();
            }

            await CheckTitleAsync(honeyType);
            if (ModelState.IsValid)
            {
                try
                {
                    // D-98: rename, description and class only; who added the type stays
                    var stored = await _context.HoneyTypes.FirstOrDefaultAsync(h => h.HoneyTypeId == id);
                    if (stored == null)
                        return NotFound();
                    stored.HoneyTypeTitle = honeyType.HoneyTypeTitle;
                    stored.HoneyTypeDescription = honeyType.HoneyTypeDescription;
                    stored.HoneyClass = honeyType.HoneyClass;
                    await _context.SaveChangesAsync();
                    HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.RefText>().Invalidate();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoneyTypeExists(honeyType.HoneyTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Edit";

            return View(honeyType);
        }

        // GET: HoneyTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var honeyType = await _context.HoneyTypes
                .FirstOrDefaultAsync(m => m.HoneyTypeId == id);
            if (honeyType == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Delete";

            return View(honeyType);
        }

        // POST: HoneyTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var honeyType = await _context.HoneyTypes.FindAsync(id);
            if (honeyType != null)
            {
                _context.HoneyTypes.Remove(honeyType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- D-98: merge one honey type into another

        public async Task<IActionResult> Merge(int id)
        {
            var source = await _context.HoneyTypes.AsNoTracking().FirstOrDefaultAsync(h => h.HoneyTypeId == id);
            if (source == null)
                return NotFound();
            await MergeViewAsync(source);
            return View(source);
        }

        private async Task MergeViewAsync(HoneyType source)
        {
            var id = source.HoneyTypeId;
            ViewBag.Targets = new SelectList(await _context.HoneyTypes.AsNoTracking().Where(h => h.HoneyTypeId != id).OrderBy(h => h.HoneyTypeTitle).ToListAsync(),
                "HoneyTypeId", "HoneyTypeTitle").Translated(HttpContext, "HoneyType", "Title");
            ViewBag.Lots = await _context.RawHoneyProductions.CountAsync(r => r.RawHoneyProductionHoneyType == id);
            ViewBag.Limits = await _context.CriticalControlPointParameterLimits.CountAsync(l => l.HoneyTypeId == id);
            ViewBag.Beekeepers = await _context.BeekeeperHoneyTypes.CountAsync(b => b.HoneyTypeId == id);
            ViewBag.Controller = "Honey Types";
            ViewBag.Action = "Merge";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Merge(int id, int targetId)
        {
            var source = await _context.HoneyTypes.AsNoTracking().FirstOrDefaultAsync(h => h.HoneyTypeId == id);
            if (source == null)
                return NotFound();
            var target = await _context.HoneyTypes.AsNoTracking().Where(h => h.HoneyTypeId == targetId).Select(h => h.HoneyTypeTitle).FirstOrDefaultAsync();
            if (await _catalog.MergeAsync(id, targetId) is string error)
            {
                ModelState.AddModelError("", error);
                await MergeViewAsync(source);
                return View(source);
            }
            TempData["Success"] = $"{source.HoneyTypeTitle} was merged into {target}.";
            return RedirectToAction(nameof(Index));
        }

        private bool HoneyTypeExists(int id)
        {
            return _context.HoneyTypes.Any(e => e.HoneyTypeId == id);
        }
    }
}
