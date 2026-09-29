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
    public class RawHoneyProductionPeriodsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public RawHoneyProductionPeriodsController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: RawHoneyProductionPeriods
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Index";

            return View(await _context.RawHoneyProductionPeriods.ToListAsync());
        }

        // GET: RawHoneyProductionPeriods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawHoneyProductionPeriod = await _context.RawHoneyProductionPeriods
                .FirstOrDefaultAsync(m => m.RawHoneyProductionPeriodId == id);
            if (rawHoneyProductionPeriod == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Details";

            return View(rawHoneyProductionPeriod);
        }

        // GET: RawHoneyProductionPeriods/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: RawHoneyProductionPeriods/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RawHoneyProductionPeriodId,RawHoneyProductionPeriodTitle,RawHoneyProductionPeriodStartDate,RawHoneyProductionPeriodEndDate")] RawHoneyProductionPeriod rawHoneyProductionPeriod)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rawHoneyProductionPeriod);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Create";

            return View(rawHoneyProductionPeriod);
        }

        // GET: RawHoneyProductionPeriods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawHoneyProductionPeriod = await _context.RawHoneyProductionPeriods.FindAsync(id);
            if (rawHoneyProductionPeriod == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Edit";

            return View(rawHoneyProductionPeriod);
        }

        // POST: RawHoneyProductionPeriods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RawHoneyProductionPeriodId,RawHoneyProductionPeriodTitle,RawHoneyProductionPeriodStartDate,RawHoneyProductionPeriodEndDate")] RawHoneyProductionPeriod rawHoneyProductionPeriod)
        {
            if (id != rawHoneyProductionPeriod.RawHoneyProductionPeriodId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rawHoneyProductionPeriod);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RawHoneyProductionPeriodExists(rawHoneyProductionPeriod.RawHoneyProductionPeriodId))
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

            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Edit";

            return View(rawHoneyProductionPeriod);
        }

        // GET: RawHoneyProductionPeriods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawHoneyProductionPeriod = await _context.RawHoneyProductionPeriods
                .FirstOrDefaultAsync(m => m.RawHoneyProductionPeriodId == id);
            if (rawHoneyProductionPeriod == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Raw Honey Production Periods";
            ViewBag.Action = "Delete";

            return View(rawHoneyProductionPeriod);
        }

        // POST: RawHoneyProductionPeriods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rawHoneyProductionPeriod = await _context.RawHoneyProductionPeriods.FindAsync(id);
            if (rawHoneyProductionPeriod != null)
            {
                _context.RawHoneyProductionPeriods.Remove(rawHoneyProductionPeriod);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RawHoneyProductionPeriodExists(int id)
        {
            return _context.RawHoneyProductionPeriods.Any(e => e.RawHoneyProductionPeriodId == id);
        }
    }
}
