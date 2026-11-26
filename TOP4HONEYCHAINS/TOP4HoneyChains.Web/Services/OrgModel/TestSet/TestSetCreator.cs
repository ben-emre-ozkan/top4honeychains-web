using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel.TestSet
{
    public sealed record TestSetLogin(string UserName, string Password, string Person, string Organization, string Role, int Order, int? Set);

    public sealed record TestSetSetInfo(int No, string ConsumerUser, string ConsumerName, string BeekeeperName, string BeekeeperBirth,
        string RegistryNumber, string BeekeeperPhone, string Apiary, string SealLabels, string PlasticSeals, string Container);

    public sealed record TestSetResult(IReadOnlyList<TestSetLogin> Logins, IReadOnlyList<TestSetSetInfo> Sets, int BatchId, string BatchTitle, Guid Qr,
        IReadOnlyList<int> Lots, string? CompanyLabels);

    // D-112: builds the live site's test set through the application itself: the cast, roles, flow and starting state of tools/live-like
    // (D-88), with the organizations marked as test data (Organizations.IsTestData) and every account in TestSetAccounts. Only the
    // platform operator's account is made with the app's user services; everything else runs through the app's own pages, hosted in
    // memory (no network port), as the person who does it: the operator creates and verifies the organizations and adds the members
    // (each signs in with the details of the SMS in the test inbox and sets the generated password), the registration officer
    // registers the beekeeper with the SMS code, the deliveries, the laboratory, the control sheets, New batch, finalizing, the
    // shipment and the storage at the shop. The seal pool, the containers and the consumers use the app's services. Never writes or
    // logs a password.
    public sealed class TestSetCreator
    {
        public const string BaseAddress = "https://localhost";
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private readonly IServiceProvider _services;
        private readonly Func<HttpMessageHandler> _handler;
        private readonly Action<string> _log;
        private readonly JsonElement _cast;
        private readonly JsonElement _values;
        private readonly JsonElement _history;
        private readonly List<JsonElement> _people;
        private readonly Dictionary<string, string> _passwords = new();
        private readonly Dictionary<string, Client> _clients = new();
        private readonly Dictionary<string, int> _orgIds = new();

        public TestSetCreator(IServiceProvider services, Func<HttpMessageHandler> handler, Action<string> log)
        {
            _services = services;
            _handler = handler;
            _log = log;
            _cast = Cast();
            _values = _cast.GetProperty("values");
            _history = _cast.GetProperty("history");
            _people = _cast.GetProperty("people").EnumerateArray().ToList();
        }

        public const int MaxSets = 12;

        public static JsonElement Cast()
        {
            using var stream = typeof(TestSetCreator).Assembly.GetManifestResourceStream("TOP4HoneyChains.Web.TestSetCast.json")
                               ?? throw new InvalidOperationException("The test set cast is missing from the application.");
            return JsonDocument.Parse(stream).RootElement.Clone();
        }

        // every user name of the test set for the given number of sets
        public static IReadOnlyList<string> UserNames(int sets)
        {
            var cast = Cast();
            return cast.GetProperty("people").EnumerateArray().Select(p => p.GetProperty("user").GetString()!)
                .Concat(cast.GetProperty("sets").EnumerateArray().Take(sets).Select(s => s.GetProperty("consumer").GetProperty("user").GetString()!))
                .Distinct().ToList();
        }

        public static string OrgName(JsonElement org) => "TEST " + org.GetProperty("name").GetString();

        // the registry number of a person of the test set (official format TR + 34 + 10 digits; 9999 00 group person)
        public static string RegistryNumber(int group, int person) => "TR34999900" + group.ToString("D2", Inv) + person.ToString("D2", Inv);

        // ---------------------------------------------------------------- passwords

        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

        // 4 groups of 4 characters (about 92 bits); a new one for every account, generated here and only written to the --out folder
        public static string NewPassword()
        {
            var b = new StringBuilder();
            for (var i = 0; i < 16; i++)
            {
                if (i > 0 && i % 4 == 0)
                    b.Append('-');
                b.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
            }
            return b.ToString();
        }

        private string PasswordOf(string user) => _passwords.TryGetValue(user, out var p) ? p : _passwords[user] = NewPassword();

        // ---------------------------------------------------------------- the run

        private T Svc<T>(IServiceScope s) where T : notnull => s.ServiceProvider.GetRequiredService<T>();

        private async Task<TResult> Db<TResult>(Func<TOP4HoneyChainsContext, Task<TResult>> work)
        {
            using var scope = _services.CreateScope();
            return await work(Svc<TOP4HoneyChainsContext>(scope));
        }

        private Task MarkAccountAsync(string userId, string kind, int? set = null) => Db(async db =>
        {
            if (!await db.TestSetAccounts.AnyAsync(a => a.UserId == userId))
                db.TestSetAccounts.Add(new TestSetAccount { UserId = userId, Kind = kind, SetNo = set, CreatedAt = DateTime.UtcNow });
            return await db.SaveChangesAsync();
        });

        private async Task<string> UserIdAsync(string user)
        {
            using var scope = _services.CreateScope();
            return (await Svc<UserManager<ApplicationUser>>(scope).FindByNameAsync(user))?.Id ?? throw new InvalidOperationException($"{user}: the account was not made");
        }

        public async Task<TestSetResult> CreateAsync(int sets)
        {
            var inv = Inv;
            var today = DateTime.Today;

            // ---- 1. the platform operator: the app's user services, Administrator role, platform membership, a test-set account
            var op = _cast.GetProperty("operator");
            var opUser = op.GetProperty("user").GetString()!;
            string operatorId;
            using (var scope = _services.CreateScope())
            {
                var users = Svc<UserManager<ApplicationUser>>(scope);
                var user = new ApplicationUser { UserName = opUser, PhoneNumber = TestPhones.Number(0, op.GetProperty("phone").GetInt32()), PhoneNumberConfirmed = true, LockoutEnabled = true };
                var created = await users.CreateAsync(user, PasswordOf(opUser));
                if (!created.Succeeded)
                    throw new InvalidOperationException("operator: " + string.Join(" ", created.Errors.Select(e => e.Description)));
                operatorId = user.Id;
                await MarkAccountAsync(user.Id, TestSetAccountKind.Operator);
                var inRole = await users.AddToRoleAsync(user, CurrentOrg.AdminRole);
                if (!inRole.Succeeded)
                    throw new InvalidOperationException("operator role: " + string.Join(" ", inRole.Errors.Select(e => e.Description)));
                var db = Svc<TOP4HoneyChainsContext>(scope);
                var platform = await db.Organizations.Where(o => o.IsPlatform).Select(o => o.OrgId).FirstAsync();
                db.Memberships.Add(new Membership { UserId = user.Id, OrgId = platform, OrgRole = OrgRoles.OrgAdmin, Status = MembershipStatus.Active, GrantedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                await AuditChain.AppendAsync(db, new[] { new AuditRecord(DateTime.UtcNow, null, "create-test-set (D-112)", platform, "Create", "AspNetUsers", user.Id, null,
                    JsonSerializer.Serialize(new { user.UserName, Role = CurrentOrg.AdminRole, TestSet = true }), "TestSet", null) });
            }
            var operatorClient = await SignInAsync(opUser);
            _log($"platform operator {opUser}");

            // ---- 2. organizations and members through the operator's pages; each member signs in with the SMS details
            foreach (var o in _cast.GetProperty("orgs").EnumerateArray())
            {
                var key = o.GetProperty("key").GetString()!;
                var name = OrgName(o);
                var caps = o.GetProperty("capabilities").EnumerateArray().Select(x => x.GetString()!).ToList();
                var accreditation = o.TryGetProperty("accreditation", out var a) ? a.GetString() : null;
                var form = new List<KeyValuePair<string, string>>
                {
                    new("DisplayName", name), new("LegalName", "TEST " + o.GetProperty("legal").GetString()),
                    new("Province", o.GetProperty("province").GetString()!), new("District", o.GetProperty("district").GetString()!)
                };
                form.AddRange(caps.Select(cap => new KeyValuePair<string, string>("capabilities", cap)));
                if (accreditation != null)
                    form.AddRange(new KeyValuePair<string, string>[]
                    {
                        new("isAccredited", "true"), new("accreditationNumber", accreditation),
                        new("accreditationValidUntil", today.AddYears(3).ToString("yyyy-MM-dd", inv))
                    });
                var created = await operatorClient.PostAsync("/Organizations/Create", form, "/Organizations/Create");
                var id = int.Parse(Regex.Match(created.Headers.Location?.ToString() ?? "", @"/Organizations/Details/(\d+)").Groups[1].Value is { Length: > 0 } s ? s
                    : throw new InvalidOperationException($"{name}: not created: {Errors(await created.Content.ReadAsStringAsync())}"), inv);
                _orgIds[key] = id;
                // the flag first (the operator is a test-set account, so the page set it already; made sure here)
                await Db(async db =>
                {
                    var org = await db.Organizations.FirstAsync(x => x.OrgId == id);
                    org.IsTestData = true;
                    return await db.SaveChangesAsync();
                });
                var document = Pdf("TEST - " + (accreditation != null ? "Akreditasyon sertifikasi" : "Faaliyet belgesi"), name, "Test verisi - gercek bir belge degildir",
                    $"{o.GetProperty("district").GetString()} / {o.GetProperty("province").GetString()}");
                Expect(await operatorClient.FlashAfterAsync(await operatorClient.PostMultipartAsync("/Organizations/UploadDocument",
                    new() { new("id", id.ToString(inv)), new("kind", accreditation != null ? "Accreditation" : "Licence") },
                    ("file", accreditation != null ? "test-akreditasyon.pdf" : "test-faaliyet-belgesi.pdf", document), $"/Organizations/Details/{id}")), "uploaded", $"{name} document");

                // the Compliance Officer first: verification needs one
                foreach (var p in _people.Where(p => p.GetProperty("org").GetString() == key).OrderBy(p => p.GetProperty("role").GetString() == OrgRoles.ComplianceOfficer ? 0 : 1))
                {
                    var user = p.GetProperty("user").GetString()!;
                    var added = await operatorClient.PostFlashAsync($"/Organizations/AddMember/{id}", new()
                    {
                        new("orgRole", p.GetProperty("role").GetString()!), new("newUserName", user), new("newPhone", TestPhones.Number(0, p.GetProperty("phone").GetInt32()))
                    }, $"/Organizations/Details/{id}");
                    Expect(added, "added as", $"{user} member");
                    await MarkAccountAsync(await UserIdAsync(user), TestSetAccountKind.Member);
                    await FirstSignInAsync(user);
                }
                Expect(await operatorClient.PostFlashAsync("/Organizations/ChangeStatus", new() { new("id", id.ToString(inv)), new("toStatus", OrgStatus.Verified) }, $"/Organizations/Details/{id}"),
                    "now Verified", $"{name} verification");
                _log($"organization {name}: {_people.Count(p => p.GetProperty("org").GetString() == key)} member(s), verified");
            }
            string? companyLabels;
            using (var scope = _services.CreateScope())
            {
                var issued = await Svc<SealPoolService>(scope).IssueLabelsAsync(_orgIds["company"], 30, operatorId, "test set (D-112)");
                companyLabels = $"{issued.FirstCode} - {issued.LastCode}";
            }

            // the harvest season of the test set (reference data, Set-up), marked as test data
            var start = today.AddDays(-120);
            var periodTitle = _history.GetProperty("period").GetString()!.Replace("{year}", today.Year.ToString(inv));
            await operatorClient.PostAsync("/RawHoneyProductionPeriods/Create", new()
            {
                new("RawHoneyProductionPeriodTitle", periodTitle), new("RawHoneyProductionPeriodStartDate", start.ToString("yyyy-MM-dd", inv)),
                new("RawHoneyProductionPeriodEndDate", today.AddDays(120).ToString("yyyy-MM-dd", inv))
            }, "/RawHoneyProductionPeriods/Create");
            var period = await Db(async db =>
            {
                var p = await db.RawHoneyProductionPeriods.Where(x => x.RawHoneyProductionPeriodTitle == periodTitle).OrderByDescending(x => x.RawHoneyProductionPeriodId).FirstOrDefaultAsync()
                        ?? throw new InvalidOperationException("the harvest season was not created");
                p.IsTestData = true;
                await db.SaveChangesAsync();
                return p.RawHoneyProductionPeriodId;
            });

            // ---- 3. the registered beekeeper: registration by the officer (SMS code from the test inbox), association, portal account
            var bee = _cast.GetProperty("beekeeper");
            var beePhone = TestPhones.Number(0, bee.GetProperty("phone").GetInt32());
            var registry = RegistryNumber(0, bee.GetProperty("phone").GetInt32());
            var officer = await As(User("RegistrationOfficer", "directorate"));
            var found = await officer.PostAsync("/Registration/Find", new() { new("number", registry) }, "/Registration/Index");
            if (!(found.Headers.Location?.ToString() ?? "").StartsWith("/Registration/New", StringComparison.Ordinal))
                throw new InvalidOperationException("registration did not start: " + await officer.FlashAfterAsync(found));
            var beeProvince = bee.GetProperty("provinceCode").GetString()!;
            var beeDistrict = await Db(db => db.Districts.Where(d => d.ProvinceCode == beeProvince && d.Name == bee.GetProperty("district").GetString()).Select(d => d.DistrictId).FirstAsync());
            var registration = new List<KeyValuePair<string, string>>
            {
                new("RegistryNumber", registry), new("FirstName", bee.GetProperty("first").GetString()!), new("LastName", bee.GetProperty("last").GetString()!),
                new("BirthDate", bee.GetProperty("birth").GetString()!), new("Phone", beePhone), new("Language", "tr"),
                new("ApiaryLatitude", bee.GetProperty("lat").GetString()!), new("ApiaryLongitude", bee.GetProperty("lng").GetString()!), new("ApiaryProvince", beeProvince),
                new("ApiaryDistrictId", beeDistrict.ToString(inv)), new("ApiaryLocality", bee.GetProperty("locality").GetString()!), new("ApiaryHives", bee.GetProperty("hives").GetString()!),
                new("PublicName", "true"), new("PortalNotifications", "true"), new("Attest", "true"), new("IssueSignIn", "true"),
                new("AssociationOrgId", _orgIds["association"].ToString(inv))
            };
            var sentHtml = WebUtility.HtmlDecode(await (await officer.PostAsync("/Registration/Submit", With(registration, ("op", "send")), "/Registration/New")).Content.ReadAsStringAsync());
            var draft = Regex.Match(sentHtml, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
            if (draft.Length == 0)
                throw new InvalidOperationException("registration code not sent: " + Errors(sentHtml));
            var confirmed = await officer.PostAsync("/Registration/Submit", With(registration, ("op", "confirm"), ("DraftKey", draft), ("Code", CodeIn(await LastSmsAsync(draft)))), "/Registration/New");
            Expect(await officer.FlashAfterAsync(confirmed), "registered and verified", "registration");
            var regId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last(), inv);
            var registered = await Db(db => db.BeekeeperRegistrations.Where(r => r.RegistrationId == regId)
                .Select(r => new { r.BeekeeperId, r.Beekeeper!.BeekeeperBusinessNumber }).FirstAsync());
            var portalUser = await PortalAccountAsync(officer, regId, registered.BeekeeperId);
            _log($"beekeeper registered ({registered.BeekeeperBusinessNumber}), portal account {portalUser}");

            // the beekeeper's second apiary: the officer finds the beekeeper again and adds it on the registration form (a new SMS code)
            var second = bee.GetProperty("apiary2");
            var again = await officer.PostAsync("/Registration/Find", new() { new("number", registry) }, "/Registration/Index");
            var reverifyUrl = again.Headers.Location?.ToString() ?? "";
            if (!reverifyUrl.StartsWith("/Registration/Reverify", StringComparison.Ordinal))
                throw new InvalidOperationException("second apiary: the beekeeper was not found: " + await officer.FlashAfterAsync(again));
            var secondDistrict = await Db(db => db.Districts.Where(d => d.ProvinceCode == beeProvince && d.Name == second.GetProperty("district").GetString()).Select(d => d.DistrictId).FirstAsync());
            var update = registration.Where(p => !p.Key.StartsWith("Apiary", StringComparison.Ordinal) && p.Key != "IssueSignIn").ToList();
            update.AddRange(new KeyValuePair<string, string>[]
            {
                new("BeekeeperId", registered.BeekeeperId), new("ApiaryLatitude", second.GetProperty("lat").GetString()!), new("ApiaryLongitude", second.GetProperty("lng").GetString()!),
                new("ApiaryProvince", beeProvince), new("ApiaryDistrictId", secondDistrict.ToString(inv)), new("ApiaryLocality", second.GetProperty("locality").GetString()!),
                new("ApiaryHives", second.GetProperty("hives").GetString()!)
            });
            var updateSent = WebUtility.HtmlDecode(await (await officer.PostAsync("/Registration/Submit", With(update, ("op", "send")), reverifyUrl)).Content.ReadAsStringAsync());
            if (!updateSent.Contains("id=\"registration-confirm\""))
                throw new InvalidOperationException("second apiary: code not sent: " + Errors(updateSent));
            var updated = await officer.PostAsync("/Registration/Submit", With(update, ("op", "confirm"), ("Code", CodeIn(await LastSmsAsync(registered.BeekeeperId)))), reverifyUrl);
            Expect(await officer.FlashAfterAsync(updated), "registered and verified", "second apiary");
            var apiaries = await Db(db => db.Apiaries.Where(x => x.ApiaryBeekeeperId == registered.BeekeeperId).OrderBy(x => x.ApiaryId).Select(x => x.ApiaryId).ToListAsync());
            _log($"second apiary added ({apiaries.Count} apiaries)");

            // ---- 4. two deliveries, the lab, CCP 0-1 for each lot; the association path (ready for packing)
            var reception = await As(User("SampleAcceptanceOfficer", "association"));
            var staff = await As(User("Staff", "association"));
            var quality = await As(User("QualityApprover", "association"));
            var analyst = await As(User("Analyst", "lab"));
            var labManager = await As(User("LabManager", "lab"));
            var honeyType = await Db(db => db.HoneyTypes.Where(h => h.HoneyTypeTitle == _history.GetProperty("honeyType").GetString()).Select(h => h.HoneyTypeId).FirstAsync());
            var lots = new List<(int Lot, decimal Kg)>();
            var seals = _history.GetProperty("seals");
            var sealNo = seals.GetProperty("first").GetInt32();
            var deliveryNo = 0;
            // the history's seals in the association's seal pool: the harvest seals as numbered plastic seals, the jar labels and the
            // closing seals as seal labels (TEST- codes, put into the pool the way an administrator's batch would be)
            {
                var deliveries = _history.GetProperty("deliveries").EnumerateArray().ToList();
                var origins = deliveries.SelectMany(d => d.GetProperty("containers").EnumerateArray().Select(c => c.GetProperty("originSeal").GetString()!)).ToList();
                var labels = Enumerable.Range(sealNo, origins.Count)
                    .SelectMany(n => new[] { "jar", "counter", "reseal" }.Select(k => seals.GetProperty(k).GetString() + n.ToString(inv))).ToList();
                foreach (var (kind, codes) in new[] { (SealKind.Plastic, origins), (SealKind.Label, labels) })
                    await Db(async db =>
                    {
                        var batch = new SealBatch { OrgId = _orgIds["association"], Kind = kind, FirstCode = codes[0], LastCode = codes[^1], CodeCount = codes.Count,
                            IssuedByUserId = operatorId, IssuedAt = DateTime.UtcNow, Note = "test set history (D-112)" };
                        foreach (var code in codes)
                            batch.Codes.Add(new SealCode { OrgId = batch.OrgId, Code = code, Kind = kind, Status = SealStatus.Issued });
                        db.SealBatches.Add(batch);
                        return await db.SaveChangesAsync();
                    });
            }
            foreach (var d in _history.GetProperty("deliveries").EnumerateArray())
            {
                deliveryNo++;
                var date = today.AddDays(-d.GetProperty("daysAgo").GetInt32());
                var form = new List<KeyValuePair<string, string>>
                {
                    new("Number", registered.BeekeeperBusinessNumber!), new("ApiaryId", apiaries[d.GetProperty("apiary").GetInt32() - 1].ToString(inv)), new("HoneyTypeId", honeyType.ToString(inv)),
                    new("PeriodId", period.ToString(inv)), new("DeliveryDate", date.ToString("yyyy-MM-dd", inv)), new("Province", bee.GetProperty("province").GetString()!),
                    new("District", (d.GetProperty("apiary").GetInt32() == 1 ? bee : second).GetProperty("district").GetString()!), new("OriginCountry", "TR"), new("LabOrgId", _orgIds["lab"].ToString(inv)),
                    new("CounterLocation", _history.GetProperty("counterLocation").GetString()!), new("RetainUntil", today.AddYears(2).ToString("yyyy-MM-dd", inv)),
                    new("CapType", JarCap.SafetyButton)
                };
                var i = 0;
                decimal kg = 0;
                foreach (var c in d.GetProperty("containers").EnumerateArray())
                {
                    var n = sealNo++;
                    form.AddRange(new KeyValuePair<string, string>[]
                    {
                        new($"Containers[{i}].Closure", ClosureKind.SealLabel),
                        new($"Containers[{i}].Kind", "Tin"), new($"Containers[{i}].NetKg", c.GetProperty("net").GetString()!), new($"Containers[{i}].GrossKg", c.GetProperty("gross").GetString()!),
                        new($"Containers[{i}].ArrivedSealed", "true"), new($"Containers[{i}].OriginSeal", c.GetProperty("originSeal").GetString()!), new($"Containers[{i}].OriginSealIntact", "true"),
                        new($"Containers[{i}].JarSeal", seals.GetProperty("jar").GetString() + n), new($"Containers[{i}].CounterSeal", seals.GetProperty("counter").GetString() + n),
                        new($"Containers[{i}].CapIntact", "true"), new($"Containers[{i}].ClosingSeal", seals.GetProperty("reseal").GetString() + n)
                    });
                    kg += decimal.Parse(c.GetProperty("net").GetString()!, inv);
                    i++;
                }
                var from = $"/Intake/Create?number={registered.BeekeeperBusinessNumber}";
                var sent = WebUtility.HtmlDecode(await (await reception.PostAsync("/Intake/Record", With(form, ("op", "send")), from)).Content.ReadAsStringAsync());
                if (sent.Contains("Wait a minute before sending a new code."))
                {
                    // one code per minute for the same record: the officer waits and sends again
                    await Task.Delay(TimeSpan.FromSeconds(62));
                    sent = WebUtility.HtmlDecode(await (await reception.PostAsync("/Intake/Record", With(form, ("op", "send")), from)).Content.ReadAsStringAsync());
                }
                var intakeDraft = Regex.Match(sent, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
                if (intakeDraft.Length == 0 || !sent.Contains("id=\"intake-confirm\""))
                    throw new InvalidOperationException($"delivery {deliveryNo}: code not sent: " + Errors(sent));
                var done = await reception.PostAsync("/Intake/Record", With(form, ("op", "confirm"), ("DraftKey", intakeDraft), ("Code", CodeIn(await LastSmsAsync(intakeDraft)))), from);
                if (done.StatusCode != HttpStatusCode.Redirect)
                    throw new InvalidOperationException($"delivery {deliveryNo} not confirmed: " + Errors(await done.Content.ReadAsStringAsync()));
                var intakeId = int.Parse(done.Headers.Location!.ToString().Split('/').Last(), inv);
                var lot = await Db(db => db.LotIntakes.Where(x => x.IntakeId == intakeId).Select(x => x.RawHoneyProductionId).FirstAsync());

                await SingleCcpAsync(reception, quality, "raw", lot, "CCP 0");

                // the laboratory: the jars received with their seals checked, one composite sample analysed, the report approved by the manager
                foreach (var jar in await Db(db => db.Samples.Where(s => s.RawHoneyProductionId == lot && s.Status == SampleStatus.Sent).Select(s => s.SampleId).ToListAsync()))
                    Expect(await analyst.PostFlashAsync("/Samples/Receive", new() { new("id", jar.ToString(inv)), new("sealMatches", "true"), new("labelIntact", "true"), new("capIntact", "true") },
                        "/Samples/Index"), "received", $"lot {lot} jar {jar}");
                var made = await analyst.PostAsync("/Samples/Composite", new() { new("lotId", lot.ToString(inv)) }, "/Samples/Index");
                var composite = int.Parse((made.Headers.Location?.ToString() ?? "").Split('/').Last(), inv);
                var reportNumber = _history.GetProperty("reportPrefix").GetString() + (1180 + deliveryNo).ToString(inv);
                var results = new List<KeyValuePair<string, string>> { new("reportNumber", reportNumber), new("issuedOn", date.AddDays(4).ToString("yyyy-MM-dd", inv)) };
                foreach (var p in await ParametersAsync("CCP 1"))
                    results.Add(new($"values[{p.ParameterId}]", Value("CCP 1", p)));
                var labOrg = _cast.GetProperty("orgs").EnumerateArray().First(o => o.GetProperty("key").GetString() == "lab");
                Expect(await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{composite}", results,
                    ("reportFile", $"{reportNumber}.pdf", Pdf("TEST - Analiz raporu " + reportNumber, OrgName(labOrg), "Test verisi - gercek bir rapor degildir",
                        $"Bilesik numune, lot {lot}", "Bal analizi: nem, HMF, diastaz, prolin, sekerler, iletkenlik")),
                    $"/LabReports/Edit/{composite}")), "saved", $"lot {lot} lab results");
                Expect(await analyst.PostFlashAsync($"/LabReports/Submit/{composite}", new(), $"/LabReports/Edit/{composite}"), "submitted", $"lot {lot} lab submit");
                Expect(await labManager.PostFlashAsync($"/LabReports/Decide/{composite}", new() { new("approve", "true") }, $"/LabReports/Edit/{composite}"), "approved", $"lot {lot} lab approval");
                await ApproveSingleAsync(quality, "raw", lot, "CCP 1");

                // D-98: the association path: after the approved lab report the Quality Assurance Officer marks the honey ready for packing
                var ccp1Id = await CcpIdAsync("CCP 1");
                Expect(await quality.PostFlashAsync("/CCPMonitoring/PackingReady", new() { new("lotId", lot.ToString(inv)), new("ccpId", ccp1Id.ToString(inv)) },
                    $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={lot}&ccpId={ccp1Id}"), "ready for packing", $"lot {lot} ready for packing");
                lots.Add((lot, kg));
                _log($"delivery {deliveryNo}: lot {lot}, {kg.ToString("0.##", inv)} kg, lab report {reportNumber}, CCP 0-1 approved, ready for packing");
            }

            // ---- 5. the batch: New batch by the association, the batch control sheet, finalizing by two people
            var batchTitle = _history.GetProperty("batchTitle").GetString()!;
            var newBatch = new List<KeyValuePair<string, string>> { new("title", batchTitle), new("date", today.AddDays(-10).ToString("yyyy-MM-dd", inv)) };
            foreach (var (lot, kg) in lots)
            {
                newBatch.Add(new("lots", lot.ToString(inv)));
                newBatch.Add(new($"kg[{lot}]", kg.ToString(inv)));
            }
            var createdBatch = await staff.PostAsync("/OrgWorkspace/NewBatch", newBatch, "/OrgWorkspace/NewBatch");
            var batchId = int.Parse(Regex.Match(createdBatch.Headers.Location?.ToString() ?? "", @"/OrgWorkspace/Batch/(\d+)").Groups[1].Value is { Length: > 0 } bs ? bs
                : throw new InvalidOperationException("batch not created: " + Errors(await createdBatch.Content.ReadAsStringAsync())), inv);
            await SheetAsync(staff, quality, "batch", batchId);
            Expect(await staff.PostFlashAsync($"/OrgWorkspace/RequestFinalize/{batchId}", new(), $"/OrgWorkspace/Batch/{batchId}"), "Finalizing requested", "finalize request");
            Expect(await quality.PostFlashAsync($"/OrgWorkspace/Finalize/{batchId}", new(), $"/OrgWorkspace/Batch/{batchId}"), "Batch finalized", "finalize");
            var qr = await Db(db => db.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == batchId).Select(f => f.FinalHoneyProductionQR).FirstAsync());
            _log($"batch #{batchId} finalized");

            // ---- 6. some stock at the shop: shipment, receipt, storage (CCP 10) approved by the shop's QA officer
            var shopStaff = await As(User("Staff", "shop"));
            var shopQuality = await As(User("QualityApprover", "shop"));
            Expect(await staff.PostFlashAsync("/Shipments/Create", new() { new("batchId", batchId.ToString(inv)), new("toOrgId", _orgIds["shop"].ToString(inv)), new("quantity", _history.GetProperty("shipKg").GetString()!) },
                "/Shipments/Index"), "Shipment created", "shipment");
            var shipment = await Db(db => db.Shipments.Where(s => s.FinalHoneyProductionId == batchId).Select(s => s.ShipmentId).FirstAsync());
            Expect(await shopStaff.PostFlashAsync("/Shipments/Decide", new() { new("id", shipment.ToString(inv)), new("confirm", "true") }, "/Shipments/Index"), "Receipt confirmed", "receipt");
            var storage = new List<KeyValuePair<string, string>> { new("batchId", batchId.ToString(inv)) };
            foreach (var p in await ParametersAsync("CCP 10"))
                storage.Add(new($"values[{p.ParameterId}]", Value("CCP 10", p)));
            Expect(await shopStaff.PostFlashAsync("/Shipments/Storage", storage, $"/Shipments/Storage?batchId={batchId}"), "saved", "storage");
            Expect(await shopQuality.PostFlashAsync("/Shipments/ApproveStorage", new() { new("batchId", batchId.ToString(inv)) }, $"/Shipments/Storage?batchId={batchId}"), "approved", "storage approval");
            _log($"{_history.GetProperty("shipKg").GetString()} kg at the shop");

            // ---- 7. the sets: per tester a consumer account with the honey saved, seal labels, plastic seals and a container of the
            // association, and a beekeeper to register live
            var setInfos = new List<TestSetSetInfo>();
            var setSpecs = _cast.GetProperty("sets").EnumerateArray().Take(sets).ToList();
            var association = _orgIds["association"];
            for (var s = 1; s <= setSpecs.Count; s++)
            {
                var spec = setSpecs[s - 1];
                var consumer = spec.GetProperty("consumer");
                var consumerUser = consumer.GetProperty("user").GetString()!;
                string labelsText, plasticText, containerCode;
                using (var scope = _services.CreateScope())
                {
                    var consumers = Svc<ConsumerService>(scope);
                    var (user, _, errors) = await consumers.CreateAsync(new ConsumerSignUp
                    {
                        UserName = consumerUser, Password = PasswordOf(consumerUser), ConfirmPassword = PasswordOf(consumerUser),
                        Language = "tr", IsAdult = true, AcceptNotice = true
                    }, null);
                    if (user == null)
                        throw new InvalidOperationException($"consumer {consumerUser}: " + string.Join(" ", errors));
                    await MarkAccountAsync(user.Id, TestSetAccountKind.Consumer, s);
                    if (!await consumers.SaveAsync(user.Id, qr))
                        throw new InvalidOperationException($"consumer {consumerUser}: the honey could not be saved");

                    var pool = Svc<SealPoolService>(scope);
                    var labels = await pool.IssueLabelsAsync(association, 12, operatorId, $"test set {s} (D-112)");
                    labelsText = $"{labels.FirstCode} - {labels.LastCode}";
                    var (plastic, plasticError) = await pool.AddPlasticRangeAsync(association, $"SAB-S{s:D2}-", "01", "04", operatorId, $"test set {s} (D-112)");
                    if (plastic == null)
                        throw new InvalidOperationException($"set {s} plastic seals: {plasticError}");
                    plasticText = $"{plastic.FirstCode} - {plastic.LastCode}";
                    var (container, containerError) = await pool.RegisterContainerAsync(association, ContainerKind.Drum, $"TEST set {s}", 300m, 18m, operatorId);
                    if (container == null)
                        throw new InvalidOperationException($"set {s} container: {containerError}");
                    containerCode = container.Code;
                }
                var b = spec.GetProperty("beekeeper");
                setInfos.Add(new TestSetSetInfo(s, consumerUser, consumer.GetProperty("name").GetString()!,
                    $"{b.GetProperty("first").GetString()} {b.GetProperty("last").GetString()}", b.GetProperty("birth").GetString()!,
                    RegistryNumber(s, 1), PhonePage.Pretty(TestPhones.Number(s, 1)),
                    $"İstanbul / Şile / {b.GetProperty("locality").GetString()} ({b.GetProperty("lat").GetString()}, {b.GetProperty("lng").GetString()}), {b.GetProperty("hives").GetString()} hives",
                    labelsText, plasticText, containerCode));
            }
            _log($"{setInfos.Count} set(s): consumer, seal labels, plastic seals and a container each");

            // ---- the logins, in the order of the journey
            var orgNames = _cast.GetProperty("orgs").EnumerateArray().ToDictionary(o => o.GetProperty("key").GetString()!, OrgName);
            orgNames["platform"] = "Platform administration";
            var journey = new[]
            {
                opUser, User("RegistrationOfficer", "directorate"), portalUser, User("SampleAcceptanceOfficer", "association"), User("QualityApprover", "association"),
                User("Analyst", "lab"), User("LabManager", "lab"), User("Staff", "association"), User("Staff", "shop"), User("QualityApprover", "shop")
            };
            var logins = new List<TestSetLogin>();
            int Order(string user) => Array.IndexOf(journey, user) is var i and >= 0 ? i + 1 : journey.Length + 2 + _people.FindIndex(p => p.GetProperty("user").GetString() == user);
            foreach (var p in _people)
            {
                var user = p.GetProperty("user").GetString()!;
                logins.Add(new TestSetLogin(user, PasswordOf(user), p.GetProperty("name").GetString()!, orgNames[p.GetProperty("org").GetString()!],
                    user == opUser ? "Platform administrator" : RoleName(p.GetProperty("role").GetString()!), Order(user), null));
            }
            logins.Add(new TestSetLogin(portalUser, PasswordOf(portalUser), bee.GetProperty("name").GetString()!, "registered by " + orgNames["directorate"], "Beekeeper (portal)", Order(portalUser), null));
            foreach (var info in setInfos)
                logins.Add(new TestSetLogin(info.ConsumerUser, PasswordOf(info.ConsumerUser), info.ConsumerName, "-", "Consumer", journey.Length + 1, info.No));

            var verified = await operatorClient.PostFlashAsync("/Audit/Verify", new(), "/Audit/Index");
            _log($"audit chain: {verified}");
            return new TestSetResult(logins.OrderBy(l => l.Order).ThenBy(l => l.Set).ToList(), setInfos, batchId, batchTitle, qr, lots.Select(l => l.Lot).ToList(), companyLabels);
        }

        public static string RoleName(string role) => role switch
        {
            OrgRoles.OrgAdmin => "Organization administrator", OrgRoles.Staff => "Staff", OrgRoles.QualityApprover => "Quality Assurance Officer",
            OrgRoles.SampleAcceptanceOfficer => "Reception (sample acceptance) officer", OrgRoles.Analyst => "Analyst", OrgRoles.LabManager => "Laboratory manager",
            OrgRoles.FieldInspector => "Field inspector", OrgRoles.RegistrationOfficer => "Beekeeper registration officer", OrgRoles.ComplianceOfficer => "Compliance Officer",
            _ => role
        };

        // ======================================================================== helpers (as tools/live-like)

        private string User(string role, string org) => _people.First(p => p.GetProperty("role").GetString() == role && p.GetProperty("org").GetString() == org).GetProperty("user").GetString()!;

        private async Task<Client> SignInAsync(string user)
        {
            var (c, response) = await Client.TrySignInAsync(this, user, PasswordOf(user));
            if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
                throw new InvalidOperationException($"sign-in of {user} failed ({(int)response.StatusCode})");
            return c;
        }

        private async Task<Client> As(string user) => _clients.TryGetValue(user, out var c) ? c : _clients[user] = await SignInAsync(user);

        private async Task FirstSignInAsync(string user)
        {
            var sms = await LastSmsAsync(await UserIdAsync(user));
            foreach (Match candidate in Regex.Matches(sms, @"(?<![A-Za-z0-9-])[ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789]{10}(?![A-Za-z0-9-])"))
            {
                var (client, response) = await Client.TrySignInAsync(this, user, candidate.Value);
                if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
                    continue;
                var password = PasswordOf(user);
                var changed = await client.PostAsync("/Account/FirstPassword", new() { new("password", password), new("confirmPassword", password) }, "/Account/FirstPassword");
                if (changed.StatusCode != HttpStatusCode.Redirect)
                    throw new InvalidOperationException($"{user}: the generated password was refused at the first sign-in: " + Errors(await changed.Content.ReadAsStringAsync()));
                return;
            }
            throw new InvalidOperationException($"{user}: no temporary password in the sign-in SMS");
        }

        private async Task<string> PortalAccountAsync(Client registrar, int registrationId, string beekeeperId)
        {
            var userId = await Db(db => db.BeekeeperAccounts.Where(x => x.BeekeeperId == beekeeperId).Select(x => x.UserId).FirstOrDefaultAsync());
            if (userId == null)
            {
                Expect(await registrar.PostFlashAsync($"/Registration/IssueSignIn/{registrationId}", new(), $"/Registration/Details/{registrationId}"), "sign-in details were sent", "portal account");
                userId = await Db(db => db.BeekeeperAccounts.Where(x => x.BeekeeperId == beekeeperId).Select(x => x.UserId).FirstAsync());
            }
            await MarkAccountAsync(userId, TestSetAccountKind.Beekeeper);
            string userName;
            using (var scope = _services.CreateScope())
                userName = (await Svc<UserManager<ApplicationUser>>(scope).FindByIdAsync(userId))!.UserName!;
            await FirstSignInAsync(userName);
            return userName;
        }

        // the newest message of a subject in the test inbox (an SMS to a test number keeps its full text)
        private Task<string> LastSmsAsync(string subjectKey) =>
            Db(db => db.SmsMessages.Where(s => s.SubjectKey == subjectKey && s.Provider == TestPhones.Provider).OrderByDescending(s => s.SmsId).Select(s => s.BodyStored).FirstAsync());

        private Task<List<CriticalControlPointParameter>> ParametersAsync(string code) => Db(db => db.CriticalControlPointParameters.AsNoTracking()
            .Where(p => p.CCPId == db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).First() && p.DataType != CcpDataType.File)
            .OrderBy(p => p.ParameterId).ToListAsync());

        // the suggested values (docs/partner-test/values.json), all within the limits
        private string Value(string code, CriticalControlPointParameter p)
        {
            var key = $"{code}|{p.ParameterName!.Trim()}";
            if (_history.GetProperty("values").TryGetProperty(key, out var own))
                return own.GetString()!;
            if (p.DataType == CcpDataType.Number)
                return _values.GetProperty("numbers").GetProperty(key).GetString()!;
            if (_values.GetProperty("texts").TryGetProperty(key, out var t))
                return t.GetString()!.Replace("{country}", "Türkiye").Replace("{botanical}", _history.GetProperty("botanical").GetString()!);
            if (code == "CCP 0")
            {
                var ccp0 = _values.GetProperty("ccp0");
                if (p.DataType == CcpDataType.YesNo)
                    return ccp0.GetProperty("yesNo").GetString()!;
                foreach (var e in ccp0.EnumerateObject())
                    if (e.Name is not ("yesNo" or "text") && p.ParameterName.StartsWith(e.Name, StringComparison.Ordinal))
                        return e.Value.GetString()!;
                return ccp0.GetProperty("text").GetString()!;
            }
            return p.DataType == CcpDataType.YesNo ? _values.GetProperty("otherYesNo").GetString()! : "";
        }

        private Task<int> CcpIdAsync(string code) => Db(db => db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).FirstAsync());

        private async Task SingleCcpAsync(Client enter, Client approve, string kind, int owner, string code)
        {
            var ccp = await CcpIdAsync(code);
            var page = kind == "raw" ? "RawHoneyProductionManageCCP" : "FinalHoneyProductionManageCCP";
            var from = $"/CCPMonitoring/{page}?{(kind == "raw" ? "rawHoneyProductionId" : "finalHoneyProductionId")}={owner}&ccpId={ccp}";
            var form = new List<KeyValuePair<string, string>> { new("CCPId", ccp.ToString(Inv)), new(kind == "raw" ? "RawHoneyProductionId" : "FinalHoneyProductionId", owner.ToString(Inv)) };
            var parameters = await ParametersAsync(code);
            for (var i = 0; i < parameters.Count; i++)
            {
                form.Add(new($"Groups[0].Parameters[{i}].ParameterId", parameters[i].ParameterId.ToString(Inv)));
                var v = Value(code, parameters[i]);
                if (v.Length > 0)
                    form.Add(new(parameters[i].DataType == CcpDataType.Number ? $"Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Groups[0].Parameters[{i}].MeasuredTextValue", v));
            }
            Expect(await enter.PostFlashAsync($"/CCPMonitoring/{page}", form, from), "values saved", $"{code} of {kind} {owner}");
            await ApproveSingleAsync(approve, kind, owner, code);
        }

        private async Task ApproveSingleAsync(Client approve, string kind, int owner, string code)
        {
            var ccp = await CcpIdAsync(code);
            var from = kind == "raw" ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={owner}&ccpId={ccp}"
                : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={owner}&ccpId={ccp}";
            Expect(await approve.PostFlashAsync("/CCPMonitoring/Approve", new() { new("ownerKind", kind), new("ownerId", owner.ToString(Inv)), new("ccpId", ccp.ToString(Inv)), new("confirmOutOfLimit", "true") }, from),
                "approved", $"{code} approval of {kind} {owner}");
        }

        // every CCP of the control sheet entered at once, then the sheet approved in one action by another person
        private async Task SheetAsync(Client enter, Client approve, string kind, int owner)
        {
            var url = $"/CCPMonitoring/Sheet?kind={kind}&id={owner}";
            var ccps = await Db(db => kind == "raw"
                ? db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == owner && !ControlSheets.OwnPageLotCcps.Contains(a.CCP!.Code) && a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable).Select(a => new { Id = a.RawHoneyCCPAssignmentCCP, a.CCP!.Code }).ToListAsync()
                : db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == owner && a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable).Select(a => new { Id = a.FinalHoneyCCPAssignmentCCP, a.CCP!.Code }).ToListAsync());
            var form = new List<KeyValuePair<string, string>> { new("Kind", kind), new("Id", owner.ToString(Inv)) };
            foreach (var c in ccps.OrderBy(c => c.Id))
            {
                form.Add(new("Ccps.Index", c.Id.ToString(Inv)));
                form.Add(new($"Ccps[{c.Id}].CCPId", c.Id.ToString(Inv)));
                var parameters = await ParametersAsync(c.Code);
                for (var i = 0; i < parameters.Count; i++)
                {
                    form.Add(new($"Ccps[{c.Id}].Groups[0].Parameters[{i}].ParameterId", parameters[i].ParameterId.ToString(Inv)));
                    var v = Value(c.Code, parameters[i]);
                    if (v.Length > 0)
                        form.Add(new(parameters[i].DataType == CcpDataType.Number ? $"Ccps[{c.Id}].Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Ccps[{c.Id}].Groups[0].Parameters[{i}].MeasuredTextValue", v));
                }
            }
            var saved = await enter.PostAsync("/CCPMonitoring/Sheet", form, url);
            if (saved.StatusCode != HttpStatusCode.Redirect)
                throw new InvalidOperationException($"control sheet of {kind} {owner}: " + Errors(await saved.Content.ReadAsStringAsync()));
            Expect(await approve.PostFlashAsync("/CCPMonitoring/ApproveSheet", new() { new("kind", kind), new("id", owner.ToString(Inv)), new("confirmOutOfLimit", "true") }, url),
                "Approved:", $"control sheet approval of {kind} {owner}");
        }

        private static void Expect(string actual, string expected, string step)
        {
            if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{step}: expected \"{expected}\", got \"{actual}\"");
        }

        private static List<KeyValuePair<string, string>> With(List<KeyValuePair<string, string>> form, params (string Key, string Value)[] more) =>
            form.Concat(more.Select(m => new KeyValuePair<string, string>(m.Key, m.Value))).ToList();

        private static string CodeIn(string sms) => Regex.Match(sms, @"\b\d{6}\b").Value;

        // the text of the error boxes of a page (the first 600 characters of each, tags removed)
        private static string Errors(string html) =>
            string.Join(" | ", Regex.Matches(WebUtility.HtmlDecode(html), "(?:class=\"[^\"]*(?:text-danger|alert-danger|validation-summary-errors)[^\"]*\"|id=\"[a-z-]*errors\")[^>]*>(.{0,600})", RegexOptions.Singleline)
                .Select(x => Regex.Replace(Regex.Replace(x.Groups[1].Value, "<[^>]+>", " "), "\\s+", " ").Trim()).Where(x => x.Length > 0).Distinct().Take(6));

        // a one-page PDF with a few lines (Latin letters only: the standard Helvetica font)
        public static byte[] Pdf(string title, params string[] lines)
        {
            static string Plain(string s)
            {
                var d = s.Replace('ı', 'i').Replace('İ', 'I').Replace('ş', 's').Replace('Ş', 'S').Replace('ğ', 'g').Replace('Ğ', 'G').Normalize(NormalizationForm.FormD);
                var b = new StringBuilder();
                foreach (var ch in d)
                    if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && ch < 128)
                        b.Append(ch is '(' or ')' or '\\' ? "\\" + ch : ch.ToString());
                return b.ToString();
            }
            var content = new StringBuilder();
            content.Append("BT /F1 16 Tf 56 780 Td (").Append(Plain(title)).Append(") Tj ET\n");
            var y = 750;
            foreach (var line in lines.Append(DateTime.Today.ToString("dd.MM.yyyy", Inv)))
            {
                content.Append("BT /F1 11 Tf 56 ").Append(y.ToString(Inv)).Append(" Td (").Append(Plain(line)).Append(") Tj ET\n");
                y -= 18;
            }
            var stream = content.ToString();
            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream"
            };
            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();
            for (var i = 0; i < objects.Length; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var o in offsets)
                pdf.Append(o.ToString("D10", Inv)).Append(" 00000 n \n");
            pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        // ======================================================================== one signed-in person in the in-memory app

        // own cookies, anti-forgery tokens from the pages, flash messages read back (English texts)
        private sealed class Client
        {
            private readonly HttpClient _http;

            private Client(HttpClient http)
            {
                _http = http;
                _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
            }

            public static Client Anonymous(TestSetCreator owner) =>
                new(new HttpClient(new CookieHandler(owner._handler())) { BaseAddress = new Uri(BaseAddress) });

            public static async Task<(Client Client, HttpResponseMessage Response)> TrySignInAsync(TestSetCreator owner, string user, string password)
            {
                var c = Anonymous(owner);
                var response = await c.PostAsync("/Account/Login", new() { new("UserName", user), new("Password", password) }, "/Account/Login");
                return (c, response);
            }

            public async Task<string> HtmlAsync(string url) => await (await _http.GetAsync(url)).Content.ReadAsStringAsync();

            private async Task<string> TokenAsync(string from)
            {
                var html = await HtmlAsync(from);
                var m = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
                if (!m.Success)
                    m = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
                return m.Success ? m.Groups[1].Value : throw new InvalidOperationException("no anti-forgery token on " + from);
            }

            public async Task<HttpResponseMessage> PostAsync(string url, List<KeyValuePair<string, string>> form, string tokenFrom)
            {
                var pairs = form.ToList();
                pairs.Add(new("__RequestVerificationToken", await TokenAsync(tokenFrom)));
                return await _http.PostAsync(url, new FormUrlEncodedContent(pairs));
            }

            public async Task<HttpResponseMessage> PostMultipartAsync(string url, List<KeyValuePair<string, string>> form, (string Name, string FileName, byte[] Content) file, string tokenFrom)
            {
                var content = new MultipartFormDataContent();
                foreach (var (k, v) in form)
                    content.Add(new StringContent(v), k);
                content.Add(new StringContent(await TokenAsync(tokenFrom)), "__RequestVerificationToken");
                var bytes = new ByteArrayContent(file.Content);
                bytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(bytes, file.Name, file.FileName);
                return await _http.PostAsync(url, content);
            }

            public async Task<string> FlashAfterAsync(HttpResponseMessage response)
            {
                if (response.StatusCode != HttpStatusCode.Redirect)
                    return $"(no redirect: {(int)response.StatusCode})";
                var html = await HtmlAsync(response.Headers.Location!.ToString());
                var m = Regex.Match(html, "id=\"flash-(?:error|success)\"[^>]*>(.*?)<button", RegexOptions.Singleline);
                if (!m.Success)
                    m = Regex.Match(html, "text: \"(.*?)\",", RegexOptions.Singleline);
                return m.Success ? WebUtility.HtmlDecode(Regex.Replace(m.Groups[1].Value, "<[^>]+>", "")).Trim() : "";
            }

            public async Task<string> PostFlashAsync(string url, List<KeyValuePair<string, string>> form, string tokenFrom) =>
                await FlashAfterAsync(await PostAsync(url, form, tokenFrom));
        }

        // the cookies of one person (the in-memory server has no cookie handling of its own); never follows redirects
        private sealed class CookieHandler : DelegatingHandler
        {
            private readonly CookieContainer _cookies = new();

            public CookieHandler(HttpMessageHandler inner) : base(inner) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var uri = request.RequestUri!;
                var header = _cookies.GetCookieHeader(uri);
                if (header.Length > 0)
                    request.Headers.Add("Cookie", header);
                var response = await base.SendAsync(request, cancellationToken);
                if (response.Headers.TryGetValues("Set-Cookie", out var values))
                    foreach (var v in values)
                        _cookies.SetCookies(uri, v);
                return response;
            }
        }
    }
}
