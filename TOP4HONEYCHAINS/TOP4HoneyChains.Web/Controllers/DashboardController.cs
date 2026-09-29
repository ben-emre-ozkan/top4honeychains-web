using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DashboardController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public DashboardController(TOP4HoneyChainsContext context)
        {

            _context = context;
        }


        //public IActionResult SetLanguage(string culture, string returnUrl)
        //{
        //    Response.Cookies.Append(
        //        CookieRequestCultureProvider.DefaultCookieName,
        //        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        //        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        //    );

        //    return LocalRedirect(returnUrl);
        //}


        public async Task<IActionResult> Index()
        {

            var totalWholesalers = await _context.Wholesalers.CountAsync();

            var totalBeekeepers = await _context.Beekeepers.CountAsync();

            var totalApiaries = await _context.Apiaries.CountAsync();

            var activeApiaries = await _context.Apiaries
                .Where(a => a.ApiaryIsActive)
                .CountAsync();

            var totalProduction = await _context.RawHoneyProductions
                .SumAsync(r => (decimal?)r.RawHoneyProductionAmount) ?? 0;

            // 🍯 Honey Type bazlı üretim
            var productionByHoneyType = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .GroupBy(r => r.RawHoneyProductionHoneyTypeNavigation.HoneyTypeTitle)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Sum(x => x.RawHoneyProductionAmount)
                })
                .ToListAsync();


            // Period bazlı toplam üretim
            var productionByPeriod = await _context.RawHoneyProductions
                .Include(p => p.RawHoneyProductionPeriod)
                .GroupBy(p => p.RawHoneyProductionPeriod.RawHoneyProductionPeriodTitle)
                .Select(g => new
                {
                    Period = g.Key,
                    TotalAmount = g.Sum(x => x.RawHoneyProductionAmount)
                })
                .ToListAsync();

            var topBeekeepers = await _context.RawHoneyProductions.Include(p => p.RawHoneyProductionApiary).ThenInclude(a => a.ApiaryBeekeeper)
                                        .GroupBy(p => new
                                        {
                                            p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperId,
                                            p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperFirstName,
                                            p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperLastName,
                                            p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperProfilePhoto
                                        })
                                        .Select(g => new
                                        {
                                            FullName = g.Key.BeekeeperFirstName + " " + g.Key.BeekeeperLastName,
                                            TotalProduction = g.Sum(x => x.RawHoneyProductionAmount),
                                            Photo = g.Key.BeekeeperProfilePhoto
                                        })
                                        .OrderByDescending(x => x.TotalProduction)
                                        .Take(5)
                                        .ToListAsync();

            var topApiaries = await _context.RawHoneyProductions
                .Include(p => p.RawHoneyProductionApiary)
                .ThenInclude(a => a.ApiaryBeekeeper)
                .GroupBy(p => new
                {
                    p.RawHoneyProductionApiary.ApiaryId,
                    p.RawHoneyProductionApiary.ApiaryTitle,
                    p.RawHoneyProductionApiary.ApiaryBeehivesCount,
                    BeekeeperName = p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperFirstName + " " +
                        p.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperLastName
                })
                .Select(g => new
                {
                    ApiaryId = g.Key.ApiaryId,
                    ApiaryTitle = g.Key.ApiaryTitle,
                    BeehivesCount = g.Key.ApiaryBeehivesCount,
                    BeekeeperName = g.Key.BeekeeperName,
                    TotalProduction = g.Sum(x => x.RawHoneyProductionAmount)
                })
                .OrderByDescending(x => x.TotalProduction)
                .Take(5)
                .ToListAsync();


            var productionByPeriodAndType = await _context.RawHoneyProductions
             .Include(p => p.RawHoneyProductionPeriod)
             .Include(p => p.RawHoneyProductionHoneyTypeNavigation)
             .GroupBy(p => new
             {
                 Period = p.RawHoneyProductionPeriod.RawHoneyProductionPeriodTitle,
                 HoneyType = p.RawHoneyProductionHoneyTypeNavigation.HoneyTypeTitle
             })
             .Select(g => new
             {
                 Period = g.Key.Period,
                 HoneyType = g.Key.HoneyType,
                 TotalAmount = g.Sum(x => x.RawHoneyProductionAmount)
             })
             .ToListAsync();

            var periods = productionByPeriodAndType
    .Select(x => x.Period)
    .Distinct()
    .ToList();

            var honeyTypes = productionByPeriodAndType
                .Select(x => x.HoneyType)
                .Distinct()
                .ToList();




            var chartData = new List<Dictionary<string, object>>();

            foreach (var period in periods)
            {
                var row = new Dictionary<string, object>();
                row["period"] = period;

                foreach (var type in honeyTypes)
                {
                    var value = productionByPeriodAndType
                        .Where(x => x.Period == period && x.HoneyType == type)
                        .Sum(x => x.TotalAmount);

                    row[type] = value;
                }

                chartData.Add(row);
            }

            ViewBag.PeriodChartData = chartData;
            ViewBag.HoneyTypes = honeyTypes;


            var productionByWholesaler = await _context.RawHoneyProductions
     .Include(r => r.RawHoneyProductionWholesaler)
     .Where(r => r.RawHoneyProductionWholesalerId != null)
     .GroupBy(r => r.RawHoneyProductionWholesaler.WholesalerTitle)
     .Select(g => new
     {
         label = g.Key,
         value = g.Sum(x => x.RawHoneyProductionAmount)
     })
     .OrderByDescending(x => x.value)
     .Take(3)
     .ToListAsync();

            ViewBag.ProductionByWholesaler = productionByWholesaler;


            ViewBag.TopApiaries = topApiaries;


            ViewBag.TotalWholesalers = totalWholesalers;

            ViewBag.TopBeekeepers = topBeekeepers;


            ViewBag.PeriodLabels = productionByPeriod.Select(x => x.Period).ToList();
            ViewBag.PeriodAmounts = productionByPeriod.Select(x => x.TotalAmount).ToList();

            ViewBag.TotalBeekeepers = totalBeekeepers;
            ViewBag.TotalApiaries = totalApiaries;
            ViewBag.ActiveApiaries = activeApiaries;
            ViewBag.TotalProduction = totalProduction;

            ViewBag.ProductionByHoneyType = productionByHoneyType;

            ViewBag.Controller = "Dashboard";
            ViewBag.Action = "Index";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
