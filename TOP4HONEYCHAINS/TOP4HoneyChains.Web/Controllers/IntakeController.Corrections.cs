using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P3 (D-82, P2 follow-up 1): the two-admin correction of a delivery recorded with containers, on the delivery page
    public partial class IntakeController
    {
        private DeliveryCorrectionService Corrections => HttpContext.RequestServices.GetRequiredService<DeliveryCorrectionService>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> RequestCorrection(int id, DeliveryCorrectionForm form)
        {
            if (!await _context.LotIntakes.AnyAsync(i => i.IntakeId == id))
                return NotFound();
            var (_, error) = await Corrections.RequestAsync(id, form, _current.UserId!);
            TempData[error == null ? "Success" : "Error"] = error ?? "Correction requested; another platform administrator must confirm it.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> DecideCorrection(int correctionId, bool approve, string? note)
        {
            var intakeId = await _context.DeliveryCorrections.Where(c => c.CorrectionId == correctionId).Select(c => (int?)c.IntakeId).FirstOrDefaultAsync();
            if (intakeId == null)
                return NotFound();
            var error = await Corrections.DecideAsync(correctionId, approve, note, _current.UserId!);
            TempData[error == null ? "Success" : "Error"] = error ?? (approve ? "Correction applied; the beekeeper was informed by SMS." : "Correction rejected.");
            return RedirectToAction(nameof(Details), new { id = intakeId });
        }

        // what the delivery page shows about corrections (the request form only to platform administrators)
        private async Task CorrectionsViewAsync(LotIntake intake)
        {
            ViewBag.DeliveryCorrections = await _context.DeliveryCorrections.AsNoTracking().Where(c => c.IntakeId == intake.IntakeId)
                .OrderByDescending(c => c.CorrectionId).ToListAsync();
            if (_current.IsPlatformAdmin && !intake.IsLegacy && await Custody.StoredContentAsync(intake.IntakeId) is { } stored)
                ViewBag.CorrectionForm = DeliveryCorrectionService.FormOf(stored);
            ViewBag.UserId = _current.UserId;
        }
    }
}
