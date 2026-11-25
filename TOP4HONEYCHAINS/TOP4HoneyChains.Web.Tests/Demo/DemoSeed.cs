using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Tests.E2E;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: records the demo data of the four partner countries. Organizations, accounts and memberships are set up directly (what a
    // platform administrator prepares); every business step goes through the application's pages as the role that does it in real
    // life: registrations (SMS code and signed form), sign-in details for the beekeeper portal, the delivery with sealed containers
    // and the beekeeper's SMS code, a rejected jar and its replacement, the lab's composite report, the CCP records, the batch,
    // finalizing, the shipment to the shop, the beekeeper's asking price and the association's consent for the beekeeper page.
    public sealed class DemoSeed
    {
        public record Credential(string Country, string UserName, string Password, string AccountKey, string Role, string OrgName);

        public record Result(List<Credential> Credentials, DemoManifest Manifest);

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private readonly DemoApp _app;
        private readonly List<Credential> _credentials = new();
        private readonly Dictionary<string, string> _passwords = new();
        private readonly DemoManifest _manifest = new();
        private readonly DateTime _today = DateTime.Today;

        public DemoSeed(DemoApp app) => _app = app;

        private Task<T> Db<T>(Func<TOP4HoneyChainsContext, Task<T>> work) => _app.WithContextAsync(work);

        public async Task<Result> RunAsync()
        {
            foreach (var c in DemoCountries.All)
                await SetUpAsync(c);
            foreach (var c in DemoCountries.All)
                await FlowsAsync(c);
            var admin = await LoginAsync(DemoCountries.All[0].Prefix + ".admin");
            var verified = await admin.PostFlashAsync("/Audit/Verify", new(), "/Audit/Index");
            if (!verified.Contains("verif", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("audit chain check: " + verified);
            _manifest.Parameters = await Db(db => db.CriticalControlPointParameters.AsNoTracking()
                .Join(db.CriticalControlPoints, p => p.CCPId, c => c.Id, (p, c) => new { p, c.Code })
                .OrderBy(x => x.p.CCPId).ThenBy(x => x.p.ParameterId)
                .Select(x => new DemoManifest.Parameter(x.p.ParameterId, x.Code, x.p.ParameterName, x.p.DataType, x.p.ParameterUnit, x.p.MinValue, x.p.MaxValue, x.p.RequiredIfParameterId))
                .ToListAsync());
            return new Result(_credentials, _manifest);
        }

        // ---------------------------------------------------------------- set-up: organizations, accounts, memberships

        public static string NewPassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            return string.Join("-", Enumerable.Range(0, 4).Select(_ =>
                new string(Enumerable.Range(0, 4).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray())));
        }

        private async Task<string> UserAsync(DemoCountry c, string userName, string? phone, string? aspRole)
        {
            using var scope = _app.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var password = NewPassword();
            var user = new ApplicationUser
            {
                UserName = userName, Email = userName + "@example.invalid", EmailConfirmed = true,
                PhoneNumber = phone, PhoneNumberConfirmed = phone != null, LockoutEnabled = true
            };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new InvalidOperationException($"{userName}: " + string.Join("; ", created.Errors.Select(e => e.Description)));
            if (aspRole != null)
                await users.AddToRoleAsync(user, aspRole);
            var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
            db.UserSettings.Add(new UserSetting { UserId = user.Id, Language = c.Language, PhoneVerifiedAt = phone != null ? DateTime.UtcNow : null });
            await db.SaveChangesAsync();
            _passwords[userName] = password;
            return user.Id;
        }

        private async Task SetUpAsync(DemoCountry c)
        {
            var m = _manifest.Country(c.Code);
            m.Language = c.Language;
            m.Currency = c.Currency;
            m.Regions = c.Regions.Select(r => r.Name).ToList();
            m.Province = c.Regions.Length > 0 ? c.Region.Name : DemoCountries.TurkishProvince;
            var province = m.Province;
            var orgIds = new Dictionary<string, int>();
            foreach (var o in c.Orgs)
            {
                var id = await Db(async db =>
                {
                    int? wholesaler = null;
                    if (o.Key == DemoCountries.Plant)
                    {
                        var w = new Wholesaler
                        {
                            WholesalerTitle = o.Name, WholesalerTaxNo = $"DEMO-{c.Code}-TAX-001", WholesalerContactName = "[DEMO] Quality office",
                            WholesalerPhone = c.StaffPhonePrefix + "99", WholesalerEmail = $"{c.Prefix}.plant@example.invalid",
                            WholesalerAddress = $"[DEMO] Demo street 1, {o.City}", WholesalerCity = o.City, WholesalerCountry = c.CountryName,
                            WholesalerIsActive = true, CreatedAt = DateTime.Now
                        };
                        db.Wholesalers.Add(w);
                        await db.SaveChangesAsync();
                        wholesaler = w.WholesalerId;
                    }
                    var org = new Organization
                    {
                        DisplayName = o.Name, LegalName = o.Name, Province = province, District = o.City, Status = OrgStatus.Verified,
                        VerifiedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, LegacyWholesalerId = wholesaler,
                        DataControllerName = o.Name, DataControllerAddress = $"[DEMO] Demo street 1, {o.City}",
                        DataControllerContact = $"privacy.{c.Prefix}.{o.Key}@example.invalid"
                    };
                    db.Organizations.Add(org);
                    await db.SaveChangesAsync();
                    foreach (var cap in o.Capabilities)
                        db.OrgCapabilities.Add(new OrgCapability
                        {
                            OrgId = org.OrgId, Capability = cap, IsAccredited = cap == Capabilities.Laboratory && o.Accreditation != null,
                            AccreditationNumber = cap == Capabilities.Laboratory ? o.Accreditation : null,
                            AccreditationValidUntil = cap == Capabilities.Laboratory && o.Accreditation != null ? DateOnly.FromDateTime(_today.AddYears(2)) : null,
                            AddedAt = DateTime.UtcNow
                        });
                    await db.SaveChangesAsync();
                    return org.OrgId;
                });
                orgIds[o.Key] = id;
                m.Orgs[o.Key] = new DemoManifest.Org { Name = o.Name, Id = id };
            }

            var platform = await Db(db => db.Organizations.Where(o => o.IsPlatform).Select(o => o.OrgId).FirstAsync());
            var n = 1;
            foreach (var a in DemoCountries.Accounts)
            {
                var userName = $"{c.Prefix}.{a.Key}";
                var phone = c.StaffPhonePrefix + (n++).ToString("D2", Inv);
                if (PhoneNumberPolicy.Normalize(phone, out _) != phone)
                    throw new InvalidOperationException($"demo phone {phone} is not valid E.164");
                var userId = await UserAsync(c, userName, phone, a.Role == "PlatformAdmin" ? "Administrator" : null);
                var memberships = a.Role == "PlatformAdmin" ? new[] { (platform, OrgRoles.OrgAdmin) }
                    : a.OrgKey == "*" ? orgIds.Values.Select(id => (id, a.Role)).ToArray()
                    : new[] { (orgIds[a.OrgKey], a.Role) };
                await Db(async db =>
                {
                    foreach (var (orgId, role) in memberships)
                        db.Memberships.Add(new Membership { UserId = userId, OrgId = orgId, OrgRole = role, Status = MembershipStatus.Active, GrantedAt = DateTime.UtcNow });
                    return await db.SaveChangesAsync();
                });
                var orgName = a.Role == "PlatformAdmin" ? "Platform administration" : a.OrgKey == "*" ? "*" : c.Org(a.OrgKey).Name;
                _credentials.Add(new Credential(c.Code, userName, _passwords[userName], a.Key, a.Role, orgName));
                m.Accounts[a.Key] = new DemoManifest.Account { UserName = userName, Role = a.Role, Org = orgName, Phone = phone };
            }
        }

        private async Task<UserClient> LoginAsync(string userName)
        {
            var (client, response) = await UserClient.SignInAsync(_app, userName, _passwords[userName]);
            if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
                throw new InvalidOperationException($"sign-in of {userName} failed: {(int)response.StatusCode} {response.Headers.Location}");
            return client;
        }

        private static void Expect(string actual, string expected, string step)
        {
            if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{step}: expected \"{expected}\", got \"{actual}\"");
        }

        private static async Task<string> BodyAsync(HttpResponseMessage r) => WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());

        private Task<string> LastSmsAsync(string subjectKey) =>
            Db(db => db.SmsMessages.Where(s => s.SubjectKey == subjectKey).OrderByDescending(s => s.SmsId).Select(s => s.BodyStored).FirstAsync());

        private static string CodeIn(string sms) => Regex.Match(sms, @"\b\d{6}\b").Value;

        // ---------------------------------------------------------------- the flows of one country

        private async Task FlowsAsync(DemoCountry c)
        {
            var m = _manifest.Country(c.Code);
            int Org(string key) => m.Orgs[key].Id;
            string U(string key) => $"{c.Prefix}.{key}";

            var admin = await LoginAsync(U("admin"));
            var ro = await LoginAsync(U("registration"));
            var sao = await LoginAsync(U("reception"));
            var staff = await LoginAsync(U("staff"));
            var quality = await LoginAsync(U("quality"));
            var analyst = await LoginAsync(U("analyst"));
            var labManager = await LoginAsync(U("labmanager"));
            var shop = await LoginAsync(U("shop"));
            var shopQuality = await LoginAsync(U("shopquality"));
            var association = await LoginAsync(U("association"));

            // ---- registrations: two by SMS code, one by a signed form waiting for the Compliance Officer
            var provinceCode = c.Regions.Length > 0 ? c.Region.Code : "48";
            foreach (var b in c.Beekeepers)
            {
                var found = await ro.PostAsync("/Registration/Find", new() { ["number"] = b.RegistryNumber }, "/Registration/Index");
                if (!(found.Headers.Location?.ToString() ?? "").StartsWith("/Registration/New", StringComparison.Ordinal))
                    throw new InvalidOperationException($"{c.Code} {b.Key}: find did not start a registration: " + await ro.FlashAfterAsync(found));
                var district = await Db(db => db.Districts.Where(d => d.ProvinceCode == provinceCode && d.Name == b.District).Select(d => d.DistrictId).FirstAsync());
                var form = new Dictionary<string, string>
                {
                    ["RegistryNumber"] = b.RegistryNumber, ["FirstName"] = b.FirstName, ["LastName"] = b.LastName, ["BirthDate"] = b.BirthDate.ToString("yyyy-MM-dd", Inv),
                    ["Phone"] = b.Phone, ["Language"] = c.Language, ["ApiaryLatitude"] = b.Latitude.ToString(Inv), ["ApiaryLongitude"] = b.Longitude.ToString(Inv),
                    ["ApiaryProvince"] = provinceCode, ["ApiaryDistrictId"] = district.ToString(Inv), ["ApiaryLocality"] = b.Locality, ["ApiaryHives"] = b.Hives.ToString(Inv),
                    ["PublicName"] = b.PublicName ? "true" : "false", ["PortalNotifications"] = b.Portal ? "true" : "false", ["Attest"] = "true"
                };
                int regId;
                if (b.Photo)
                {
                    form["NoPhone"] = "true";
                    form["op"] = "photo";
                    var saved = await ro.PostMultipartAsync("/Registration/Submit", form,
                        ("evidence", "demo-signed-registration-form.pdf", DemoFiles.Pdf("[DEMO] Signed registration form",
                            $"{b.FirstName} {b.LastName} - {b.RegistryNumber}", c.Org(DemoCountries.Authority).Name,
                            "Test data for the TOP4HoneyChains partner demo. Not a real person, not a real signature."), "application/pdf"), "/Registration/New");
                    if (saved.StatusCode != HttpStatusCode.Redirect)
                        throw new InvalidOperationException($"{c.Code} {b.Key}: photo registration failed: " + Regex.Match(await BodyAsync(saved), "text-danger[^>]*>([^<]+)").Groups[1].Value);
                    regId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last(), Inv);
                }
                else
                {
                    form["op"] = "send";
                    var html = await BodyAsync(await ro.PostAsync("/Registration/Submit", form, "/Registration/New"));
                    if (!html.Contains("id=\"registration-confirm\""))
                        throw new InvalidOperationException($"{c.Code} {b.Key}: registration code not sent: " + string.Join(" | ", Regex.Matches(html, "(?:text-danger|alert-danger|validation-summary-errors)[^>]*>\\s*(?:<ul>\\s*<li>)?([^<]+)").Select(x => x.Groups[1].Value.Trim())));
                    var draft = Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
                    form["DraftKey"] = draft;
                    form["Code"] = CodeIn(await LastSmsAsync(draft));
                    form["op"] = "confirm";
                    var confirmed = await ro.PostAsync("/Registration/Submit", form, "/Registration/New");
                    Expect(await ro.FlashAfterAsync(confirmed), "registered and verified", $"{c.Code} {b.Key} registration");
                    regId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last(), Inv);
                }
                var bk = await Db(db => db.BeekeeperRegistrations.Where(r => r.RegistrationId == regId)
                    .Select(r => new { r.BeekeeperId, r.Beekeeper!.BeekeeperBusinessNumber, Apiary = r.Beekeeper.Apiaries.Select(a => a.ApiaryId).FirstOrDefault() }).FirstAsync());
                m.Beekeepers[b.Key] = new DemoManifest.Beekeeper
                {
                    Name = $"{b.FirstName} {b.LastName}", BusinessNumber = bk.BeekeeperBusinessNumber, RegistryNumber = b.RegistryNumber,
                    Phone = b.Phone, RegistrationId = regId, Photo = b.Photo, District = b.District, Locality = b.Locality,
                    Id = bk.BeekeeperId!, ApiaryId = bk.Apiary
                };
                if (!b.Photo)
                {
                    var linked = await admin.PostFlashAsync($"/Registry/SetAssociation/{bk.BeekeeperId}", new() { ["associationOrgId"] = Org(DemoCountries.Association).ToString(Inv) },
                        $"/Registry/Edit/{bk.BeekeeperId}");
                    Expect(linked, "Association saved", $"{c.Code} {b.Key} association");
                }
                if (b.Portal)
                    await PortalAccountAsync(c, ro, regId, m.Beekeepers[b.Key]);
            }

            var b1 = m.Beekeepers["b1"];
            var example = new DemoManifest.Example();
            m.Example = example;

            // ---- the delivery with two sealed containers, confirmed by the beekeeper's SMS code
            var honey = await Db(db => db.HoneyTypes.Where(h => h.HoneyTypeTitle == c.HoneyType).Select(h => h.HoneyTypeId).FirstAsync());
            var period = await Db(db => db.RawHoneyProductionPeriods.Where(p => p.RawHoneyProductionPeriodTitle == DemoReference.DemoPeriodTitle).Select(p => p.RawHoneyProductionPeriodId).FirstAsync());
            var seal = (string kind, int n) => $"{c.Code}-{kind}-{n:D4}";
            // D-97: the plant's seal pool (seal labels issued by the platform), from which every seal of the delivery comes
            var poolCodes = new[] { seal("O", 1001), seal("J", 2001), seal("K", 2001), seal("P", 3001), seal("J", 2002), seal("K", 2002), seal("P", 3003),
                seal("J", 2003), seal("K", 2003), seal("P", 3004) };
            await Db(async db =>
            {
                var pool = new SealBatch { OrgId = Org(DemoCountries.Plant), Kind = SealKind.Label, FirstCode = poolCodes[0], LastCode = poolCodes[^1], CodeCount = poolCodes.Length,
                    IssuedByUserId = "seed", IssuedAt = DateTime.UtcNow, Note = "[DEMO]" };
                foreach (var code in poolCodes)
                    pool.Codes.Add(new SealCode { OrgId = pool.OrgId, Code = code, Kind = SealKind.Label, Status = SealStatus.Issued });
                db.SealBatches.Add(pool);
                return await db.SaveChangesAsync();
            });
            var intake = new Dictionary<string, string>
            {
                ["Number"] = b1.BusinessNumber, ["ApiaryId"] = b1.ApiaryId.ToString(Inv), ["HoneyTypeId"] = honey.ToString(Inv), ["PeriodId"] = period.ToString(Inv),
                ["DeliveryDate"] = _today.AddDays(-7).ToString("yyyy-MM-dd", Inv), ["Province"] = m.Province, ["District"] = b1.District, ["OriginCountry"] = c.Code,
                ["LabOrgId"] = Org(DemoCountries.Lab).ToString(Inv), ["CounterLocation"] = "[DEMO] Counter-sample shelf A3",
                ["RetainUntil"] = _today.AddYears(2).ToString("yyyy-MM-dd", Inv), ["CapType"] = JarCap.SafetyButton,
                ["Containers[0].Closure"] = ClosureKind.SealLabel, ["Containers[0].Kind"] = "Tin", ["Containers[0].NetKg"] = "24.5", ["Containers[0].GrossKg"] = "25.9",
                ["Containers[0].ArrivedSealed"] = "true", ["Containers[0].OriginSeal"] = seal("O", 1001), ["Containers[0].OriginSealIntact"] = "true",
                ["Containers[0].JarSeal"] = seal("J", 2001), ["Containers[0].CounterSeal"] = seal("K", 2001), ["Containers[0].CapIntact"] = "true", ["Containers[0].ClosingSeal"] = seal("P", 3001),
                ["Containers[1].Closure"] = ClosureKind.SealLabel, ["Containers[1].Kind"] = "Tin", ["Containers[1].NetKg"] = "23.75", ["Containers[1].GrossKg"] = "25.1", ["Containers[1].ArrivedSealed"] = "false",
                ["Containers[1].JarSeal"] = seal("J", 2002), ["Containers[1].CounterSeal"] = seal("K", 2002), ["Containers[1].CapIntact"] = "true", ["Containers[1].ClosingSeal"] = seal("P", 3003)
            };
            var sent = await BodyAsync(await sao.PostAsync("/Intake/Record", new(intake) { ["op"] = "send" }, $"/Intake/Create?number={b1.BusinessNumber}"));
            if (!sent.Contains("id=\"intake-confirm\""))
                throw new InvalidOperationException($"{c.Code}: delivery code not sent: " + string.Join(" | ", Regex.Matches(sent, "(?:text-danger|alert-danger|validation-summary-errors)[^>]*>\\s*(?:<ul>\\s*<li>)?([^<]+)").Select(x => x.Groups[1].Value.Trim())));
            var intakeDraft = Regex.Match(sent, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
            var confirmedIntake = await sao.PostAsync("/Intake/Record", new(intake) { ["op"] = "confirm", ["DraftKey"] = intakeDraft, ["Code"] = CodeIn(await LastSmsAsync(intakeDraft)) },
                $"/Intake/Create?number={b1.BusinessNumber}");
            if (confirmedIntake.StatusCode != HttpStatusCode.Redirect)
                throw new InvalidOperationException($"{c.Code}: delivery not confirmed");
            example.IntakeId = int.Parse(confirmedIntake.Headers.Location!.ToString().Split('/').Last(), Inv);
            example.LotId = await Db(db => db.LotIntakes.Where(i => i.IntakeId == example.IntakeId).Select(i => i.RawHoneyProductionId).FirstAsync());
            var lot = example.LotId;

            // ---- the beekeeper's asking price in the portal (possible until the batch is finalized) and consent for the association
            var bee = await LoginAsync(b1.PortalUserName!);
            Expect(await bee.PostFlashAsync("/Me/Price", new() { ["lotId"] = lot.ToString(Inv), ["pricePerKg"] = c.Price.ToString(Inv), ["currency"] = c.Currency, ["note"] = "[DEMO]" }, "/Me/Deliveries"),
                "Price saved", $"{c.Code} price");
            Expect(await bee.PostFlashAsync("/Me/Consent", new() { ["purpose"] = ConsentPurpose.AssociationPrice, ["give"] = "true" }, "/Me/Privacy"), "Consent given", $"{c.Code} consent");

            // ---- CCP 0 (primary audit) by the reception officer, approved by the Quality Assurance Officer
            await CcpAsync(sao, quality, "raw", lot, "CCP 0", c);

            // ---- laboratory: jar of container 1 rejected (cap popped), a new jar taken; composite analysed and approved
            var jars = await Db(db => db.Samples.Where(s => s.RawHoneyProductionId == lot).OrderBy(s => s.SampleId).Select(s => new { s.SampleId, s.ContainerId }).ToListAsync());
            Expect(await analyst.PostFlashAsync("/Samples/Receive", new() { ["id"] = jars[0].SampleId.ToString(Inv), ["sealMatches"] = "true", ["labelIntact"] = "true", ["capIntact"] = "false",
                ["note"] = "[DEMO] The cap's button was up: the jar had been opened." }, "/Samples/Index"), "Jar rejected", $"{c.Code} jar 1");
            Expect(await analyst.PostFlashAsync("/Samples/Receive", new() { ["id"] = jars[1].SampleId.ToString(Inv), ["sealMatches"] = "true", ["labelIntact"] = "true", ["capIntact"] = "true" },
                "/Samples/Index"), "Jar received", $"{c.Code} jar 2");
            Expect(await sao.PostFlashAsync("/Samples/Resample", new() { ["containerId"] = jars[0].ContainerId!.Value.ToString(Inv), ["jarSeal"] = seal("J", 2003),
                ["counterSeal"] = seal("K", 2003), ["capIntact"] = "true", ["reseal"] = seal("P", 3004) }, $"/Intake/Details/{example.IntakeId}"), "sent", $"{c.Code} new jar");
            var newJar = await Db(db => db.Samples.Where(s => s.RawHoneyProductionId == lot).OrderByDescending(s => s.SampleId).Select(s => s.SampleId).FirstAsync());
            Expect(await analyst.PostFlashAsync("/Samples/Receive", new() { ["id"] = newJar.ToString(Inv), ["sealMatches"] = "true", ["labelIntact"] = "true", ["capIntact"] = "true" },
                "/Samples/Index"), "Jar received", $"{c.Code} new jar received");
            var made = await analyst.PostAsync("/Samples/Composite", new() { ["lotId"] = lot.ToString(Inv) }, "/Samples/Index");
            var composite = int.Parse((made.Headers.Location?.ToString() ?? "").Split('/').Last(), Inv);
            var reportNumber = $"DEMO-{c.Code}-LAB-2026-0101";
            var results = new Dictionary<string, string> { ["reportNumber"] = reportNumber, ["issuedOn"] = _today.AddDays(-4).ToString("yyyy-MM-dd", Inv) };
            foreach (var p in await ParametersAsync("CCP 1"))
                results[$"values[{p.ParameterId}]"] = DemoValues.For("CCP 1", p, c);
            var lab = c.Org(DemoCountries.Lab);
            Expect(await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{composite}", results,
                ("reportFile", "demo-lab-report.pdf", DemoFiles.Pdf("[DEMO] Laboratory analysis report " + reportNumber, lab.Name,
                    $"Accreditation {lab.Accreditation} (demo). Composite sample of lot {lot}.",
                    "Test data for the TOP4HoneyChains partner demo. Not a real analysis."), "application/pdf"), $"/LabReports/Edit/{composite}")),
                "Lab results saved", $"{c.Code} lab results");
            Expect(await analyst.PostFlashAsync($"/LabReports/Submit/{composite}", new(), $"/LabReports/Edit/{composite}"), "Report submitted", $"{c.Code} lab submit");
            Expect(await labManager.PostFlashAsync($"/LabReports/Decide/{composite}", new() { ["approve"] = "true" }, $"/LabReports/Edit/{composite}"), "approved", $"{c.Code} lab approval");
            example.ReportNumber = reportNumber;

            // ---- CCP 2 and 3 of the lot
            foreach (var code in new[] { "CCP 2", "CCP 3" })
                await CcpAsync(staff, quality, "raw", lot, code, c);

            // ---- the batch (administrator), its CCPs (plant), finalizing (two people)
            var plantId = Org(DemoCountries.Plant);
            var wholesaler = await Db(db => db.Organizations.Where(o => o.OrgId == plantId).Select(o => o.LegacyWholesalerId!.Value).FirstAsync());
            var created = await admin.PostAsync("/FinalHoneyProductions/Create", new()
            {
                ["FinalHoneyProductionWholesalerId"] = wholesaler.ToString(Inv), ["FinalHoneyProductionTitle"] = c.BatchTitle,
                ["FinalHoneyProductionDate"] = _today.AddDays(-2).ToString("yyyy-MM-dd", Inv)
            }, $"/FinalHoneyProductions/Create?wholesalerId={wholesaler}");
            var batch = int.Parse(Regex.Match(created.Headers.Location?.ToString() ?? "", @"ManageBatch/(\d+)").Groups[1].Value, Inv);
            await admin.PostAsync("/FinalHoneyProductions/ManageBatchPost", new() { ["batchId"] = batch.ToString(Inv), [$"usedAmounts[{lot}]"] = "48.25" }, $"/FinalHoneyProductions/ManageBatch/{batch}");
            var batchCcps = await Db(db => db.CriticalControlPoints.Where(x => x.AppliesTo == CcpAppliesTo.Batch).OrderBy(x => x.Id).Select(x => new { x.Id, x.Code }).ToListAsync());
            var assign = new Dictionary<string, string> { ["FinalHoneyProductionId"] = batch.ToString(Inv) };
            for (var i = 0; i < batchCcps.Count; i++)
                assign[$"SelectedCCPIds[{i}]"] = batchCcps[i].Id.ToString(Inv);
            await admin.PostAsync($"/FinalHoneyProductions/ManageCCP/{batch}", assign, $"/FinalHoneyProductions/ManageCCP/{batch}");
            await Db(async db =>
            {
                var b = await db.FinalHoneyProductions.FirstAsync(x => x.FinalHoneyProductionId == batch);
                b.OwnerOrgId = Org(DemoCountries.Plant);
                return await db.SaveChangesAsync();
            });
            foreach (var ccp in batchCcps)
                await CcpAsync(staff, quality, "batch", batch, ccp.Code, c);
            Expect(await staff.PostFlashAsync($"/OrgWorkspace/RequestFinalize/{batch}", new(), $"/OrgWorkspace/Batch/{batch}"), "Finalizing requested", $"{c.Code} finalize request");
            Expect(await quality.PostFlashAsync($"/OrgWorkspace/Finalize/{batch}", new(), $"/OrgWorkspace/Batch/{batch}"), "Batch finalized", $"{c.Code} finalize");
            example.BatchId = batch;
            example.BatchTitle = c.BatchTitle;
            example.Qr = (await Db(db => db.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == batch).Select(f => f.FinalHoneyProductionQR).FirstAsync())).ToString();

            // ---- shipment to the shop, receipt and storage (CCP 10 at the holder)
            Expect(await staff.PostFlashAsync("/Shipments/Create", new() { ["batchId"] = batch.ToString(Inv), ["toOrgId"] = Org(DemoCountries.Shop).ToString(Inv), ["quantity"] = "20" },
                "/Shipments/Index"), "Shipment created", $"{c.Code} shipment");
            var shipment = await Db(db => db.Shipments.Where(s => s.FinalHoneyProductionId == batch).Select(s => s.ShipmentId).FirstAsync());
            Expect(await shop.PostFlashAsync("/Shipments/Decide", new() { ["id"] = shipment.ToString(Inv), ["confirm"] = "true" }, "/Shipments/Index"), "Receipt confirmed", $"{c.Code} receipt");
            var storage = new Dictionary<string, string> { ["batchId"] = batch.ToString(Inv) };
            foreach (var p in await ParametersAsync("CCP 10", includeFiles: true))
                storage[$"values[{p.ParameterId}]"] = DemoValues.For("CCP 10", p, c);
            Expect(await shop.PostFlashAsync("/Shipments/Storage", storage, $"/Shipments/Storage?batchId={batch}"), "Storage values saved", $"{c.Code} storage");
            Expect(await shopQuality.PostFlashAsync("/Shipments/ApproveStorage", new() { ["batchId"] = batch.ToString(Inv) }, $"/Shipments/Storage?batchId={batch}"), "approved", $"{c.Code} storage approval");

            // ---- the association records the beekeeper's consent for a public beekeeper page (name shown)
            Expect(await association.FlashAfterAsync(await association.PostMultipartAsync($"/Storefronts/Consent/{b1.Id}",
                new() { ["textRead"] = "true", ["nameConsented"] = "true", ["givenOn"] = _today.AddDays(-1).ToString("yyyy-MM-dd", Inv) },
                ("document", "demo-consent.pdf", DemoFiles.Pdf("[DEMO] Consent for a public beekeeper page", b1.Name, c.Org(DemoCountries.Association).Name,
                    "Test data for the TOP4HoneyChains partner demo. Not a real signature."), "application/pdf"), $"/Storefronts/Consent/{b1.Id}")),
                "published", $"{c.Code} beekeeper page");
            example.StorefrontCode = await Db(db => db.BeekeeperStorefronts.Where(s => s.BeekeeperId == b1.Id).Select(s => s.Code).FirstAsync());
        }

        // A CCP record entered by one person and approved by another
        private async Task CcpAsync(UserClient enter, UserClient approve, string kind, int ownerId, string code, DemoCountry c)
        {
            var ccpId = await Db(db => db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).FirstAsync());
            var parameters = await ParametersAsync(code);
            var page = kind == "raw" ? "RawHoneyProductionManageCCP" : "FinalHoneyProductionManageCCP";
            var idField = kind == "raw" ? "RawHoneyProductionId" : "FinalHoneyProductionId";
            var from = $"/CCPMonitoring/{page}?{(kind == "raw" ? "rawHoneyProductionId" : "finalHoneyProductionId")}={ownerId}&ccpId={ccpId}";
            var form = new Dictionary<string, string> { ["CCPId"] = ccpId.ToString(Inv), [idField] = ownerId.ToString(Inv) };
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                form[$"Groups[0].Parameters[{i}].ParameterId"] = p.ParameterId.ToString(Inv);
                var value = DemoValues.For(code, p, c);
                if (value.Length > 0)
                    form[p.DataType == CcpDataType.Number ? $"Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Groups[0].Parameters[{i}].MeasuredTextValue"] = value;
            }
            Expect(await enter.PostFlashAsync($"/CCPMonitoring/{page}", form, from), "values saved", $"{c.Code} {code} entry");
            Expect(await approve.PostFlashAsync("/CCPMonitoring/Approve", new() { ["ownerKind"] = kind, ["ownerId"] = ownerId.ToString(Inv), ["ccpId"] = ccpId.ToString(Inv) }, from),
                "approved", $"{c.Code} {code} approval");
        }

        private Task<List<CriticalControlPointParameter>> ParametersAsync(string code, bool includeFiles = false) => Db(db => db.CriticalControlPointParameters.AsNoTracking()
            .Where(p => p.CCPId == db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).First() && (includeFiles || p.DataType != CcpDataType.File))
            .OrderBy(p => p.ParameterId).ToListAsync());

        // The registration officer sends the sign-in details; the beekeeper signs in with the temporary password and chooses a password
        private async Task PortalAccountAsync(DemoCountry c, UserClient ro, int regId, DemoManifest.Beekeeper b)
        {
            Expect(await ro.PostFlashAsync($"/Registration/IssueSignIn/{regId}", new(), $"/Registration/Details/{regId}"), "sign-in details were sent", $"{c.Code} sign-in details");
            var userId = await Db(db => db.BeekeeperAccounts.Where(a => a.BeekeeperId == b.Id).Select(a => a.UserId).FirstAsync());
            string userName;
            using (var scope = _app.Services.CreateScope())
                userName = (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(userId))!.UserName!;
            var account = new { UserId = userId, UserName = userName };
            var sms = await LastSmsAsync(account.UserId);
            foreach (Match candidate in Regex.Matches(sms, @"(?<![A-Za-z0-9-])[ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789]{10}(?![A-Za-z0-9-])"))
            {
                var (client, response) = await UserClient.SignInAsync(_app, account.UserName!, candidate.Value);
                if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
                    continue;
                var password = NewPassword();
                var changed = await client.PostAsync("/Account/FirstPassword", new() { ["password"] = password, ["confirmPassword"] = password }, "/Account/FirstPassword");
                if (changed.StatusCode != HttpStatusCode.Redirect)
                    throw new InvalidOperationException($"{c.Code}: the beekeeper's first password was refused");
                _passwords[account.UserName!] = password;
                _credentials.Add(new Credential(c.Code, account.UserName!, password, "beekeeper", "Beekeeper (portal)", b.Name));
                b.PortalUserName = account.UserName;
                return;
            }
            throw new InvalidOperationException($"{c.Code}: no temporary password found in the sign-in SMS");
        }
    }
}
