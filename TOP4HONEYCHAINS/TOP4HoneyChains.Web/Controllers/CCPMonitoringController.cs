using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CCPMonitoringController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CCPMonitoringController(TOP4HoneyChainsContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> RawHoneyProductionManageCCP(int rawHoneyProductionId, int ccpId)
        {
            var parameters = await _context.CriticalControlPointParameters
                .Include(p => p.ParameterCategoryNavigation)
                .Where(p => p.CCPId == ccpId)
                .ToListAsync();

            var existingLogs = await _context.CriticalControlPointMonitoringLogs
                .Where(x => x.RawHoneyProductionId == rawHoneyProductionId && x.CCPId == ccpId)
                .ToListAsync();

            var grouped = parameters
                .GroupBy(p => p.ParameterCategoryNavigation)
                .Select(g => new CCPParameterGroup
                {
                    CategoryName = g.Key?.CategoryName ?? "Other",
                    CategoryDescription = g.Key?.CategoryDesc,
                    Parameters = g.Select(p =>
                    {
                        var log = existingLogs
                        .FirstOrDefault(x => x.ParameterId == p.ParameterId);

                        return new CCPParameterInput
                        {
                            ParameterId = p.ParameterId,
                            ParameterName = p.ParameterName!,
                            Unit = p.ParameterUnit,
                            CriticalLimit = p.CriticalLimit,
                            MeasuredNumericValue = log?.MeasuredNumericValue,
                            MeasuredTextValue = log?.MeasuredTextValue,
                            Notes = log?.Notes
                        };
                    }).ToList()
                }).ToList();


            var model = new CCPMonitoringViewModelForRawHoney
            {
                CCPId = ccpId,
                RawHoneyProductionId = rawHoneyProductionId,
                Groups = grouped
            };

            var ccp = await _context.CriticalControlPoints.FirstOrDefaultAsync(x => x.Id == ccpId);

            model.CCPCode = ccp.Code;
            model.CCPTitle = ccp.Title;
            model.ProcessingStep = ccp.ProcessingStep;
            model.CriticalityJustification = ccp.CriticalityJustification;

            var production = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == rawHoneyProductionId);

            model.BeekeeperName = production.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperFirstName + " " + production.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperLastName;
            model.ApiaryName = production.RawHoneyProductionApiary.ApiaryTitle;
            model.HoneyType = production.RawHoneyProductionHoneyTypeNavigation.HoneyTypeTitle;
            model.ProductionDate = production.RawHoneyProductionDate.HasValue
    ? production.RawHoneyProductionDate.Value.ToDateTime(TimeOnly.MinValue)
    : (DateTime?)null;
            model.Amount = production.RawHoneyProductionAmount;
            model.ApiaryId = production.RawHoneyProductionApiaryId.Value;


            ViewBag.Controller = "Raw Honey Production Manage CCP";
            ViewBag.Action = "Manage";


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RawHoneyProductionManageCCP(CCPMonitoringViewModelForRawHoney model)
        {
            // 1️⃣ Mevcut logları tek seferde çek
            var existingLogs = await _context.CriticalControlPointMonitoringLogs
                .Where(x =>
                    x.RawHoneyProductionId == model.RawHoneyProductionId &&
                    x.CCPId == model.CCPId)
                .ToListAsync();

            foreach (var group in model.Groups)
            {
                foreach (var param in group.Parameters)
                {
                    var existingLog = existingLogs
                        .FirstOrDefault(x => x.ParameterId == param.ParameterId);

                    var hasValue =
                        param.MeasuredNumericValue != null ||
                        !string.IsNullOrWhiteSpace(param.MeasuredTextValue);

                    // 🔹 Değer yok ama DB'de varsa → SİL
                    if (!hasValue && existingLog != null)
                    {
                        _context.CriticalControlPointMonitoringLogs.Remove(existingLog);
                        continue;
                    }

                    // 🔹 Yeni değer varsa ve DB'de yoksa → EKLE
                    if (hasValue && existingLog == null)
                    {
                        _context.CriticalControlPointMonitoringLogs.Add(
                            new CriticalControlPointMonitoringLog
                            {
                                CCPId = model.CCPId,
                                RawHoneyProductionId = model.RawHoneyProductionId,
                                ParameterId = param.ParameterId,
                                MeasuredNumericValue = param.MeasuredNumericValue,
                                MeasuredTextValue = param.MeasuredTextValue,
                                MeasurementDate = DateTime.Now,
                                Notes = param.Notes
                            });
                    }

                    // 🔹 Güncelle
                    if (hasValue && existingLog != null)
                    {
                        existingLog.MeasuredNumericValue = param.MeasuredNumericValue;
                        existingLog.MeasuredTextValue = param.MeasuredTextValue;
                        existingLog.MeasurementDate = DateTime.Now;
                        existingLog.Notes = param.Notes;
                    }
                }
            }

            // Önce log değişikliklerini kaydet
            await _context.SaveChangesAsync();

            // 2CCP Completion kontrolü

            var totalParameters = await _context.CriticalControlPointParameters
                .CountAsync(p => p.CCPId == model.CCPId);

            var loggedParameters = await _context.CriticalControlPointMonitoringLogs
                .Where(x =>
                x.RawHoneyProductionId == model.RawHoneyProductionId && x.CCPId == model.CCPId && (x.MeasuredNumericValue != null || !string.IsNullOrWhiteSpace(x.MeasuredTextValue)))
                .Select(x => x.ParameterId)
                .Distinct()
                .CountAsync();


            var assignment = await _context.RawHoneyCCPAssignments
                .FirstOrDefaultAsync(x => x.RawHoneyProductionId == model.RawHoneyProductionId && x.RawHoneyCCPAssignmentCCP == model.CCPId);

            if (assignment != null)
            {
                // 🔹 Tamamlandıysa
                if (loggedParameters == totalParameters && totalParameters > 0)
                {
                    assignment.RawHoneyCCPAssignmentStatus = 3; // Completed
                    assignment.RawHoneyCCPAssignmentEndDate = DateTime.Now;
                }
                else
                {
                    // 🔹 Eksik varsa tekrar In Progress
                    assignment.RawHoneyCCPAssignmentStatus = 2; // In Progress
                    assignment.RawHoneyCCPAssignmentEndDate = null;
                }

                await _context.SaveChangesAsync();
            }


            //var loggedParameters = existingLogs
            //    .Where(x => x.MeasuredNumericValue != null ||
            //                !string.IsNullOrWhiteSpace(x.MeasuredTextValue))
            //    .Select(x => x.ParameterId)
            //    .Distinct()
            //    .Count();

            //if (loggedParameters == totalParameters && totalParameters > 0)
            //{
            //    var assignment = await _context.RawHoneyCCPAssignments
            //        .FirstOrDefaultAsync(x =>
            //            x.RawHoneyProductionId == model.RawHoneyProductionId &&
            //            x.RawHoneyCCPAssignmentCCP == model.CCPId);

            //    if (assignment != null)
            //    {
            //        assignment.RawHoneyCCPAssignmentStatus = 3; // Completed
            //        assignment.RawHoneyCCPAssignmentEndDate = DateTime.Now;
            //    }
            //}


            ViewBag.Controller = "Raw Honey Production Manage CCP";
            ViewBag.Action = "Manage";

            return RedirectToAction("RawHoneyProductionByApiary", "RawHoneyProductions",
                new { id = model.ApiaryId });
        }




        public async Task<IActionResult> FinalHoneyProductionManageCCP(int finalHoneyProductionId, int ccpId)
        {
            var parameters = await _context.CriticalControlPointParameters
                .Include(p => p.ParameterCategoryNavigation)
                .Where(p => p.CCPId == ccpId)
                .ToListAsync();

            var existingLogs = await _context.CriticalControlPointMonitoringLogs
                .Where(x => x.FinalHoneyProductionId == finalHoneyProductionId && x.CCPId == ccpId)
                .ToListAsync();

            var grouped = parameters
                .GroupBy(p => p.ParameterCategoryNavigation)
                .Select(g => new CCPParameterGroup
                {
                    CategoryName = g.Key?.CategoryName ?? "Other",
                    CategoryDescription = g.Key?.CategoryDesc,
                    Parameters = g.Select(p =>
                    {
                        var log = existingLogs.FirstOrDefault(x => x.ParameterId == p.ParameterId);

                        return new CCPParameterInput
                        {
                            ParameterId = p.ParameterId,
                            ParameterName = p.ParameterName!,
                            Unit = p.ParameterUnit,
                            CriticalLimit = p.CriticalLimit,
                            MeasuredNumericValue = log?.MeasuredNumericValue,
                            MeasuredTextValue = log?.MeasuredTextValue,
                            Notes = log?.Notes
                        };
                    }).ToList()
                }).ToList();

            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionWholesaler)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionApiary)
                            .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == finalHoneyProductionId);

            var model = new CCPMonitoringViewModelForFinalBatch
            {
                CCPId = ccpId,
                FinalHoneyProductionId = finalHoneyProductionId,
                Groups = grouped,

                BatchTitle = batch.FinalHoneyProductionTitle,
                BatchQR = batch.FinalHoneyProductionQR,
                ProductionDate = batch.FinalHoneyProductionDate,
                TotalAmount = batch.FinalHoneyProductionTotalAmount,
                WholesalerName = batch.FinalHoneyProductionWholesaler.WholesalerTitle,
                WholesalerId = batch.FinalHoneyProductionWholesalerId,
                RawSourceCount = batch.FinalHoneyProductionDetails.Count
            };

            foreach (var detail in batch.FinalHoneyProductionDetails)
            {
                var raw = detail.RawHoneyProduction;

                model.RawSources.Add(new FinalBatchRawSourceInfo
                {
                    ApiaryName = raw.RawHoneyProductionApiary?.ApiaryTitle,
                    BeekeeperName = raw.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperFirstName
                                    + " "
                                    + raw.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperLastName,
                    HoneyType = raw.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                    UsedAmount = detail.UsedAmount
                });
            }

            var ccp = await _context.CriticalControlPoints.FirstOrDefaultAsync(x => x.Id == ccpId);

            model.CCPCode = ccp.Code;
            model.CCPTitle = ccp.Title;
            model.ProcessingStep = ccp.ProcessingStep;
            model.CriticalityJustification = ccp.CriticalityJustification;


            ViewBag.Controller = "Final Honey Production Manage CCP";
            ViewBag.Action = "Manage";

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalHoneyProductionManageCCP(CCPMonitoringViewModelForFinalBatch model)
        {
            var existingLogs = await _context.CriticalControlPointMonitoringLogs
                .Where(x =>
                    x.FinalHoneyProductionId == model.FinalHoneyProductionId &&
                    x.CCPId == model.CCPId)
                .ToListAsync();

            foreach (var group in model.Groups)
            {
                foreach (var param in group.Parameters)
                {
                    var existingLog = existingLogs
                        .FirstOrDefault(x => x.ParameterId == param.ParameterId);

                    var hasValue =
                        param.MeasuredNumericValue != null ||
                        !string.IsNullOrWhiteSpace(param.MeasuredTextValue);

                    if (!hasValue && existingLog != null)
                    {
                        _context.CriticalControlPointMonitoringLogs.Remove(existingLog);
                        continue;
                    }

                    if (hasValue && existingLog == null)
                    {
                        _context.CriticalControlPointMonitoringLogs.Add(
                            new CriticalControlPointMonitoringLog
                            {
                                CCPId = model.CCPId,
                                FinalHoneyProductionId = model.FinalHoneyProductionId,
                                ParameterId = param.ParameterId,
                                MeasuredNumericValue = param.MeasuredNumericValue,
                                MeasuredTextValue = param.MeasuredTextValue,
                                MeasurementDate = DateTime.Now,
                                Notes = param.Notes
                            });
                    }

                    if (hasValue && existingLog != null)
                    {
                        existingLog.MeasuredNumericValue = param.MeasuredNumericValue;
                        existingLog.MeasuredTextValue = param.MeasuredTextValue;
                        existingLog.MeasurementDate = DateTime.Now;
                        existingLog.Notes = param.Notes;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Completion logic
            var totalParameters = await _context.CriticalControlPointParameters
                .CountAsync(p => p.CCPId == model.CCPId);

            var loggedParameters = await _context.CriticalControlPointMonitoringLogs
                .Where(x =>
                    x.FinalHoneyProductionId == model.FinalHoneyProductionId &&
                    x.CCPId == model.CCPId &&
                    (x.MeasuredNumericValue != null ||
                     !string.IsNullOrWhiteSpace(x.MeasuredTextValue)))
                .Select(x => x.ParameterId)
                .Distinct()
                .CountAsync();

           

            if (loggedParameters == totalParameters && totalParameters > 0)
            {
                var assignment = await _context.FinalHoneyCCPAssignments
                    .FirstOrDefaultAsync(x =>
                        x.FinalHoneyProductionId == model.FinalHoneyProductionId &&
                        x.FinalHoneyCCPAssignmentCCP == model.CCPId);

                if (assignment != null)
                {
                    assignment.FinalHoneyCCPAssignmentStatus = 3;
                    assignment.FinalHoneyCCPAssignmentEndDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            var batch = await _context.FinalHoneyProductions
                .FirstOrDefaultAsync(x => x.FinalHoneyProductionId == model.FinalHoneyProductionId);


            ViewBag.Controller = "Final Honey Production Manage CCP";
            ViewBag.Action = "Manage";

            return RedirectToAction("ByWholesaler",
                "FinalHoneyProductions",
                new { wholesalerId = batch.FinalHoneyProductionWholesalerId });
        }

    }
}
