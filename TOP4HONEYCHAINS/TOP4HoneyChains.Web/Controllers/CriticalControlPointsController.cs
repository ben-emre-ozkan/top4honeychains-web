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
    public class CriticalControlPointsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CriticalControlPointsController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: CriticalControlPoints
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Index";

            return View(await _context.CriticalControlPoints.OrderBy(c => c.Id).ToListAsync());
        }

        // GET: CriticalControlPoints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPoint = await _context.CriticalControlPoints
                .FirstOrDefaultAsync(m => m.Id == id);
            if (criticalControlPoint == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Details";

            return View(criticalControlPoint);
        }

        // GET: CriticalControlPoints/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: CriticalControlPoints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProcessingStep,Code,Title,CriticalityJustification,AppliesTo")] CriticalControlPoint criticalControlPoint)
        {
            if (ModelState.IsValid)
            {
                _context.Add(criticalControlPoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Create";

            return View(criticalControlPoint);
        }

        // GET: CriticalControlPoints/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPoint = await _context.CriticalControlPoints.FindAsync(id);
            if (criticalControlPoint == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Edit";

            return View(criticalControlPoint);
        }

        // POST: CriticalControlPoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProcessingStep,Code,Title,CriticalityJustification,AppliesTo")] CriticalControlPoint criticalControlPoint)
        {
            if (id != criticalControlPoint.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(criticalControlPoint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CriticalControlPointExists(criticalControlPoint.Id))
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

            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Edit";

            return View(criticalControlPoint);
        }

        // GET: CriticalControlPoints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPoint = await _context.CriticalControlPoints
                .FirstOrDefaultAsync(m => m.Id == id);
            if (criticalControlPoint == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Critical Control Points";
            ViewBag.Action = "Delete";

            return View(criticalControlPoint);
        }

        // POST: CriticalControlPoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var criticalControlPoint = await _context.CriticalControlPoints.FindAsync(id);
            if (criticalControlPoint != null)
            {
                _context.CriticalControlPoints.Remove(criticalControlPoint);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CriticalControlPointExists(int id)
        {
            return _context.CriticalControlPoints.Any(e => e.Id == id);
        }
    }
}
