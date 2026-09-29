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
    public class BeekeeperEducationLevelsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public BeekeeperEducationLevelsController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Index";

            return View(await _context.BeekeeperEducationLevels.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Create";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BeekeeperEducationLevelId,BeekeeperEducationLevelTitle")] BeekeeperEducationLevel beekeeperEducationLevel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(beekeeperEducationLevel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Create";

            return View(beekeeperEducationLevel);
        }

        // GET: BeekeeperEducationLevels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeeperEducationLevel = await _context.BeekeeperEducationLevels.FindAsync(id);
            if (beekeeperEducationLevel == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Edit";
            return View(beekeeperEducationLevel);
        }

        // POST: BeekeeperEducationLevels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BeekeeperEducationLevelId,BeekeeperEducationLevelTitle")] BeekeeperEducationLevel beekeeperEducationLevel)
        {
            if (id != beekeeperEducationLevel.BeekeeperEducationLevelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(beekeeperEducationLevel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeekeeperEducationLevelExists(beekeeperEducationLevel.BeekeeperEducationLevelId))
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

            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Edit";
            return View(beekeeperEducationLevel);
        }

        // GET: BeekeeperEducationLevels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeeperEducationLevel = await _context.BeekeeperEducationLevels
                .FirstOrDefaultAsync(m => m.BeekeeperEducationLevelId == id);
            if (beekeeperEducationLevel == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Delete";

            return View(beekeeperEducationLevel);
        }

        // POST: BeekeeperEducationLevels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beekeeperEducationLevel = await _context.BeekeeperEducationLevels.FindAsync(id);
            if (beekeeperEducationLevel != null)
            {
                _context.BeekeeperEducationLevels.Remove(beekeeperEducationLevel);
            }

            await _context.SaveChangesAsync();

            ViewBag.Controller = "Beekeeper Education Levels";
            ViewBag.Action = "Delete";
            return RedirectToAction(nameof(Index));
        }

        private bool BeekeeperEducationLevelExists(int id)
        {
            return _context.BeekeeperEducationLevels.Any(e => e.BeekeeperEducationLevelId == id);
        }
    }
}
