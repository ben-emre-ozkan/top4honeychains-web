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
    public class CriticalControlPointParametersController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CriticalControlPointParametersController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: CriticalControlPointParameters
        public async Task<IActionResult> Index()
        {
            var tOP4HoneyChainsContext = _context.CriticalControlPointParameters.Include(c => c.CCP).Include(c => c.ParameterCategoryNavigation);

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Index";

            return View(await tOP4HoneyChainsContext.ToListAsync());
        }

        // GET: CriticalControlPointParameters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameter = await _context.CriticalControlPointParameters
                .Include(c => c.CCP)
                .Include(c => c.ParameterCategoryNavigation)
                .FirstOrDefaultAsync(m => m.ParameterId == id);
            if (criticalControlPointParameter == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Details";

            return View(criticalControlPointParameter);
        }

        // GET: CriticalControlPointParameters/Create
        public IActionResult Create()
        {
            ViewData["CCPId"] = new SelectList(_context.CriticalControlPoints, "Id", "Code");
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName");

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Create";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CriticalControlPointParameter criticalControlPointParameter)
        {

            //if (!ModelState.IsValid)
            //{
            //    foreach (var key in ModelState.Keys)
            //    {
            //        var state = ModelState[key];
            //        foreach (var error in state.Errors)
            //        {
            //            Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
            //        }
            //    }
            //}

            if (ModelState.IsValid)
            {
                _context.Add(criticalControlPointParameter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CCPId"] = new SelectList(_context.CriticalControlPoints, "Id", "Code", criticalControlPointParameter.CCPId);
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory);

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Create";

            return View(criticalControlPointParameter);
        }

        // GET: CriticalControlPointParameters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameter = await _context.CriticalControlPointParameters.FindAsync(id);
            if (criticalControlPointParameter == null)
            {
                return NotFound();
            }
            ViewData["CCPId"] = new SelectList(_context.CriticalControlPoints, "Id", "Code", criticalControlPointParameter.CCPId);
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory);

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Edit";

            return View(criticalControlPointParameter);
        }

        // POST: CriticalControlPointParameters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParameterId,CCPId,ParameterName,ParameterCategory,ParameterUnit,CriticalLimit")] CriticalControlPointParameter criticalControlPointParameter)
        {
            if (id != criticalControlPointParameter.ParameterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(criticalControlPointParameter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CriticalControlPointParameterExists(criticalControlPointParameter.ParameterId))
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
            ViewData["CCPId"] = new SelectList(_context.CriticalControlPoints, "Id", "Code", criticalControlPointParameter.CCPId);
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory);

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Edit";

            return View(criticalControlPointParameter);
        }

        // GET: CriticalControlPointParameters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameter = await _context.CriticalControlPointParameters
                .Include(c => c.CCP)
                .Include(c => c.ParameterCategoryNavigation)
                .FirstOrDefaultAsync(m => m.ParameterId == id);
            if (criticalControlPointParameter == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Delete";

            return View(criticalControlPointParameter);
        }

        // POST: CriticalControlPointParameters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var criticalControlPointParameter = await _context.CriticalControlPointParameters.FindAsync(id);
            if (criticalControlPointParameter != null)
            {
                _context.CriticalControlPointParameters.Remove(criticalControlPointParameter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CriticalControlPointParameterExists(int id)
        {
            return _context.CriticalControlPointParameters.Any(e => e.ParameterId == id);
        }
    }
}
