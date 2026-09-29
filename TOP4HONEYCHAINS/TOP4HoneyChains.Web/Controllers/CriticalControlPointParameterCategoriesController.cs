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
    public class CriticalControlPointParameterCategoriesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CriticalControlPointParameterCategoriesController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: CriticalControlPointParameterCategories
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Index";

            return View(await _context.CriticalControlPointParameterCategories.ToListAsync());
        }

        // GET: CriticalControlPointParameterCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameterCategory = await _context.CriticalControlPointParameterCategories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (criticalControlPointParameterCategory == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Details";

            return View(criticalControlPointParameterCategory);
        }

        // GET: CriticalControlPointParameterCategories/Create
        public IActionResult Create()
        {
            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: CriticalControlPointParameterCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,CategoryDesc")] CriticalControlPointParameterCategory criticalControlPointParameterCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(criticalControlPointParameterCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Create";

            return View(criticalControlPointParameterCategory);
        }

        // GET: CriticalControlPointParameterCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameterCategory = await _context.CriticalControlPointParameterCategories.FindAsync(id);
            if (criticalControlPointParameterCategory == null)
            {
                return NotFound();
            }

            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Edit";

            return View(criticalControlPointParameterCategory);
        }

        // POST: CriticalControlPointParameterCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,CategoryDesc")] CriticalControlPointParameterCategory criticalControlPointParameterCategory)
        {
            if (id != criticalControlPointParameterCategory.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(criticalControlPointParameterCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CriticalControlPointParameterCategoryExists(criticalControlPointParameterCategory.CategoryId))
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

            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Edit";

            return View(criticalControlPointParameterCategory);
        }

        // GET: CriticalControlPointParameterCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criticalControlPointParameterCategory = await _context.CriticalControlPointParameterCategories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (criticalControlPointParameterCategory == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Critical Control Point Parameter Categories";
            ViewBag.Action = "Delete";

            return View(criticalControlPointParameterCategory);
        }

        // POST: CriticalControlPointParameterCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var criticalControlPointParameterCategory = await _context.CriticalControlPointParameterCategories.FindAsync(id);
            if (criticalControlPointParameterCategory != null)
            {
                _context.CriticalControlPointParameterCategories.Remove(criticalControlPointParameterCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CriticalControlPointParameterCategoryExists(int id)
        {
            return _context.CriticalControlPointParameterCategories.Any(e => e.CategoryId == id);
        }
    }
}
