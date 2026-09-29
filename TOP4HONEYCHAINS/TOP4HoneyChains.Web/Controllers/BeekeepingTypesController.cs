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
    public class BeekeepingTypesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public BeekeepingTypesController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: BeekeepingTypes
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Index";

            return View(await _context.BeekeepingTypes.ToListAsync());
        }

        // GET: BeekeepingTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingType = await _context.BeekeepingTypes
                .FirstOrDefaultAsync(m => m.BeekeepingTypeId == id);
            if (beekeepingType == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Details";

            return View(beekeepingType);
        }

        // GET: BeekeepingTypes/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: BeekeepingTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BeekeepingTypeId,BeekeepingTypeTitle")] BeekeepingType beekeepingType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(beekeepingType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Create";

            return View(beekeepingType);
        }

        // GET: BeekeepingTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingType = await _context.BeekeepingTypes.FindAsync(id);
            if (beekeepingType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Edit";

            return View(beekeepingType);
        }

        // POST: BeekeepingTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BeekeepingTypeId,BeekeepingTypeTitle")] BeekeepingType beekeepingType)
        {
            if (id != beekeepingType.BeekeepingTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(beekeepingType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeekeepingTypeExists(beekeepingType.BeekeepingTypeId))
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

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Edit";

            return View(beekeepingType);
        }

        // GET: BeekeepingTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingType = await _context.BeekeepingTypes
                .FirstOrDefaultAsync(m => m.BeekeepingTypeId == id);
            if (beekeepingType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeping Types";
            ViewBag.Action = "Delete";

            return View(beekeepingType);
        }

        // POST: BeekeepingTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beekeepingType = await _context.BeekeepingTypes.FindAsync(id);
            if (beekeepingType != null)
            {
                _context.BeekeepingTypes.Remove(beekeepingType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BeekeepingTypeExists(int id)
        {
            return _context.BeekeepingTypes.Any(e => e.BeekeepingTypeId == id);
        }
    }
}
