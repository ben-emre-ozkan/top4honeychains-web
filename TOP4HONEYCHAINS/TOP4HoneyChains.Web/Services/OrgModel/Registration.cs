using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P1 (D78-SPEC 4.1, 4.2): what the registration officer enters. The same form serves a new beekeeper (DraftKey) and
    // the re-verification of an existing one (BeekeeperId). The SMS code confirms exactly these values (content hash).
    public class RegistrationForm
    {
        public string? DraftKey { get; set; }
        public string? BeekeeperId { get; set; }
        public string? RegistryNumber { get; set; }
        public string? ProvinceMismatchReason { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Phone { get; set; }
        // O-17: no mobile phone: signed photo, no account
        public bool NoPhone { get; set; }
        public int? EducationLevelId { get; set; }
        public int? PurposeTypeId { get; set; }
        public int? BeekeepingTypeId { get; set; }
        public string? Language { get; set; }

        // the first apiary (required for a new beekeeper)
        public decimal? ApiaryLatitude { get; set; }
        public decimal? ApiaryLongitude { get; set; }
        public string? ApiaryProvince { get; set; }
        public int? ApiaryDistrictId { get; set; }
        public string? ApiaryLocality { get; set; }
        public short? ApiaryHives { get; set; }

        // optional consents (never pre-ticked) and the officer's attestation
        public bool PublicName { get; set; }
        public bool PortalNotifications { get; set; }
        public bool Attest { get; set; }

        public string? Code { get; set; }
        public string? FallbackReason { get; set; }
        // O-15: only offered while Features:BeekeeperAccounts is on
        public bool IssueSignIn { get; set; }
        // D-88: the beekeeper's association (optional), linked at registration instead of by the platform administrator
        public int? AssociationOrgId { get; set; }

        public bool IsNew => string.IsNullOrEmpty(BeekeeperId);
        public bool HasApiary => ApiaryLatitude != null || ApiaryLongitude != null || !string.IsNullOrWhiteSpace(ApiaryProvince)
                                 || ApiaryDistrictId != null || !string.IsNullOrWhiteSpace(ApiaryLocality) || ApiaryHives != null;
    }

    // The organization a registration is recorded for: the officer's directorate, or the platform for its administrators
    public record Registrar(int OrgId, string OrgName, string? OrgProvince, bool IsPlatformAdmin);

    public class RegistrationService
    {
        public const string EvidenceFolder = "App_Data/registration-evidence";

        public record Checked(string? RegistryNumber, string? PhoneE164, bool? ProvinceMatches, string? ApiaryProvinceName, string? ApiaryDistrictName);
        public record Saved(int RegistrationId, string BeekeeperId);

        private readonly TOP4HoneyChainsContext _context;
        private readonly IMessageService _messages;
        private readonly Places _places;
        private readonly NoticeService _notices;
        private readonly TimeProvider _clock;
        private readonly ICurrentOrg _current;
        private readonly Permissions _permissions;
        private readonly SmsOptions _sms;
        private readonly bool _demo;

        public RegistrationService(TOP4HoneyChainsContext context, IMessageService messages, Places places, NoticeService notices,
            TimeProvider clock, ICurrentOrg current, Permissions permissions, IOptions<SmsOptions> sms, DemoMode demo)
        {
            _demo = demo.Enabled;
            _sms = sms.Value;
            _context = context;
            _messages = messages;
            _places = places;
            _notices = notices;
            _clock = clock;
            _current = current;
            _permissions = permissions;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        // ---------------------------------------------------------------- who may register and review

        // A verified organization with the Beekeeper registration capability and a role with Registration/Enter (or Read);
        // platform administrators register for the platform organization
        public async Task<Registrar?> RegistrarAsync(string action = PermissionAreas.Enter)
        {
            if (_current.IsPlatformAdmin)
            {
                var platform = await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.IsPlatform);
                return platform == null ? null : new Registrar(platform.OrgId, platform.DisplayName, null, true);
            }
            var active = await _current.GetAsync();
            if (active == null || !active.IsVerified || !active.Has(Capabilities.BeekeeperRegistration)
                || !_permissions.Allows(PermissionAreas.Registration, action, active.OrgRole))
                return null;
            var province = await _context.Organizations.AsNoTracking().Where(o => o.OrgId == active.OrgId).Select(o => o.Province).FirstOrDefaultAsync();
            return new Registrar(active.OrgId, active.OrgName, province, false);
        }

        // who may accept or reject signed-photo evidence: the registering organization's Compliance Officer (Evidence/Approve),
        // or a platform administrator for the platform's registrations; never the person who recorded it (four-eyes)
        public async Task<bool> MayReviewAsync(BeekeeperRegistration r)
        {
            if (r.OfficerUserId == _current.UserId)
                return false;
            if (_current.IsPlatformAdmin)
                return await _context.Organizations.AnyAsync(o => o.OrgId == r.RegisteringOrgId && o.IsPlatform);
            var active = await _current.GetAsync();
            return active != null && active.IsVerified && active.OrgId == r.RegisteringOrgId
                   && _permissions.Allows(PermissionAreas.Evidence, PermissionAreas.Approve, active.OrgRole);
        }

        // reading a registration: its organization's registrars and reviewers, and platform administrators
        public async Task<bool> MayReadAsync(BeekeeperRegistration r)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            return active != null && active.OrgId == r.RegisteringOrgId
                   && (_permissions.Allows(PermissionAreas.Registration, PermissionAreas.Read, active.OrgRole)
                       || _permissions.Allows(PermissionAreas.Registration, PermissionAreas.Enter, active.OrgRole)
                       || _permissions.Allows(PermissionAreas.Evidence, PermissionAreas.Approve, active.OrgRole));
        }

        // ---------------------------------------------------------------- checks

        public async Task<(Checked? Data, List<string> Errors)> CheckAsync(RegistrationForm f, Registrar registrar, bool photo)
        {
            var errors = new List<string>();
            var number = RegistryNumber.Normalize(f.RegistryNumber);
            if (!RegistryNumber.IsWellFormed(number, _demo))
                errors.Add(RegistryNumber.Message(_demo));
            else if (await _context.BeekeeperRegistry.AnyAsync(r => r.RegistryNumber == number && r.BeekeeperId != (f.BeekeeperId ?? "")))
                errors.Add("A beekeeper with this registry number is already registered. Find it and re-verify instead.");

            bool? provinceMatches = null;
            if (RegistryNumber.IsWellFormed(number) && _places.Province(registrar.OrgProvince) is { } orgProvince)
            {
                provinceMatches = number.Substring(2, 2) == orgProvince.Code;
                if (provinceMatches == false && string.IsNullOrWhiteSpace(f.ProvinceMismatchReason))
                    errors.Add("The registry number belongs to another province than the directorate. Give a reason to continue.");
            }

            if (string.IsNullOrWhiteSpace(f.FirstName) || string.IsNullOrWhiteSpace(f.LastName) || f.FirstName.Trim().Length > 50 || f.LastName.Trim().Length > 50)
                errors.Add("Enter the first and last name (at most 50 characters each).");
            var today = DateOnly.FromDateTime(Now);
            if (f.BirthDate == null || f.BirthDate > today.AddYears(-15) || f.BirthDate < today.AddYears(-110))
                errors.Add("Enter a valid birth date.");

            string? e164 = null;
            if (f.NoPhone)
            {
                if (!photo)
                    errors.Add("Without a mobile phone the registration is confirmed by a signed photo.");
            }
            else
            {
                var (phone, phoneError) = _messages.CheckPhone(f.Phone);
                if (phone == null)
                    errors.Add(phoneError!);
                e164 = phone;
            }

            string? apiaryProvince = null, apiaryDistrict = null;
            if (f.IsNew || f.HasApiary)
            {
                if (f.ApiaryLatitude is not (>= -90 and <= 90) || f.ApiaryLongitude is not (>= -180 and <= 180) || (f.ApiaryLatitude == 0 && f.ApiaryLongitude == 0))
                    errors.Add("Set the apiary location on the map or type its coordinates.");
                if (f.ApiaryHives is not (>= 1 and <= 10000))
                    errors.Add("The beehive count must be between 1 and 10000.");
                string? placeError;
                (apiaryProvince, apiaryDistrict, placeError) = await ApiaryTitles.ResolveAsync(_context, f.ApiaryProvince, f.ApiaryDistrictId, f.ApiaryLocality);
                if (placeError != null)
                    errors.Add(placeError);
            }

            if (f.AssociationOrgId != null && !await _context.Organizations.AnyAsync(o => o.OrgId == f.AssociationOrgId && !o.IsPlatform
                    && o.Status == OrgStatus.Verified && o.Capabilities.Any(c => c.Capability == Capabilities.Association)))
                errors.Add("Choose an organization with the Association capability.");
            if (!f.Attest)
                errors.Add("Confirm the attestation: you have seen the identity document and the registry record.");
            if (!SmsTemplates.Languages.Contains(f.Language ?? ""))
                f.Language = SmsTemplates.DefaultLanguage;
            return (errors.Count == 0 ? new Checked(number, e164, provinceMatches, apiaryProvince, apiaryDistrict) : null, errors);
        }

        // exactly what the SMS code confirms (D78-SPEC 4.1 step 6): changing any of it voids the code
        public static string ContentHash(RegistrationForm f, Checked c) =>
            CodeRules.ContentHash(string.Join("|", new[]
            {
                "registration", f.BeekeeperId ?? f.DraftKey ?? "", c.RegistryNumber ?? "", f.FirstName?.Trim() ?? "", f.LastName?.Trim() ?? "",
                f.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "", c.PhoneE164 ?? "",
                f.EducationLevelId?.ToString(CultureInfo.InvariantCulture) ?? "", f.PurposeTypeId?.ToString(CultureInfo.InvariantCulture) ?? "",
                f.BeekeepingTypeId?.ToString(CultureInfo.InvariantCulture) ?? "", f.Language ?? "",
                f.ApiaryLatitude?.ToString("0.#########", CultureInfo.InvariantCulture) ?? "", f.ApiaryLongitude?.ToString("0.#########", CultureInfo.InvariantCulture) ?? "",
                f.ApiaryProvince ?? "", f.ApiaryDistrictId?.ToString(CultureInfo.InvariantCulture) ?? "", f.ApiaryLocality?.Trim() ?? "",
                f.ApiaryHives?.ToString(CultureInfo.InvariantCulture) ?? "", f.PublicName ? "name:yes" : "name:no", f.PortalNotifications ? "sms:yes" : "sms:no"
            }.Concat(f.AssociationOrgId is int association ? new[] { "association:" + association.ToString(CultureInfo.InvariantCulture) } : Array.Empty<string>())));

        public static string SubjectKey(RegistrationForm f) => f.BeekeeperId ?? f.DraftKey!;

        // ---------------------------------------------------------------- saving

        // Saves the beekeeper, the first apiary, the registration (attestation), the notice acceptance and the consents in one
        // transaction. SMS: evidence Accepted and the registry number Verified. Photo: PendingReview until the Compliance Officer decides.
        public async Task<Saved> SaveAsync(RegistrationForm f, Checked c, Registrar registrar, string officerUserId, long? codeId,
            (string Stored, string Original, string Sha256)? evidence)
        {
            var now = Now;
            var method = codeId != null ? AcceptanceMethod.SmsCode : AcceptanceMethod.SignedPhoto;
            var accepted = codeId != null;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            Beekeeper beekeeper;
            string kind;
            if (f.IsNew)
            {
                beekeeper = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperBusinessNumber = await BusinessNumbers.NextAsync(_context, now.Year, RegistryNumber.CountryOf(c.RegistryNumber!), _demo), IsActive = true };
                _context.Beekeepers.Add(beekeeper);
                kind = RegistrationKind.New;
            }
            else
            {
                beekeeper = await _context.Beekeepers.FirstAsync(b => b.BeekeeperId == f.BeekeeperId);
                kind = await _context.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == f.BeekeeperId) ? RegistrationKind.IdentityChange : RegistrationKind.Reverification;
            }
            beekeeper.BeekeeperFirstName = f.FirstName!.Trim();
            beekeeper.BeekeeperLastName = f.LastName!.Trim();
            beekeeper.BeekeeperBirthDate = f.BirthDate!.Value;
            beekeeper.BeekeeperPhoneNumber = c.PhoneE164 ?? beekeeper.BeekeeperPhoneNumber;
            beekeeper.BeekeeperEducationLevelId = f.EducationLevelId;
            beekeeper.BeekeeperBeekeepingPurposeTypeId = f.PurposeTypeId;
            beekeeper.BeekeeperBeekeepingTypeId = f.BeekeepingTypeId;
            beekeeper.PreferredLanguage = f.Language;
            await _context.SaveChangesAsync();

            var registry = await _context.BeekeeperRegistry.FirstOrDefaultAsync(r => r.BeekeeperId == beekeeper.BeekeeperId);
            if (registry == null)
                _context.BeekeeperRegistry.Add(registry = new BeekeeperRegistry { BeekeeperId = beekeeper.BeekeeperId! });
            registry.RegistryNumber = c.RegistryNumber;
            registry.Province = _places.ByPlate(c.RegistryNumber![2..4])?.Name;
            registry.Status = accepted ? RegistryStatus.Verified : RegistryStatus.Pending;
            registry.VerifiedAt = accepted ? now : null;
            registry.VerifiedByUserId = accepted ? officerUserId : null;

            if (c.ApiaryProvinceName != null)
                _context.Apiaries.Add(await NewApiaryAsync(beekeeper.BeekeeperId!, f.ApiaryLatitude!.Value, f.ApiaryLongitude!.Value, f.ApiaryProvince!,
                    f.ApiaryDistrictId!.Value, f.ApiaryLocality!, f.ApiaryHives!.Value, c.ApiaryProvinceName, c.ApiaryDistrictName!));

            var attestation = await _notices.CurrentAsync(NoticeKind.Attestation, NoticeAudience.Member, null, null);
            var registration = new BeekeeperRegistration
            {
                BeekeeperId = beekeeper.BeekeeperId!, RegisteringOrgId = registrar.OrgId, OfficerUserId = officerUserId, At = now, Kind = kind,
                RegistryNumber = c.RegistryNumber, RegistryFormatOk = true, RegistryProvinceMatches = c.ProvinceMatches,
                ProvinceMismatchReason = c.ProvinceMatches == false ? Trim(f.ProvinceMismatchReason, 300) : null,
                PhoneE164Hash = c.PhoneE164 == null ? null : CodeRules.PhoneHash(_sms.KeyBytes(), c.PhoneE164),
                Method = method, CodeId = codeId, FallbackReason = accepted ? null : Trim(f.NoPhone ? "No mobile phone" : f.FallbackReason, 300),
                EvidenceStoredFileName = evidence?.Stored, EvidenceOriginalFileName = evidence?.Original is string o ? Trim(o, 260) : null, EvidenceSha256 = evidence?.Sha256,
                OfficerAttestation = true, AttestationNoticeId = attestation?.NoticeId,
                EvidenceStatus = accepted ? EvidenceStatus.Accepted : EvidenceStatus.PendingReview
            };
            _context.BeekeeperRegistrations.Add(registration);

            // the information notice of the platform for beekeepers and, if it published one, of the registering organization
            foreach (var orgId in new int?[] { null, registrar.IsPlatformAdmin ? null : registrar.OrgId }.Distinct())
                if (await _notices.CurrentAsync(NoticeKind.PrivacyNotice, NoticeAudience.Beekeeper, orgId, null, f.Language) is Notice notice)
                    _context.NoticeAcceptances.Add(new NoticeAcceptance
                    {
                        NoticeId = notice.NoticeId, BeekeeperId = beekeeper.BeekeeperId, OrgId = registrar.OrgId, Method = method, CodeId = codeId,
                        EvidenceStoredFileName = evidence?.Stored, EvidenceSha256 = evidence?.Sha256, RecordedByUserId = officerUserId, At = now
                    });

            // one consent per "yes"; an earlier consent of the same purpose is replaced (the unique index allows one active)
            foreach (var (purpose, given) in new[] { (ConsentPurpose.PublicName, f.PublicName), (ConsentPurpose.PortalNotifications, f.PortalNotifications) })
            {
                var active = await _context.Consents.Where(x => x.BeekeeperId == beekeeper.BeekeeperId && x.Purpose == purpose && x.WithdrawnAt == null).ToListAsync();
                foreach (var a in active)
                {
                    a.WithdrawnAt = now;
                    a.WithdrawnByUserId = officerUserId;
                    a.WithdrawReason = given ? "Replaced at registration" : "Not given at registration";
                }
                if (!given)
                    continue;
                var text = await _notices.CurrentAsync(NoticeKind.ConsentText, NoticeAudience.Beekeeper, null, purpose, f.Language);
                _context.Consents.Add(new Consent
                {
                    BeekeeperId = beekeeper.BeekeeperId!, AssociationOrgId = null, RegisteringOrgId = registrar.OrgId, Purpose = purpose,
                    NameConsented = purpose == ConsentPurpose.PublicName, TextVersion = text?.Version ?? "none", TextSha256 = text?.BodySha256 ?? new string('0', 64),
                    NoticeId = text?.NoticeId, GivenOn = DateOnly.FromDateTime(now.ToLocalTime()), Method = method, CodeId = codeId,
                    DocumentStoredFileName = evidence?.Stored, DocumentOriginalFileName = evidence?.Original is string d ? Trim(d, 260) : null, DocumentSha256 = evidence?.Sha256,
                    RecordedByUserId = officerUserId, RecordedAt = now
                });
            }
            // D-88: the association chosen at registration; an earlier link ends (history kept)
            if (f.AssociationOrgId is int associationId)
            {
                var today = DateOnly.FromDateTime(now);
                var link = await _context.BeekeeperAssociations.FirstOrDefaultAsync(a => a.BeekeeperId == beekeeper.BeekeeperId && a.ValidTo == null);
                if (link?.AssociationOrgId != associationId)
                {
                    if (link != null)
                        link.ValidTo = today;
                    _context.BeekeeperAssociations.Add(new BeekeeperAssociation
                    {
                        BeekeeperId = beekeeper.BeekeeperId!, AssociationOrgId = associationId, ValidFrom = today, CreatedByUserId = officerUserId
                    });
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new Saved(registration.RegistrationId, beekeeper.BeekeeperId!);
        }

        public async Task<Apiary> NewApiaryAsync(string beekeeperId, decimal lat, decimal lng, string provinceCode, int districtId, string locality, short hives,
            string provinceName, string districtName) => new()
        {
            ApiaryBeekeeperId = beekeeperId, ApiaryQRCode = Guid.NewGuid(), ApiaryBeehivesCount = hives,
            ApiaryLocationLatitude = lat, ApiaryLocationLongitude = lng, ApiaryIsActive = true,
            Province = provinceCode, DistrictId = districtId, Locality = locality.Trim(), TitleSource = ApiaryTitles.Generated,
            ApiaryTitle = await ApiaryTitles.UniqueAsync(_context, provinceName, districtName, locality)
        };

        // ---------------------------------------------------------------- review of signed-photo evidence (four-eyes)

        public async Task ReviewAsync(BeekeeperRegistration r, bool accept, string? reason, string reviewerUserId)
        {
            var now = Now;
            r.EvidenceStatus = accept ? EvidenceStatus.Accepted : EvidenceStatus.Rejected;
            r.ReviewedByUserId = reviewerUserId;
            r.ReviewedAt = now;
            r.ReviewReason = Trim(reason, 500);
            var registry = await _context.BeekeeperRegistry.FirstOrDefaultAsync(x => x.BeekeeperId == r.BeekeeperId);
            if (registry != null && registry.RegistryNumber == r.RegistryNumber)
            {
                registry.Status = accept ? RegistryStatus.Verified : RegistryStatus.Rejected;
                registry.VerifiedAt = accept ? now : null;
                registry.VerifiedByUserId = accept ? reviewerUserId : null;
            }
            if (!accept)
                // the consents rest on the rejected evidence
                foreach (var consent in await _context.Consents.Where(x => x.BeekeeperId == r.BeekeeperId && x.WithdrawnAt == null
                                                                           && x.DocumentSha256 == r.EvidenceSha256 && x.Method == AcceptanceMethod.SignedPhoto).ToListAsync())
                {
                    consent.WithdrawnAt = now;
                    consent.WithdrawnByUserId = reviewerUserId;
                    consent.WithdrawReason = "Evidence rejected";
                }
            await _context.SaveChangesAsync();
        }

        private static string? Trim(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : s.Trim().Length > max ? s.Trim()[..max] : s.Trim();
    }

    // Business numbers TR-BEE-{year}-{nnnnnn}, the next free one. D-86: on a demo site {country}-BEE-DEMO-{nnnnnn}, so a demo
    // number never equals a real one (country TR, or AR, PL, LV for the demo registry numbers of the partner countries).
    public static class BusinessNumbers
    {
        public static async Task<string> NextAsync(TOP4HoneyChainsContext db, int year, string country = "TR", bool demo = false)
        {
            var prefix = demo ? $"{country}-BEE-DEMO-" : $"{country}-BEE-{year}-";
            var series = demo ? "BEE-DEMO-" : $"BEE-{year}-";
            var count = await db.Beekeepers.CountAsync(b => b.BeekeeperBusinessNumber.Contains(series));
            for (var n = count + 1; ; n++)
            {
                var number = prefix + n.ToString("D6", CultureInfo.InvariantCulture);
                if (!await db.Beekeepers.AnyAsync(b => b.BeekeeperBusinessNumber == number))
                    return number;
            }
        }
    }

    // D-78 (O-15, D78-SPEC 4.1 step 9-10): the beekeeper's sign-in, only while Features:BeekeeperAccounts is on and the phone was
    // confirmed by the SMS code. A legacy login (Beekeepers.BeekeeperId = AspNetUsers.Id) is reused, so its history stays.
    public class BeekeeperSignIn
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly CredentialService _credentials;
        private readonly FeatureFlags _flags;

        public BeekeeperSignIn(TOP4HoneyChainsContext context, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles,
            CredentialService credentials, IOptionsSnapshot<FeatureFlags> flags)
        {
            _context = context;
            _users = users;
            _roles = roles;
            _credentials = credentials;
            _flags = flags.Value;
        }

        public bool Enabled => _flags.BeekeeperAccounts;

        public async Task<CredentialService.IssueResult> IssueAsync(Beekeeper beekeeper, string officerUserId, string link)
        {
            if (!Enabled)
                return new CredentialService.IssueResult(false, "Beekeeper accounts are not enabled yet.");
            if (string.IsNullOrEmpty(beekeeper.BeekeeperPhoneNumber))
                return new CredentialService.IssueResult(false, "A mobile phone number is required.");
            var account = await _context.BeekeeperAccounts.FirstOrDefaultAsync(a => a.BeekeeperId == beekeeper.BeekeeperId);
            var user = account == null ? await _users.FindByIdAsync(beekeeper.BeekeeperId!) : await _users.FindByIdAsync(account.UserId);
            if (user == null)
            {
                var name = beekeeper.BeekeeperBusinessNumber;
                for (var n = 2; await _users.FindByNameAsync(name) != null; n++)
                    name = beekeeper.BeekeeperBusinessNumber + "-" + n;
                user = new ApplicationUser { UserName = name };
                var created = await _users.CreateAsync(user, CredentialService.Unusable());
                if (!created.Succeeded)
                    return new CredentialService.IssueResult(false, string.Join(" ", created.Errors.Select(e => e.Description)));
            }
            if (await _roles.RoleExistsAsync("Beekeeper") && !await _users.IsInRoleAsync(user, "Beekeeper"))
                await _users.AddToRoleAsync(user, "Beekeeper");
            if (account == null)
            {
                _context.BeekeeperAccounts.Add(new BeekeeperAccount { BeekeeperId = beekeeper.BeekeeperId!, UserId = user.Id, LinkedAt = DateTime.UtcNow, LinkedByUserId = officerUserId });
                await _context.SaveChangesAsync();
            }
            return await _credentials.IssueBySmsAsync(user, beekeeper.BeekeeperPhoneNumber, beekeeper.PreferredLanguage, officerUserId, link);
        }
    }
}
