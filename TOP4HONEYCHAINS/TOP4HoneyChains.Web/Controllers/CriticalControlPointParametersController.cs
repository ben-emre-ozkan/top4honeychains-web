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
            var tOP4HoneyChainsContext = _context.CriticalControlPointParameters.Include(c => c.CCP).Include(c => c.ParameterCategoryNavigation)
                .Include(c => c.Limits).ThenInclude(l => l.ConditionParameter)
                .Include(c => c.Limits).ThenInclude(l => l.SumWithParameter)
                .Include(c => c.Limits).ThenInclude(l => l.RatioWithParameter)
                .Include(c => c.Limits).ThenInclude(l => l.HoneyType);

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
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName").Translated(HttpContext, "ParameterCategory", "Name");

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Create";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CCPId,ParameterName,ParameterCategory,ParameterUnit,CriticalLimit,DataType,MinValue,MaxValue,IsRequired,LimitNote,LimitSource,LimitVerified")] CriticalControlPointParameter criticalControlPointParameter)
        {
            ValidateLimits(criticalControlPointParameter.MinValue, criticalControlPointParameter.MaxValue);

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
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory).Translated(HttpContext, "ParameterCategory", "Name");

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Create";

            return View(criticalControlPointParameter);
        }

        // GET: CriticalControlPointParameters/Edit/5
        // the yes/no questions of the same CCP a parameter's requirement can depend on (D-62)
        private SelectList RequiredIfOptions(CriticalControlPointParameter p) =>
            new SelectList(_context.CriticalControlPointParameters.AsNoTracking()
                .Where(x => x.CCPId == p.CCPId && x.ParameterId != p.ParameterId && x.DataType == CcpDataType.YesNo)
                .OrderBy(x => x.ParameterId).ToList(), "ParameterId", "ParameterName", p.RequiredIfParameterId).Translated(HttpContext, "Parameter", "Name");

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
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory).Translated(HttpContext, "ParameterCategory", "Name");
            ViewData["RequiredIfParameterId"] = RequiredIfOptions(criticalControlPointParameter);

            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Edit";

            return View(criticalControlPointParameter);
        }

        // POST: CriticalControlPointParameters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParameterId,CCPId,ParameterName,ParameterCategory,ParameterUnit,CriticalLimit,DataType,MinValue,MaxValue,IsRequired,LimitNote,LimitSource,LimitVerified,RequiredIfParameterId,RequiredIfValue")] CriticalControlPointParameter criticalControlPointParameter)
        {
            if (id != criticalControlPointParameter.ParameterId)
            {
                return NotFound();
            }

            // D-62: the condition must be another yes/no parameter of the same CCP, with an answer
            if (criticalControlPointParameter.RequiredIfParameterId is int conditionId)
            {
                var condition = await _context.CriticalControlPointParameters.AsNoTracking().FirstOrDefaultAsync(p => p.ParameterId == conditionId);
                if (conditionId == id || condition == null || condition.CCPId != criticalControlPointParameter.CCPId || condition.DataType != CcpDataType.YesNo)
                    ModelState.AddModelError(nameof(criticalControlPointParameter.RequiredIfParameterId), "Choose a yes/no question of the same CCP.");
                else if (criticalControlPointParameter.RequiredIfValue is not ("Yes" or "No"))
                    ModelState.AddModelError(nameof(criticalControlPointParameter.RequiredIfValue), "Choose the answer that makes this parameter required.");
            }
            else
                criticalControlPointParameter.RequiredIfValue = null;

            ValidateLimits(criticalControlPointParameter.MinValue, criticalControlPointParameter.MaxValue);

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
            ViewData["ParameterCategory"] = new SelectList(_context.CriticalControlPointParameterCategories, "CategoryId", "CategoryName", criticalControlPointParameter.ParameterCategory).Translated(HttpContext, "ParameterCategory", "Name");
            ViewData["RequiredIfParameterId"] = RequiredIfOptions(criticalControlPointParameter);

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

        private void ValidateLimits(decimal? min, decimal? max)
        {
            if (min != null && max != null && min > max)
                ModelState.AddModelError("MaxValue", "The maximum must not be lower than the minimum.");
        }

        // ---- Honey-type specific, conditional and sum limits (D-18) ----

        public async Task<IActionResult> Limits(int id)
        {
            var parameter = await _context.CriticalControlPointParameters
                .Include(p => p.CCP)
                .Include(p => p.Limits).ThenInclude(l => l.ConditionParameter)
                .Include(p => p.Limits).ThenInclude(l => l.SumWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.RatioWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.HoneyType)
                .FirstOrDefaultAsync(p => p.ParameterId == id);
            if (parameter == null)
                return NotFound();

            ViewBag.Siblings = await _context.CriticalControlPointParameters
                .Where(p => p.CCPId == parameter.CCPId && p.ParameterId != id)
                .OrderBy(p => p.ParameterName)
                .ToListAsync();
            ViewBag.HoneyTypes = await _context.HoneyTypes.OrderBy(h => h.HoneyTypeTitle).ToListAsync();
            ViewBag.Controller = "Critical Control Point Parameters";
            ViewBag.Action = "Limits";
            return View(parameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLimit(int parameterId, string? honeyClass, int? honeyTypeId, int? conditionParameterId, decimal? conditionMaxValue,
            int? sumWithParameterId, int? ratioWithParameterId, decimal? minValue, decimal? maxValue, string? note)
        {
            var parameter = await _context.CriticalControlPointParameters.FindAsync(parameterId);
            if (parameter == null)
                return NotFound();

            honeyClass = string.IsNullOrWhiteSpace(honeyClass) ? null : honeyClass;
            string? error = null;
            if (honeyClass != null && !HoneyClasses.All.Contains(honeyClass))
                error = "Unknown honey class.";
            else if (minValue == null && maxValue == null)
                error = "Enter a minimum or a maximum.";
            else if (minValue != null && maxValue != null && minValue > maxValue)
                error = "The maximum must not be lower than the minimum.";
            else if ((conditionParameterId == null) != (conditionMaxValue == null))
                error = "A condition needs both a parameter and a value.";
            else if (note?.Length > 300)
                error = "The note can be at most 300 characters.";
            else if (sumWithParameterId != null && ratioWithParameterId != null)
                error = "A limit can use a sum or a ratio, not both.";
            else if (honeyTypeId != null && !await _context.HoneyTypes.AnyAsync(h => h.HoneyTypeId == honeyTypeId))
                error = "Unknown honey type.";
            else
            {
                foreach (var otherId in new[] { conditionParameterId, sumWithParameterId, ratioWithParameterId }.Where(x => x != null))
                {
                    var other = await _context.CriticalControlPointParameters.FindAsync(otherId);
                    if (other == null || other.CCPId != parameter.CCPId || other.ParameterId == parameterId)
                        error = "Condition, sum and ratio parameters must be other parameters of the same CCP.";
                }
            }

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                _context.CriticalControlPointParameterLimits.Add(new CriticalControlPointParameterLimit
                {
                    ParameterId = parameterId,
                    HoneyClass = honeyClass,
                    HoneyTypeId = honeyTypeId,
                    RatioWithParameterId = ratioWithParameterId,
                    ConditionParameterId = conditionParameterId,
                    ConditionMaxValue = conditionMaxValue,
                    SumWithParameterId = sumWithParameterId,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Limit added.";
            }
            return RedirectToAction(nameof(Limits), new { id = parameterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLimit(int limitId)
        {
            var limit = await _context.CriticalControlPointParameterLimits.FindAsync(limitId);
            if (limit == null)
                return NotFound();

            _context.CriticalControlPointParameterLimits.Remove(limit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Limit removed.";
            return RedirectToAction(nameof(Limits), new { id = limit.ParameterId });
        }

        private bool CriticalControlPointParameterExists(int id)
        {
            return _context.CriticalControlPointParameters.Any(e => e.ParameterId == id);
        }
    }
}
