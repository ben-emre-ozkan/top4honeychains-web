using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D78-SPEC 2.4, 3.8, 4.7, D-82): the beekeeper portal /Me. Every query is filtered by the beekeeper linked to the
    // signed-in account (never by an id from the page); an id that is not theirs is simply not found. The only business write is
    // the asking price of their own lots; besides it their consents, phone, language and password.
    public class BeekeeperPortalService
    {
        public static readonly string[] Currencies = { "TRY", "EUR", "USD", "ARS", "PLN" };

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;
        private readonly TimeProvider _clock;

        public BeekeeperPortalService(TOP4HoneyChainsContext context, CcpIntegrityService ccp, TimeProvider clock)
        {
            _context = context;
            _ccp = ccp;
            _clock = clock;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        // the active beekeeper of an active beekeeper account, or null
        public Task<Beekeeper?> BeekeeperOfAsync(string? userId) => userId == null ? Task.FromResult<Beekeeper?>(null)
            : _context.BeekeeperAccounts.AsNoTracking()
                .Where(a => a.UserId == userId && a.Status == BeekeeperAccountStatus.Active && a.Beekeeper!.IsActive)
                .Select(a => a.Beekeeper).FirstOrDefaultAsync();

        public IQueryable<RawHoneyProduction> Lots(string beekeeperId) =>
            _context.RawHoneyProductions.Where(r => r.RawHoneyProductionApiary!.ApiaryBeekeeperId == beekeeperId);

        public static string DefaultCurrency(string? origin) => origin switch
        {
            "TR" => "TRY", "AR" => "ARS", "PL" => "PLN", _ => "EUR"
        };

        // ---------------------------------------------------------------- deliveries and prices

        public record Delivery(LotIntake Intake, RawHoneyProduction Lot, List<Container> Containers, List<LotPrice> Prices, string? PriceDenied,
            bool Corrected);

        public async Task<List<Delivery>> DeliveriesAsync(string beekeeperId)
        {
            var lots = await Lots(beekeeperId).AsNoTracking().Include(r => r.RawHoneyProductionHoneyTypeNavigation).Include(r => r.RawHoneyProductionApiary)
                .ToDictionaryAsync(r => r.RawHoneyProductionId);
            var ids = lots.Keys.ToList();
            var intakes = await _context.LotIntakes.AsNoTracking().Include(i => i.ReceivingOrg)
                .Include(i => i.Containers).ThenInclude(c => c.SealEvents)
                .Where(i => ids.Contains(i.RawHoneyProductionId)).OrderByDescending(i => i.DeliveryDate).ThenByDescending(i => i.IntakeId).ToListAsync();
            var prices = (await _context.LotPrices.AsNoTracking().Where(p => ids.Contains(p.RawHoneyProductionId)).OrderByDescending(p => p.PriceId).ToListAsync())
                .GroupBy(p => p.RawHoneyProductionId).ToDictionary(g => g.Key, g => g.ToList());
            var locked = await _ccp.GetLockedRawLotIdsAsync(ids);
            var corrected = (await _context.DeliveryCorrections.AsNoTracking().Where(c => ids.Contains(c.RawHoneyProductionId) && c.Status == CorrectionStatus.Applied)
                .Select(c => c.IntakeId).ToListAsync()).ToHashSet();
            return intakes.Select(i => new Delivery(i, lots[i.RawHoneyProductionId], i.Containers.OrderBy(c => c.Seq).ToList(),
                prices.GetValueOrDefault(i.RawHoneyProductionId) ?? new(), PriceRule(i, locked.Contains(i.RawHoneyProductionId)), corrected.Contains(i.IntakeId))).ToList();
        }

        // O-5: from the confirmation of the delivery until the lot is in a finalized batch
        public static string? PriceRule(LotIntake intake, bool locked) =>
            locked ? "The lot is in a finalized batch; its price is locked."
            : !intake.IsLegacy && intake.EvidenceStatus != EvidenceStatus.Accepted ? "The delivery is not confirmed yet; the price can be entered once it is."
            : null;

        // Found = false: not their lot or not a delivery (404)
        public async Task<(bool Found, string? Error)> SetPriceAsync(string beekeeperId, string userId, int lotId, decimal? pricePerKg, string? currency, string? note, string? ip)
        {
            var intake = await _context.LotIntakes.AsNoTracking()
                .FirstOrDefaultAsync(i => i.RawHoneyProductionId == lotId && Lots(beekeeperId).Any(r => r.RawHoneyProductionId == lotId));
            if (intake == null)
                return (false, null);
            if (PriceRule(intake, (await _ccp.GetLockedRawLotIdsAsync(new[] { lotId })).Count > 0) is string denied)
                return (true, denied);
            if (pricePerKg is not (> 0 and < 100000) || decimal.Round(pricePerKg.Value, 2) != pricePerKg.Value)
                return (true, "Enter the price per kg (more than 0, at most two decimals).");
            currency = currency?.Trim().ToUpperInvariant();
            if (!Currencies.Contains(currency))
                return (true, "Choose a currency.");
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (note?.Length > 200)
                return (true, "The note can be at most 200 characters.");
            _context.LotPrices.Add(new LotPrice
            {
                RawHoneyProductionId = lotId, PricePerKg = pricePerKg.Value, Currency = currency!, Basis = PriceBasis.Asking, Note = note,
                EnteredByUserId = userId, EnteredAt = Now, ClientIp = ip?.Length > 45 ? ip[..45] : ip
            });
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ---------------------------------------------------------------- samples, batches, stories

        public record SampleRow(Sample Sample, LabReport? ApprovedReport, List<(CriticalControlPointParameter Parameter, LabResult Result)> Values);

        public async Task<List<SampleRow>> SamplesAsync(string beekeeperId)
        {
            var ids = await Lots(beekeeperId).Select(r => r.RawHoneyProductionId).ToListAsync();
            var samples = await _context.Samples.AsNoTracking().Include(s => s.Container).Include(s => s.LabOrg)
                .Include(s => s.Report!).ThenInclude(r => r.Results)
                .Where(s => ids.Contains(s.RawHoneyProductionId) && s.FinalHoneyProductionId == null).OrderByDescending(s => s.SampleId).ToListAsync();
            var parameterIds = samples.Where(s => s.Report?.Status == LabReportStatus.Approved).SelectMany(s => s.Report!.Results.Select(r => r.ParameterId)).Distinct().ToList();
            var parameters = await _context.CriticalControlPointParameters.AsNoTracking().Where(p => parameterIds.Contains(p.ParameterId)).ToDictionaryAsync(p => p.ParameterId);
            // C4: a report is shown only once approved
            return samples.Select(s =>
            {
                var approved = s.Report?.Status == LabReportStatus.Approved ? s.Report : null;
                var values = approved?.Results.Where(r => parameters.ContainsKey(r.ParameterId)).OrderBy(r => r.ParameterId)
                    .Select(r => (parameters[r.ParameterId], r)).ToList() ?? new();
                return new SampleRow(s, approved, values);
            }).ToList();
        }

        public record BatchRow(FinalHoneyProduction Batch, decimal MyKg, decimal TotalKg)
        {
            public decimal Percent => TotalKg <= 0 ? 0 : Math.Round(MyKg * 100 / TotalKg, 1);
        }

        public async Task<List<BatchRow>> BatchesAsync(string beekeeperId)
        {
            var ids = await Lots(beekeeperId).Select(r => r.RawHoneyProductionId).ToListAsync();
            var mine = await _context.FinalHoneyProductionDetails.AsNoTracking().Where(d => ids.Contains(d.RawHoneyProductionId))
                .GroupBy(d => d.FinalHoneyProductionId).Select(g => new { BatchId = g.Key, Kg = g.Sum(d => d.UsedAmount) }).ToListAsync();
            var batchIds = mine.Select(m => m.BatchId).ToList();
            var totals = await _context.FinalHoneyProductionDetails.AsNoTracking().Where(d => batchIds.Contains(d.FinalHoneyProductionId))
                .GroupBy(d => d.FinalHoneyProductionId).Select(g => new { BatchId = g.Key, Kg = g.Sum(d => d.UsedAmount) }).ToDictionaryAsync(x => x.BatchId, x => x.Kg);
            var batches = await _context.FinalHoneyProductions.AsNoTracking().Where(b => batchIds.Contains(b.FinalHoneyProductionId)).ToDictionaryAsync(b => b.FinalHoneyProductionId);
            return mine.Select(m => new BatchRow(batches[m.BatchId], m.Kg, totals.GetValueOrDefault(m.BatchId)))
                .OrderByDescending(b => b.Batch.FinalHoneyProductionDate).ToList();
        }

        // ---------------------------------------------------------------- privacy

        public record ConsentRow(string Purpose, Consent? Active, bool MayGive, bool MayWithdraw);
        public record Controller(string Name, string? Address, string? Contact, string Why);

        public static readonly string[] PortalPurposes = { ConsentPurpose.PublicName, ConsentPurpose.PortalNotifications, ConsentPurpose.AssociationPrice, ConsentPurpose.Storefront };

        public async Task<List<ConsentRow>> ConsentsAsync(string beekeeperId)
        {
            var active = await _context.Consents.AsNoTracking().Where(c => c.BeekeeperId == beekeeperId && c.WithdrawnAt == null).ToListAsync();
            var associated = await _context.BeekeeperAssociations.AnyAsync(a => a.BeekeeperId == beekeeperId && a.ValidTo == null);
            return PortalPurposes
                .Where(p => p != ConsentPurpose.AssociationPrice || associated)
                .Select(p =>
                {
                    var c = active.FirstOrDefault(x => x.Purpose == p);
                    // the storefront consent is given through the association (D-30); every consent can be withdrawn here
                    return new ConsentRow(p, c, c == null && p != ConsentPurpose.Storefront, c != null);
                })
                .Where(r => r.Purpose != ConsentPurpose.Storefront || r.Active != null)
                .ToList();
        }

        public async Task<string?> GiveConsentAsync(string beekeeperId, string userId, string purpose, Func<Task<Notice?>> text)
        {
            if (!PortalPurposes.Contains(purpose) || purpose == ConsentPurpose.Storefront)
                return "This consent cannot be given here.";
            if (purpose == ConsentPurpose.AssociationPrice && !await _context.BeekeeperAssociations.AnyAsync(a => a.BeekeeperId == beekeeperId && a.ValidTo == null))
                return "This consent cannot be given here.";
            if (await _context.Consents.AnyAsync(c => c.BeekeeperId == beekeeperId && c.Purpose == purpose && c.WithdrawnAt == null))
                return null;
            var notice = await text();
            var association = purpose == ConsentPurpose.AssociationPrice
                ? await _context.BeekeeperAssociations.Where(a => a.BeekeeperId == beekeeperId && a.ValidTo == null).Select(a => (int?)a.AssociationOrgId).FirstOrDefaultAsync()
                : null;
            _context.Consents.Add(new Consent
            {
                BeekeeperId = beekeeperId, AssociationOrgId = association, Purpose = purpose, NameConsented = purpose == ConsentPurpose.PublicName,
                TextVersion = notice?.Version ?? "none", TextSha256 = notice?.BodySha256 ?? new string('0', 64), NoticeId = notice?.NoticeId,
                GivenOn = DateOnly.FromDateTime(Now.ToLocalTime()), Method = "Portal", RecordedByUserId = userId, RecordedAt = Now
            });
            await _context.SaveChangesAsync();
            return null;
        }

        // effective at once (D-30): the public pages read the active consents on every request
        public async Task<bool> WithdrawConsentAsync(string beekeeperId, string userId, string purpose)
        {
            var active = await _context.Consents.Where(c => c.BeekeeperId == beekeeperId && c.Purpose == purpose && c.WithdrawnAt == null).ToListAsync();
            foreach (var c in active)
            {
                c.WithdrawnAt = Now;
                c.WithdrawnByUserId = userId;
                c.WithdrawReason = "Withdrawn by the beekeeper in the portal";
            }
            await _context.SaveChangesAsync();
            return active.Count > 0;
        }

        // the organizations that hold the beekeeper's data, with their data controller details (F8)
        public async Task<List<Controller>> ControllersAsync(string beekeeperId)
        {
            var lotOrgs = await Lots(beekeeperId).Where(r => r.OwnerOrgId != null).Select(r => r.OwnerOrgId!.Value)
                .Concat(_context.LotIntakes.Where(i => i.RawHoneyProduction!.RawHoneyProductionApiary!.ApiaryBeekeeperId == beekeeperId).Select(i => i.ReceivingOrgId))
                .Distinct().ToListAsync();
            var registering = await _context.BeekeeperRegistrations.Where(r => r.BeekeeperId == beekeeperId).Select(r => r.RegisteringOrgId).Distinct().ToListAsync();
            var association = await _context.BeekeeperAssociations.Where(a => a.BeekeeperId == beekeeperId && a.ValidTo == null).Select(a => a.AssociationOrgId).ToListAsync();
            var ids = lotOrgs.Concat(registering).Concat(association).Distinct().ToList();
            var orgs = await _context.Organizations.AsNoTracking().Where(o => ids.Contains(o.OrgId) || o.IsPlatform).ToListAsync();
            return orgs.OrderBy(o => o.IsPlatform ? 0 : 1).ThenBy(o => o.DisplayName).Select(o => new Controller(o.DataControllerName ?? o.DisplayName,
                o.DataControllerAddress, o.DataControllerContact,
                o.IsPlatform ? "Me.Why.Platform" : registering.Contains(o.OrgId) ? "Me.Why.Registration" : association.Contains(o.OrgId) ? "Me.Why.Association" : "Me.Why.Deliveries"))
                .ToList();
        }

        // D-95: a notice version accepted at registration and again by the account is listed once (its latest acceptance)
        public async Task<List<NoticeAcceptance>> AcceptedNoticesAsync(string beekeeperId, string userId) =>
            (await _context.NoticeAcceptances.AsNoTracking().Include(a => a.Notice)
                .Where(a => a.BeekeeperId == beekeeperId || a.UserId == userId).OrderByDescending(a => a.At).ToListAsync())
                .GroupBy(a => a.NoticeId).Select(g => g.First()).ToList();

        public static string Money(decimal value, string currency) => $"{value.ToString("0.00", CultureInfo.CurrentCulture)} {currency}";
    }
}
