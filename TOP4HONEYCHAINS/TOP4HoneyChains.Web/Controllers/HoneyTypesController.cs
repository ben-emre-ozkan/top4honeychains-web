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
    public class HoneyTypesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public HoneyTypesController(TOP4HoneyChainsContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("HoneyTypeId,HoneyTypeTitle,HoneyTypeDescription")] HoneyType honeyType)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> Edit(int id, [Bind("HoneyTypeId,HoneyTypeTitle,HoneyTypeDescription")] HoneyType honeyType)
        {
            if (id != honeyType.HoneyTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(honeyType);
                    await _context.SaveChangesAsync();
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

        private bool HoneyTypeExists(int id)
        {
            return _context.HoneyTypes.Any(e => e.HoneyTypeId == id);
        }
    }
}
