using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-98: the short address in the QR of a seal label, a plastic seal's record or an association container (/s/{code}).
    // Signed-in members of the organization whose pool holds the code see its state; anyone else gets the sign-in page (with the
    // address to come back to), so the page never tells whether a code exists or whose it is. Anonymous: [Authorize] challenges.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class SealLinkController : Controller
    {
        public record SealView(string Code, SealCode? Seal, OrgContainer? Box, Container? Container, IReadOnlyList<ContainerEmptying> Checks,
            IReadOnlyList<Container> BoxUses, string? InUse);

        private readonly TOP4HoneyChainsContext _context;
        private readonly ContainerEmptyingService _emptying;

        public SealLinkController(TOP4HoneyChainsContext context, ContainerEmptyingService emptying)
        {
            _context = context;
            _emptying = emptying;
        }

        [HttpGet("/s/{code}")]
        public async Task<IActionResult> Show(string code)
        {
            var c = SealPoolService.Normalize(code);
            var seal = c == null ? null : await _context.SealCodes.AsNoTracking().FirstOrDefaultAsync(s => s.Code == c);
            var box = c == null || seal != null ? null : await _context.OrgContainers.AsNoTracking().FirstOrDefaultAsync(b => b.Code == c);
            var orgId = seal?.OrgId ?? box?.OrgId;
            var userId = User.Identity?.IsAuthenticated == true ? HttpContext.RequestServices.GetRequiredService<ICurrentOrg>().UserId : null;
            var member = orgId != null && userId != null
                && await _context.Memberships.AnyAsync(m => m.UserId == userId && m.OrgId == orgId && m.Status == MembershipStatus.Active);
            if (!member)
            {
                if (userId == null)
                    return Challenge();
                // signed in, but not a member of the issuing organization: the sign-in page, as for anyone else
                var login = HttpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                    .Get(IdentityConstants.ApplicationScheme).LoginPath;
                return Redirect(login + QueryString.Create("ReturnUrl", Request.PathBase + Request.Path).ToUriComponent());
            }

            Container? container = null;
            var boxUses = new List<Container>();
            string? inUse = null;
            if (seal != null)
            {
                var containerId = seal.ContainerId ?? await _context.Samples.Where(s => s.SampleId == seal.SampleId).Select(s => s.ContainerId).FirstOrDefaultAsync();
                if (containerId != null)
                    container = await _context.Containers.AsNoTracking().Include(x => x.Intake).Include(x => x.SealEvents).Include(x => x.Samples).Include(x => x.OrgContainer)
                        .FirstOrDefaultAsync(x => x.ContainerId == containerId);
            }
            else
            {
                boxUses = await _context.Containers.AsNoTracking().Include(x => x.Intake).Where(x => x.OrgContainerId == box!.OrgContainerId)
                    .OrderByDescending(x => x.ContainerId).Take(20).ToListAsync();
                inUse = (await _emptying.BoxesInUseAsync(new[] { box!.OrgContainerId })).GetValueOrDefault(box.OrgContainerId);
            }
            var checks = container == null ? new List<ContainerEmptying>()
                : (await _emptying.ChecksAsync(new[] { container.ContainerId })).GetValueOrDefault(container.ContainerId) ?? new List<ContainerEmptying>();
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Seals";
            return View(new SealView(c!, seal, box, container, checks, boxUses, inUse));
        }
    }
}
