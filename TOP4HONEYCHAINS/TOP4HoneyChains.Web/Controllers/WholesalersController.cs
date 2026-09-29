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
    public class WholesalersController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public WholesalersController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

        // GET: Wholesalers
        public async Task<IActionResult> Index()
        {


            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Index";

            return View(await _context.Wholesalers.ToListAsync());
        }

        // GET: Wholesalers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(m => m.WholesalerId == id);
            if (wholesaler == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Details";

            return View(wholesaler);
        }

        // GET: Wholesalers/Create
        public IActionResult Create()
        {

            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Create";

            return View();
        }

        // POST: Wholesalers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WholesalerId,WholesalerTitle,WholesalerTaxNo,WholesalerContactName,WholesalerPhone,WholesalerEmail,WholesalerAddress,WholesalerCity,WholesalerCountry,WholesalerIsActive,CreatedAt")] Wholesaler wholesaler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wholesaler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Create";

            return View(wholesaler);
        }

        // GET: Wholesalers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wholesaler = await _context.Wholesalers.FindAsync(id);
            if (wholesaler == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Edit";

            return View(wholesaler);
        }

        // POST: Wholesalers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WholesalerId,WholesalerTitle,WholesalerTaxNo,WholesalerContactName,WholesalerPhone,WholesalerEmail,WholesalerAddress,WholesalerCity,WholesalerCountry,WholesalerIsActive,CreatedAt")] Wholesaler wholesaler)
        {
            if (id != wholesaler.WholesalerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wholesaler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WholesalerExists(wholesaler.WholesalerId))
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


            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Edit";

            return View(wholesaler);
        }

        // GET: Wholesalers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(m => m.WholesalerId == id);
            if (wholesaler == null)
            {
                return NotFound();
            }



            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Delete";

            return View(wholesaler);
        }

        // POST: Wholesalers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wholesaler = await _context.Wholesalers.FindAsync(id);
            if (wholesaler != null)
            {
                _context.Wholesalers.Remove(wholesaler);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Wholesalers/RawHoney/5
        public async Task<IActionResult> RawHoneyProductions(int id)
        {
            var wholesaler = await _context.Wholesalers
        .FirstOrDefaultAsync(w => w.WholesalerId == id);

            if (wholesaler == null)
                return NotFound();

            var rawHoneys = await _context.RawHoneyProductions
                .Where(r => r.RawHoneyProductionWholesalerId == id)
                .Include(r => r.RawHoneyProductionApiary)
                .Include(r => r.RawHoneyProductionPeriod)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyCCPAssignments)
                .ThenInclude(a => a.AssignmentStatus)
                .OrderByDescending(r => r.RawHoneyProductionDate)
                .ToListAsync();

            ViewBag.Wholesaler = wholesaler;



            ViewBag.Controller = "Wholesalers";
            ViewBag.Action = "Raw Honey Productions";

            return View(rawHoneys);
        }


        private bool WholesalerExists(int id)
        {
            return _context.Wholesalers.Any(e => e.WholesalerId == id);
        }
    }
}
