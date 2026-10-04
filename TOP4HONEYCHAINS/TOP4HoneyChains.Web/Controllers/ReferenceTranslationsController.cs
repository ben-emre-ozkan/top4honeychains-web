using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-51: translations of reference data (stored in ReferenceTranslations). The base column of each reference
    // table stays the fallback and is edited on the table's own page. Every change is audited by the interceptor.
    [Authorize(Roles = "Administrator")]
    public class ReferenceTranslationsController : Controller
    {
        public static readonly string[] Languages = { "tr", "en", "es", "pl", "lv" };
        private readonly TOP4HoneyChainsContext _context;
        private readonly RefText _ref;
        private readonly ICurrentOrg _current;

        public ReferenceTranslationsController(TOP4HoneyChainsContext context, RefText refText, ICurrentOrg current)
        {
            _context = context;
            _ref = refText;
            _current = current;
        }

        public record Row(string Key, string Base, Dictionary<string, string> Values);

        // key -> base text of one field of one entity
        private async Task<List<(string Key, string Base)>> BaseTextsAsync(string entity, string field)
        {
            static string K(int id) => id.ToString(CultureInfo.InvariantCulture);
            switch (entity, field)
            {
                case (RefEntity.HoneyType, "Title"): return (await _context.HoneyTypes.AsNoTracking().OrderBy(x => x.HoneyTypeId).ToListAsync()).Select(x => (K(x.HoneyTypeId), x.HoneyTypeTitle ?? "")).ToList();
                case (RefEntity.HoneyType, "Description"): return (await _context.HoneyTypes.AsNoTracking().OrderBy(x => x.HoneyTypeId).ToListAsync()).Select(x => (K(x.HoneyTypeId), x.HoneyTypeDescription ?? "")).ToList();
                case (RefEntity.ProductionPeriod, "Title"): return (await _context.RawHoneyProductionPeriods.AsNoTracking().OrderBy(x => x.RawHoneyProductionPeriodId).ToListAsync()).Select(x => (K(x.RawHoneyProductionPeriodId), x.RawHoneyProductionPeriodTitle ?? "")).ToList();
                case (RefEntity.EducationLevel, "Title"): return (await _context.BeekeeperEducationLevels.AsNoTracking().OrderBy(x => x.BeekeeperEducationLevelId).ToListAsync()).Select(x => (K(x.BeekeeperEducationLevelId), x.BeekeeperEducationLevelTitle ?? "")).ToList();
                case (RefEntity.BeekeepingPurposeType, "Title"): return (await _context.BeekeepingPurposeTypes.AsNoTracking().OrderBy(x => x.BeekeepingPurposeTypeId).ToListAsync()).Select(x => (K(x.BeekeepingPurposeTypeId), x.BeekeepingPurposeTypeTitle ?? "")).ToList();
                case (RefEntity.BeekeepingType, "Title"): return (await _context.BeekeepingTypes.AsNoTracking().OrderBy(x => x.BeekeepingTypeId).ToListAsync()).Select(x => (K(x.BeekeepingTypeId), x.BeekeepingTypeTitle ?? "")).ToList();
                case (RefEntity.AssignmentStatus, "Name"): return (await _context.AssignmentStatuses.AsNoTracking().OrderBy(x => x.AssignmentStatusId).ToListAsync()).Select(x => (K(x.AssignmentStatusId), x.AssignmentStatusName ?? "")).ToList();
                case (RefEntity.Capability, "Name"): return (await _context.Capabilities.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync()).Select(x => (x.Code, x.Code)).ToList();
                case (RefEntity.Capability, "Description"): return (await _context.Capabilities.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync()).Select(x => (x.Code, x.Description ?? "")).ToList();
                case (RefEntity.OrgRole, "Name"): return OrgRoles.All.Select(r => (r, r)).ToList();
                case (RefEntity.Ccp, "Title"): return (await _context.CriticalControlPoints.AsNoTracking().OrderBy(x => x.Id).ToListAsync()).Select(x => (K(x.Id), $"{x.Code}: {x.Title}")).ToList();
                case (RefEntity.Ccp, "ProcessingStep"): return (await _context.CriticalControlPoints.AsNoTracking().OrderBy(x => x.Id).ToListAsync()).Select(x => (K(x.Id), x.ProcessingStep ?? "")).ToList();
                case (RefEntity.Ccp, "CriticalityJustification"): return (await _context.CriticalControlPoints.AsNoTracking().OrderBy(x => x.Id).ToListAsync()).Select(x => (K(x.Id), x.CriticalityJustification ?? "")).ToList();
                case (RefEntity.ParameterCategory, "Name"): return (await _context.CriticalControlPointParameterCategories.AsNoTracking().OrderBy(x => x.CategoryId).ToListAsync()).Select(x => (K(x.CategoryId), x.CategoryName ?? "")).ToList();
                case (RefEntity.ParameterCategory, "Description"): return (await _context.CriticalControlPointParameterCategories.AsNoTracking().OrderBy(x => x.CategoryId).ToListAsync()).Select(x => (K(x.CategoryId), x.CategoryDesc ?? "")).ToList();
                case (RefEntity.Parameter, "Name"): return (await _context.CriticalControlPointParameters.AsNoTracking().OrderBy(x => x.ParameterId).ToListAsync()).Select(x => (K(x.ParameterId), x.ParameterName ?? "")).ToList();
                case (RefEntity.Parameter, "Unit"): return (await _context.CriticalControlPointParameters.AsNoTracking().Where(x => x.ParameterUnit != null && x.ParameterUnit != "None").OrderBy(x => x.ParameterId).ToListAsync()).Select(x => (K(x.ParameterId), x.ParameterUnit!)).ToList();
                case (RefEntity.Parameter, "LimitNote"): return (await _context.CriticalControlPointParameters.AsNoTracking().Where(x => x.LimitNote != null).OrderBy(x => x.ParameterId).ToListAsync()).Select(x => (K(x.ParameterId), x.LimitNote!)).ToList();
                case (RefEntity.Parameter, "LimitSource"): return (await _context.CriticalControlPointParameters.AsNoTracking().Where(x => x.LimitSource != null).OrderBy(x => x.ParameterId).ToListAsync()).Select(x => (K(x.ParameterId), x.LimitSource!)).ToList();
                case (RefEntity.ParameterLimit, "Note"): return (await _context.CriticalControlPointParameterLimits.AsNoTracking().Where(x => x.Note != null).OrderBy(x => x.LimitId).ToListAsync()).Select(x => (K(x.LimitId), x.Note!)).ToList();
                case (RefEntity.ParameterContent, "WhatIsIt"): return (await _context.CriticalControlPointParameterContents.AsNoTracking().OrderBy(x => x.ContentId).ToListAsync()).Select(x => (K(x.ContentId), x.WhatIsIt ?? "")).ToList();
                case (RefEntity.ParameterContent, "WhyImportant"): return (await _context.CriticalControlPointParameterContents.AsNoTracking().OrderBy(x => x.ContentId).ToListAsync()).Select(x => (K(x.ContentId), x.WhyImportant ?? "")).ToList();
                case (RefEntity.ParameterContent, "ExtraInfo"): return (await _context.CriticalControlPointParameterContents.AsNoTracking().OrderBy(x => x.ContentId).ToListAsync()).Select(x => (K(x.ContentId), x.ExtraInfo ?? "")).ToList();
                case (RefEntity.Country, "Name"): return Origin.Countries.Select(c => (c.Code, $"{c.Code}: {c.En}")).ToList();
                default: return new();
            }
        }

        public async Task<IActionResult> Index(string? entity, string? field)
        {
            entity = entity != null && RefEntity.Fields.ContainsKey(entity) ? entity : RefEntity.HoneyType;
            field = field != null && RefEntity.Fields[entity].Contains(field) ? field : RefEntity.Fields[entity][0];
            var rows = await BaseTextsAsync(entity, field);
            var stored = await _context.ReferenceTranslations.AsNoTracking().Where(t => t.Entity == entity && t.Field == field).ToListAsync();
            ViewBag.Entity = entity;
            ViewBag.Field = field;
            ViewBag.Controller = "Translations";
            ViewBag.Action = "Index";
            return View(rows.Select(r => new Row(r.Key, r.Base,
                stored.Where(t => t.EntityKey == r.Key).ToDictionary(t => t.Culture, t => t.Value))).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string entity, string field)
        {
            if (!RefEntity.Fields.TryGetValue(entity, out var fields) || !fields.Contains(field))
                return NotFound();
            var keys = (await BaseTextsAsync(entity, field)).Select(r => r.Key).ToHashSet();
            var stored = await _context.ReferenceTranslations.Where(t => t.Entity == entity && t.Field == field).ToListAsync();
            var changed = 0;
            foreach (var key in keys)
                foreach (var lang in Languages)
                {
                    var posted = Request.Form[$"v[{key}][{lang}]"].ToString().Trim();
                    if (posted.Length > 4000)
                        posted = posted[..4000];
                    var row = stored.FirstOrDefault(t => t.EntityKey == key && t.Culture == lang);
                    if (row == null && posted.Length > 0)
                    {
                        _context.ReferenceTranslations.Add(new ReferenceTranslation { Entity = entity, EntityKey = key, Field = field, Culture = lang, Value = posted, UpdatedAt = DateTime.UtcNow, UpdatedByUserId = _current.UserId });
                        changed++;
                    }
                    else if (row != null && posted.Length == 0)
                    {
                        _context.ReferenceTranslations.Remove(row);
                        changed++;
                    }
                    else if (row != null && row.Value != posted)
                    {
                        row.Value = posted;
                        row.UpdatedAt = DateTime.UtcNow;
                        row.UpdatedByUserId = _current.UserId;
                        changed++;
                    }
                }
            await _context.SaveChangesAsync();
            _ref.Invalidate();
            TempData["Success"] = $"{changed} translations saved.";
            return RedirectToAction(nameof(Index), new { entity, field });
        }
    }
}
