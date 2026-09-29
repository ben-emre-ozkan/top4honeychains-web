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
    public class BeekeepingPurposeTypesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public BeekeepingPurposeTypesController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: BeekeepingPurposeTypes
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Index";

            return View(await _context.BeekeepingPurposeTypes.ToListAsync());
        }

        // GET: BeekeepingPurposeTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingPurposeType = await _context.BeekeepingPurposeTypes
                .FirstOrDefaultAsync(m => m.BeekeepingPurposeTypeId == id);
            if (beekeepingPurposeType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Details";

            return View(beekeepingPurposeType);
        }

        // GET: BeekeepingPurposeTypes/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: BeekeepingPurposeTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BeekeepingPurposeTypeId,BeekeepingPurposeTypeTitle")] BeekeepingPurposeType beekeepingPurposeType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(beekeepingPurposeType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Create";
            return View(beekeepingPurposeType);
        }

        // GET: BeekeepingPurposeTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingPurposeType = await _context.BeekeepingPurposeTypes.FindAsync(id);
            if (beekeepingPurposeType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Edit";

            return View(beekeepingPurposeType);
        }

        // POST: BeekeepingPurposeTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BeekeepingPurposeTypeId,BeekeepingPurposeTypeTitle")] BeekeepingPurposeType beekeepingPurposeType)
        {
            if (id != beekeepingPurposeType.BeekeepingPurposeTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(beekeepingPurposeType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeekeepingPurposeTypeExists(beekeepingPurposeType.BeekeepingPurposeTypeId))
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

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Edit";

            return View(beekeepingPurposeType);
        }

        // GET: BeekeepingPurposeTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeepingPurposeType = await _context.BeekeepingPurposeTypes
                .FirstOrDefaultAsync(m => m.BeekeepingPurposeTypeId == id);
            if (beekeepingPurposeType == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeping Purpose Types";
            ViewBag.Action = "Delete";

            return View(beekeepingPurposeType);
        }

        // POST: BeekeepingPurposeTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beekeepingPurposeType = await _context.BeekeepingPurposeTypes.FindAsync(id);
            if (beekeepingPurposeType != null)
            {
                _context.BeekeepingPurposeTypes.Remove(beekeepingPurposeType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BeekeepingPurposeTypeExists(int id)
        {
            return _context.BeekeepingPurposeTypes.Any(e => e.BeekeepingPurposeTypeId == id);
        }
    }
}
