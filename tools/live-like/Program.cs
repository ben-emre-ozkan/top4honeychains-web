extern alias web;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.OrgModel;

// D-88: builds the live-like cast (cast.json) and its history in the LOCAL database through the application, hosted in-process
// with the settings of run-demo.ps1. Only the platform operator's account is made with the app's own user services (the first
// administrator cannot be made through a page); the operator then creates the organizations and members on the app's pages, every
// member signs in with the details from the SMS on the phone page and sets the practice password, and the history (two deliveries
// of the registered beekeeper, the lab, the control sheets, one finalized batch, a shipment to the shop) goes through the pages as
// the person who does it. Writes the logins file to Downloads. Never prints a password.
//   dotnet run --project tools/live-like            (stop run-demo.ps1 first)
const string Server = @"(localdb)\MSSQLLocalDB";
const string Database = "TOP4HoneyChainsDb";
var inv = CultureInfo.InvariantCulture;
var watch = Stopwatch.StartNew();

var repo = FindRepo();
var webDir = Path.Combine(repo, "TOP4HONEYCHAINS", "TOP4HoneyChains.Web");
using var castJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "tools", "live-like", "cast.json"), Encoding.UTF8));
var cast = castJson.RootElement;
var values = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "docs", "partner-test", "values.json"), Encoding.UTF8)).RootElement;
var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
var loginsFile = Path.Combine(downloads, "TOP4-local-logins.txt");
var password = PracticePassword(downloads) ?? throw new InvalidOperationException("No practice password found in Downloads (TOP4-local-logins.txt or TOP4-presenter-logins.txt).");
var connectionString = $"Server={Server};Database={Database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

// ---- guards: the local copy only, the app stopped, no cast yet
await using (var c = new SqlConnection(connectionString))
{
    await c.OpenAsync();
    await using var cmd = new SqlCommand("SELECT CAST(SERVERPROPERTY('IsLocalDB') AS int), DB_NAME(), (SELECT COUNT(*) FROM dbo.AspNetUsers WHERE UserName = @u)", c);
    cmd.Parameters.AddWithValue("@u", cast.GetProperty("operator").GetProperty("user").GetString());
    await using var r = await cmd.ExecuteReaderAsync();
    await r.ReadAsync();
    if (r.IsDBNull(0) || r.GetInt32(0) != 1 || r.GetString(1) != Database)
        throw new InvalidOperationException("Runs only on the LocalDB database TOP4HoneyChainsDb.");
    if (r.GetInt32(2) > 0)
        throw new InvalidOperationException("The cast exists already. Run the cleanup script next to the backup first.");
}
if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(e => e.Port == 5255))
    throw new InvalidOperationException("Stop run-demo.ps1 first (port 5255 is in use).");

var phoneKey = PhonePageKey(webDir);
// the test host finds the web project through MvcTestingAppManifest.json in the current directory
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
await using var app = new LocalApp(webDir, connectionString, phoneKey);
_ = app.Server;
Log("app started in-process on the local copy");

T Svc<T>(IServiceScope s) where T : notnull => s.ServiceProvider.GetRequiredService<T>();
async Task<TResult> Db<TResult>(Func<TOP4HoneyChainsContext, Task<TResult>> work)
{
    using var scope = app.Services.CreateScope();
    return await work(Svc<TOP4HoneyChainsContext>(scope));
}

// ---- 1. the platform operator: the app's user services (password rules included), Administrator role, platform membership
var op = cast.GetProperty("operator");
var opUser = op.GetProperty("user").GetString()!;
using (var scope = app.Services.CreateScope())
{
    var users = Svc<UserManager<ApplicationUser>>(scope);
    var user = new ApplicationUser { UserName = opUser, PhoneNumber = E164(op.GetProperty("phone").GetString()!), PhoneNumberConfirmed = true, LockoutEnabled = true };
    var created = await users.CreateAsync(user, password);
    if (!created.Succeeded)
        throw new InvalidOperationException("operator: " + string.Join(" ", created.Errors.Select(e => e.Description)));
    await users.AddToRoleAsync(user, "Administrator");
    var db = Svc<TOP4HoneyChainsContext>(scope);
    var platform = await db.Organizations.Where(o => o.IsPlatform).Select(o => o.OrgId).FirstAsync();
    db.Memberships.Add(new Membership { UserId = user.Id, OrgId = platform, OrgRole = OrgRoles.OrgAdmin, Status = MembershipStatus.Active, GrantedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();
    await AuditChain.AppendAsync(db, new[] { new AuditRecord(DateTime.UtcNow, null, "local set-up (D-88)", platform, "Create", "AspNetUsers", user.Id, null,
        JsonSerializer.Serialize(new { user.UserName, Role = "Administrator" }), "LocalSetup", null) });
}
var operatorClient = await Client.SignInAsync(app, opUser, password);
Log($"platform operator {opUser}");

// ---- 2. organizations and members through the operator's pages; each member signs in with the SMS details and sets the password
var orgIds = new Dictionary<string, int>();
var people = cast.GetProperty("people").EnumerateArray().ToList();
foreach (var o in cast.GetProperty("orgs").EnumerateArray())
{
    var key = o.GetProperty("key").GetString()!;
    var name = o.GetProperty("name").GetString()!;
    var caps = o.GetProperty("capabilities").EnumerateArray().Select(x => x.GetString()!).ToList();
    var accreditation = o.TryGetProperty("accreditation", out var a) ? a.GetString() : null;
    // an accredited laboratory: the accreditation is entered when the capability is added, so the organization starts with
    // another capability that is removed again (all on the organization's page)
    var initial = accreditation != null ? new List<string> { Capabilities.Retail } : caps;
    var form = new List<KeyValuePair<string, string>>
    {
        new("DisplayName", name), new("LegalName", o.GetProperty("legal").GetString()!),
        new("Province", o.GetProperty("province").GetString()!), new("District", o.GetProperty("district").GetString()!)
    };
    form.AddRange(initial.Select(cap => new KeyValuePair<string, string>("capabilities", cap)));
    var created = await operatorClient.PostAsync("/Organizations/Create", form, "/Organizations/Create");
    var id = int.Parse(Regex.Match(created.Headers.Location?.ToString() ?? "", @"/Organizations/Details/(\d+)").Groups[1].Value is { Length: > 0 } s ? s
        : throw new InvalidOperationException($"{name}: not created: {await operatorClient.FlashAfterAsync(created)}"), inv);
    orgIds[key] = id;
    if (accreditation != null)
    {
        Expect(await operatorClient.PostFlashAsync($"/Organizations/AddCapability/{id}", new()
        {
            new("capability", Capabilities.Laboratory), new("isAccredited", "true"), new("accreditationNumber", accreditation),
            new("accreditationValidUntil", DateTime.Today.AddYears(3).ToString("yyyy-MM-dd", inv))
        }, $"/Organizations/Details/{id}"), "added", $"{name} laboratory");
        Expect(await operatorClient.PostFlashAsync($"/Organizations/RemoveCapability/{id}", new() { new("capability", Capabilities.Retail) }, $"/Organizations/Details/{id}"),
            "removed", $"{name} temporary capability");
    }
    var document = Pdf(accreditation != null ? "Akreditasyon sertifikasi" : "Faaliyet belgesi", name, o.GetProperty("legal").GetString()!,
        $"{o.GetProperty("district").GetString()} / {o.GetProperty("province").GetString()}");
    Expect(await operatorClient.FlashAfterAsync(await operatorClient.PostMultipartAsync("/Organizations/UploadDocument",
        new() { new("id", id.ToString(inv)), new("kind", accreditation != null ? "Accreditation" : "Licence") },
        ("file", accreditation != null ? "akreditasyon.pdf" : "faaliyet-belgesi.pdf", document), $"/Organizations/Details/{id}")), "uploaded", $"{name} document");

    // the Compliance Officer first: verification needs one
    foreach (var p in people.Where(p => p.GetProperty("org").GetString() == key).OrderBy(p => p.GetProperty("role").GetString() == OrgRoles.ComplianceOfficer ? 0 : 1))
    {
        var user = p.GetProperty("user").GetString()!;
        var added = await operatorClient.PostFlashAsync($"/Organizations/AddMember/{id}", new()
        {
            new("orgRole", p.GetProperty("role").GetString()!), new("newUserName", user), new("newPhone", p.GetProperty("phone").GetString()!)
        }, $"/Organizations/Details/{id}");
        Expect(added, "added as", $"{user} member");
        await FirstSignInAsync(user);
    }
    Expect(await operatorClient.PostFlashAsync("/Organizations/ChangeStatus", new() { new("id", id.ToString(inv)), new("toStatus", OrgStatus.Verified) }, $"/Organizations/Details/{id}"),
        "now Verified", $"{name} verification");
    Log($"organization {name}: {people.Count(p => p.GetProperty("org").GetString() == key)} member(s), verified");
}

// the chestnut harvest season (reference data, Set-up)
var history = cast.GetProperty("history");
var periodSpec = history.GetProperty("period");
await operatorClient.PostAsync("/RawHoneyProductionPeriods/Create", new()
{
    new("RawHoneyProductionPeriodTitle", periodSpec.GetProperty("title").GetString()!), new("RawHoneyProductionPeriodStartDate", periodSpec.GetProperty("start").GetString()!),
    new("RawHoneyProductionPeriodEndDate", periodSpec.GetProperty("end").GetString()!)
}, "/RawHoneyProductionPeriods/Create");
var period = await Db(db => db.RawHoneyProductionPeriods.Where(p => p.RawHoneyProductionPeriodTitle == periodSpec.GetProperty("title").GetString()).Select(p => p.RawHoneyProductionPeriodId).FirstAsync());

var clients = new Dictionary<string, Client>();
async Task<Client> As(string user) => clients.TryGetValue(user, out var c) ? c : clients[user] = await Client.SignInAsync(app, user, password);

// ---- 3. the registered beekeeper: registration by the officer (SMS code from the beekeeper's phone), association, portal account
var bee = cast.GetProperty("beekeepers").EnumerateArray().First(b => b.GetProperty("key").GetString() == "registered");
var officer = await As(User("RegistrationOfficer", "directorate"));
var found = await officer.PostAsync("/Registration/Find", new() { new("number", bee.GetProperty("registry").GetString()!) }, "/Registration/Index");
if (!(found.Headers.Location?.ToString() ?? "").StartsWith("/Registration/New", StringComparison.Ordinal))
    throw new InvalidOperationException("registration did not start: " + await officer.FlashAfterAsync(found));
var beeDistrict = await Db(db => db.Districts.Where(d => d.ProvinceCode == "52" && d.Name == bee.GetProperty("district").GetString()).Select(d => d.DistrictId).FirstAsync());
var registration = new List<KeyValuePair<string, string>>
{
    new("RegistryNumber", bee.GetProperty("registry").GetString()!), new("FirstName", bee.GetProperty("first").GetString()!), new("LastName", bee.GetProperty("last").GetString()!),
    new("BirthDate", bee.GetProperty("birth").GetString()!), new("Phone", bee.GetProperty("phone").GetString()!), new("Language", "tr"),
    new("ApiaryLatitude", bee.GetProperty("lat").GetString()!), new("ApiaryLongitude", bee.GetProperty("lng").GetString()!), new("ApiaryProvince", "52"),
    new("ApiaryDistrictId", beeDistrict.ToString(inv)), new("ApiaryLocality", bee.GetProperty("locality").GetString()!), new("ApiaryHives", bee.GetProperty("hives").GetString()!),
    new("PublicName", "true"), new("PortalNotifications", "true"), new("Attest", "true"), new("IssueSignIn", "true"),
    new("AssociationOrgId", orgIds["association"].ToString(inv))
};
var sentHtml = WebUtility.HtmlDecode(await (await officer.PostAsync("/Registration/Submit", With(registration, ("op", "send")), "/Registration/New")).Content.ReadAsStringAsync());
var draft = Regex.Match(sentHtml, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
if (draft.Length == 0)
    throw new InvalidOperationException("registration code not sent: " + Errors(sentHtml));
var confirmed = await officer.PostAsync("/Registration/Submit", With(registration, ("op", "confirm"), ("DraftKey", draft), ("Code", CodeIn(await LastSmsAsync(draft)))), "/Registration/New");
Expect(await officer.FlashAfterAsync(confirmed), "registered and verified", "registration");
var regId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last(), inv);
var registered = await Db(db => db.BeekeeperRegistrations.Where(r => r.RegistrationId == regId)
    .Select(r => new { r.BeekeeperId, r.Beekeeper!.BeekeeperBusinessNumber, Apiary = r.Beekeeper.Apiaries.Select(x => x.ApiaryId).FirstOrDefault() }).FirstAsync());
var portalUser = await PortalAccountAsync(officer, regId, registered.BeekeeperId);
Log($"beekeeper registered ({registered.BeekeeperBusinessNumber}), portal account {portalUser}");

// the beekeeper's second apiary: the officer finds the beekeeper again and adds it on the registration form (a new SMS code)
var second = bee.GetProperty("apiary2");
var again = await officer.PostAsync("/Registration/Find", new() { new("number", bee.GetProperty("registry").GetString()!) }, "/Registration/Index");
var reverifyUrl = again.Headers.Location?.ToString() ?? "";
if (!reverifyUrl.StartsWith("/Registration/Reverify", StringComparison.Ordinal))
    throw new InvalidOperationException("second apiary: the beekeeper was not found: " + await officer.FlashAfterAsync(again));
var secondDistrict = await Db(db => db.Districts.Where(d => d.ProvinceCode == "52" && d.Name == second.GetProperty("district").GetString()).Select(d => d.DistrictId).FirstAsync());
var update = registration.Where(p => !p.Key.StartsWith("Apiary", StringComparison.Ordinal) && p.Key != "IssueSignIn").ToList();
update.AddRange(new KeyValuePair<string, string>[]
{
    new("BeekeeperId", registered.BeekeeperId), new("ApiaryLatitude", second.GetProperty("lat").GetString()!), new("ApiaryLongitude", second.GetProperty("lng").GetString()!),
    new("ApiaryProvince", "52"), new("ApiaryDistrictId", secondDistrict.ToString(inv)), new("ApiaryLocality", second.GetProperty("locality").GetString()!),
    new("ApiaryHives", second.GetProperty("hives").GetString()!)
});
var updateSent = WebUtility.HtmlDecode(await (await officer.PostAsync("/Registration/Submit", With(update, ("op", "send")), reverifyUrl)).Content.ReadAsStringAsync());
if (!updateSent.Contains("id=\"registration-confirm\""))
    throw new InvalidOperationException("second apiary: code not sent: " + Errors(updateSent));
var updated = await officer.PostAsync("/Registration/Submit", With(update, ("op", "confirm"), ("Code", CodeIn(await LastSmsAsync(registered.BeekeeperId)))), reverifyUrl);
Expect(await officer.FlashAfterAsync(updated), "registered and verified", "second apiary");
var apiaries = await Db(db => db.Apiaries.Where(x => x.ApiaryBeekeeperId == registered.BeekeeperId).OrderBy(x => x.ApiaryId).Select(x => x.ApiaryId).ToListAsync());
Log($"second apiary added ({apiaries.Count} apiaries)");

// ---- 4. two deliveries, the lab, CCP 0-3 (lot control sheet) for each lot
var reception = await As(User("SampleAcceptanceOfficer", "plant"));
var staff = await As(User("Staff", "plant"));
var quality = await As(User("QualityApprover", "plant"));
var analyst = await As(User("Analyst", "lab"));
var labManager = await As(User("LabManager", "lab"));
var honeyType = await Db(db => db.HoneyTypes.Where(h => h.HoneyTypeTitle == history.GetProperty("honeyType").GetString()).Select(h => h.HoneyTypeId).FirstAsync());
var lots = new List<(int Lot, decimal Kg)>();
var sealNo = 24101;
var deliveryNo = 0;
foreach (var d in history.GetProperty("deliveries").EnumerateArray())
{
    deliveryNo++;
    var date = DateTime.Today.AddDays(-d.GetProperty("daysAgo").GetInt32());
    var form = new List<KeyValuePair<string, string>>
    {
        new("Number", registered.BeekeeperBusinessNumber!), new("ApiaryId", apiaries[d.GetProperty("apiary").GetInt32() - 1].ToString(inv)), new("HoneyTypeId", honeyType.ToString(inv)),
        new("PeriodId", period.ToString(inv)), new("DeliveryDate", date.ToString("yyyy-MM-dd", inv)), new("Province", "Ordu"),
        new("District", (d.GetProperty("apiary").GetInt32() == 1 ? bee : second).GetProperty("district").GetString()!), new("OriginCountry", "TR"), new("LabOrgId", orgIds["lab"].ToString(inv)),
        new("CounterLocation", "Numune arşivi, raf B2"), new("RetainUntil", DateTime.Today.AddYears(2).ToString("yyyy-MM-dd", inv))
    };
    var i = 0;
    decimal kg = 0;
    foreach (var c in d.GetProperty("containers").EnumerateArray())
    {
        var n = sealNo++;
        form.AddRange(new KeyValuePair<string, string>[]
        {
            new($"Containers[{i}].Kind", "Tin"), new($"Containers[{i}].NetKg", c.GetProperty("net").GetString()!), new($"Containers[{i}].GrossKg", c.GetProperty("gross").GetString()!),
            new($"Containers[{i}].ArrivedSealed", "true"), new($"Containers[{i}].OriginSeal", c.GetProperty("originSeal").GetString()!), new($"Containers[{i}].OriginSealIntact", "true"),
            new($"Containers[{i}].JarSeal", $"YBG-J-{n}"), new($"Containers[{i}].CounterSeal", $"YBG-K-{n}"), new($"Containers[{i}].CapIntact", "true"),
            new($"Containers[{i}].Reseal", $"YBG-P-{n}")
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
    var reportNumber = history.GetProperty("lab").GetProperty("reportPrefix").GetString() + (1180 + deliveryNo).ToString(inv);
    var results = new List<KeyValuePair<string, string>> { new("reportNumber", reportNumber), new("issuedOn", date.AddDays(4).ToString("yyyy-MM-dd", inv)) };
    foreach (var p in await ParametersAsync("CCP 1"))
        results.Add(new($"values[{p.ParameterId}]", Value("CCP 1", p)));
    var labName = cast.GetProperty("orgs").EnumerateArray().First(o => o.GetProperty("key").GetString() == "lab");
    Expect(await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{composite}", results,
        ("reportFile", $"{reportNumber}.pdf", Pdf("Analiz raporu " + reportNumber, labName.GetProperty("name").GetString()!,
            $"Akreditasyon no. {labName.GetProperty("accreditation").GetString()}", $"Bilesik numune, lot {lot}", "Bal analizi: nem, HMF, diastaz, prolin, sekerler, iletkenlik")),
        $"/LabReports/Edit/{composite}")), "saved", $"lot {lot} lab results");
    Expect(await analyst.PostFlashAsync($"/LabReports/Submit/{composite}", new(), $"/LabReports/Edit/{composite}"), "submitted", $"lot {lot} lab submit");
    Expect(await labManager.PostFlashAsync($"/LabReports/Decide/{composite}", new() { new("approve", "true") }, $"/LabReports/Edit/{composite}"), "approved", $"lot {lot} lab approval");
    await ApproveSingleAsync(quality, "raw", lot, "CCP 1");

    await SheetAsync(staff, quality, "raw", lot);
    lots.Add((lot, kg));
    Log($"delivery {deliveryNo}: lot {lot}, {kg.ToString("0.##", inv)} kg, lab report {reportNumber}, CCP 0-3 approved");
}

// ---- 5. the batch: New batch by the plant, the batch control sheet, finalizing by two people
var newBatch = new List<KeyValuePair<string, string>> { new("title", history.GetProperty("batchTitle").GetString()!), new("date", DateTime.Today.AddDays(-10).ToString("yyyy-MM-dd", inv)) };
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
Log($"batch #{batchId} finalized");

// ---- 6. some stock at the shop: shipment, receipt, storage (CCP 10) approved by the shop's QA officer
var shopStaff = await As(User("Staff", "shop"));
var shopQuality = await As(User("QualityApprover", "shop"));
Expect(await staff.PostFlashAsync("/Shipments/Create", new() { new("batchId", batchId.ToString(inv)), new("toOrgId", orgIds["shop"].ToString(inv)), new("quantity", history.GetProperty("shipKg").GetString()!) },
    "/Shipments/Index"), "Shipment created", "shipment");
var shipment = await Db(db => db.Shipments.Where(s => s.FinalHoneyProductionId == batchId).Select(s => s.ShipmentId).FirstAsync());
Expect(await shopStaff.PostFlashAsync("/Shipments/Decide", new() { new("id", shipment.ToString(inv)), new("confirm", "true") }, "/Shipments/Index"), "Receipt confirmed", "receipt");
var storage = new List<KeyValuePair<string, string>> { new("batchId", batchId.ToString(inv)) };
foreach (var p in await ParametersAsync("CCP 10"))
    storage.Add(new($"values[{p.ParameterId}]", Value("CCP 10", p)));
Expect(await shopStaff.PostFlashAsync("/Shipments/Storage", storage, $"/Shipments/Storage?batchId={batchId}"), "saved", "storage");
Expect(await shopQuality.PostFlashAsync("/Shipments/ApproveStorage", new() { new("batchId", batchId.ToString(inv)) }, $"/Shipments/Storage?batchId={batchId}"), "approved", "storage approval");
Log($"{history.GetProperty("shipKg").GetString()} kg at {cast.GetProperty("orgs").EnumerateArray().First(o => o.GetProperty("key").GetString() == "shop").GetProperty("name").GetString()}");

// ---- 7. the consumer: sign-up page, then the honey saved to My honeys
var consumerSpec = cast.GetProperty("consumer");
var consumerUser = consumerSpec.GetProperty("user").GetString()!;
var consumer = Client.Anonymous(app);
var signUpPage = await consumer.HtmlAsync("/Consumer/SignUp");
var formTime = Regex.Match(signUpPage, "name=\"formTime\"[^>]*value=\"([^\"]+)\"|value=\"([^\"]+)\"[^>]*name=\"formTime\"");
await Task.Delay(TimeSpan.FromSeconds(3.5));
var signedUp = await consumer.PostAsync("/Consumer/SignUp", new()
{
    new("UserName", consumerUser), new("Password", password), new("ConfirmPassword", password), new("Language", "tr"), new("IsAdult", "true"), new("AcceptNotice", "true"),
    new("website", ""), new("formTime", WebUtility.HtmlDecode(formTime.Groups[1].Success ? formTime.Groups[1].Value : formTime.Groups[2].Value))
}, "/Consumer/SignUp");
if (signedUp.StatusCode != HttpStatusCode.Redirect)
    throw new InvalidOperationException("consumer sign-up: " + Errors(await signedUp.Content.ReadAsStringAsync()));
var saved = await consumer.PostAsync("/Consumer/Save", new() { new("qr", qr.ToString()) }, "/Consumer");
if (saved.StatusCode != HttpStatusCode.Redirect)
    throw new InvalidOperationException($"consumer save: {(int)saved.StatusCode}");
Log($"consumer account {consumerUser}");

// ---- 8. the logins file (Downloads, outside the repository)
await WriteLoginsAsync();
var verified = await operatorClient.PostFlashAsync("/Audit/Verify", new(), "/Audit/Index");
Log($"audit chain: {verified}");
Log($"done in {watch.Elapsed.TotalSeconds:0} s; batch #{batchId}, Honey Story /honeystory/{qr.ToString().ToUpperInvariant()}");
return 0;

// ======================================================================== helpers

string User(string role, string org) => people.First(p => p.GetProperty("role").GetString() == role && p.GetProperty("org").GetString() == org).GetProperty("user").GetString()!;

async Task FirstSignInAsync(string user)
{
    string userId;
    using (var scope = app.Services.CreateScope())
        userId = (await Svc<UserManager<ApplicationUser>>(scope).FindByNameAsync(user))!.Id;
    var sms = await LastSmsAsync(userId);
    foreach (Match candidate in Regex.Matches(sms, @"(?<![A-Za-z0-9-])[ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789]{10}(?![A-Za-z0-9-])"))
    {
        var (client, response) = await Client.TrySignInAsync(app, user, candidate.Value);
        if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
            continue;
        var changed = await client.PostAsync("/Account/FirstPassword", new() { new("password", password), new("confirmPassword", password) }, "/Account/FirstPassword");
        if (changed.StatusCode != HttpStatusCode.Redirect)
            throw new InvalidOperationException($"{user}: the practice password was refused at the first sign-in: " + Errors(await changed.Content.ReadAsStringAsync()));
        return;
    }
    throw new InvalidOperationException($"{user}: no temporary password in the sign-in SMS");
}

async Task<string> PortalAccountAsync(Client registrar, int registrationId, string beekeeperId)
{
    var userId = await Db(db => db.BeekeeperAccounts.Where(x => x.BeekeeperId == beekeeperId).Select(x => x.UserId).FirstOrDefaultAsync());
    if (userId == null)
    {
        Expect(await registrar.PostFlashAsync($"/Registration/IssueSignIn/{registrationId}", new(), $"/Registration/Details/{registrationId}"), "sign-in details were sent", "portal account");
        userId = await Db(db => db.BeekeeperAccounts.Where(x => x.BeekeeperId == beekeeperId).Select(x => x.UserId).FirstAsync());
    }
    string userName;
    using (var scope = app.Services.CreateScope())
        userName = (await Svc<UserManager<ApplicationUser>>(scope).FindByIdAsync(userId))!.UserName!;
    await FirstSignInAsync(userName);
    return userName;
}

Task<string> LastSmsAsync(string subjectKey) =>
    Db(db => db.SmsMessages.Where(s => s.SubjectKey == subjectKey).OrderByDescending(s => s.SmsId).Select(s => s.BodyStored).FirstAsync());

Task<List<CriticalControlPointParameter>> ParametersAsync(string code) => Db(db => db.CriticalControlPointParameters.AsNoTracking()
    .Where(p => p.CCPId == db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).First() && p.DataType != CcpDataType.File)
    .OrderBy(p => p.ParameterId).ToListAsync());

// the suggested values (docs/partner-test/values.json), all within the limits
string Value(string code, CriticalControlPointParameter p)
{
    var key = $"{code}|{p.ParameterName!.Trim()}";
    if (history.GetProperty("values").TryGetProperty(key, out var own))
        return own.GetString()!;
    if (p.DataType == CcpDataType.Number)
        return values.GetProperty("numbers").GetProperty(key).GetString()!;
    if (values.GetProperty("texts").TryGetProperty(key, out var t))
        return t.GetString()!.Replace("{country}", "Türkiye").Replace("{botanical}", history.GetProperty("botanical").GetString()!);
    if (code == "CCP 0")
    {
        var ccp0 = values.GetProperty("ccp0");
        if (p.DataType == CcpDataType.YesNo)
            return ccp0.GetProperty("yesNo").GetString()!;
        foreach (var e in ccp0.EnumerateObject())
            if (e.Name is not ("yesNo" or "text") && p.ParameterName.StartsWith(e.Name, StringComparison.Ordinal))
                return e.Value.GetString()!;
        return ccp0.GetProperty("text").GetString()!;
    }
    return p.DataType == CcpDataType.YesNo ? values.GetProperty("otherYesNo").GetString()! : "";
}

async Task<int> CcpIdAsync(string code) => await Db(db => db.CriticalControlPoints.Where(x => x.Code == code).Select(x => x.Id).FirstAsync());

async Task SingleCcpAsync(Client enter, Client approve, string kind, int owner, string code)
{
    var ccp = await CcpIdAsync(code);
    var page = kind == "raw" ? "RawHoneyProductionManageCCP" : "FinalHoneyProductionManageCCP";
    var from = $"/CCPMonitoring/{page}?{(kind == "raw" ? "rawHoneyProductionId" : "finalHoneyProductionId")}={owner}&ccpId={ccp}";
    var form = new List<KeyValuePair<string, string>> { new("CCPId", ccp.ToString(inv)), new(kind == "raw" ? "RawHoneyProductionId" : "FinalHoneyProductionId", owner.ToString(inv)) };
    var parameters = await ParametersAsync(code);
    for (var i = 0; i < parameters.Count; i++)
    {
        form.Add(new($"Groups[0].Parameters[{i}].ParameterId", parameters[i].ParameterId.ToString(inv)));
        var v = Value(code, parameters[i]);
        if (v.Length > 0)
            form.Add(new(parameters[i].DataType == CcpDataType.Number ? $"Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Groups[0].Parameters[{i}].MeasuredTextValue", v));
    }
    Expect(await enter.PostFlashAsync($"/CCPMonitoring/{page}", form, from), "values saved", $"{code} of {kind} {owner}");
    await ApproveSingleAsync(approve, kind, owner, code);
}

async Task ApproveSingleAsync(Client approve, string kind, int owner, string code)
{
    var ccp = await CcpIdAsync(code);
    var from = kind == "raw" ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={owner}&ccpId={ccp}"
        : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={owner}&ccpId={ccp}";
    Expect(await approve.PostFlashAsync("/CCPMonitoring/Approve", new() { new("ownerKind", kind), new("ownerId", owner.ToString(inv)), new("ccpId", ccp.ToString(inv)), new("confirmOutOfLimit", "true") }, from),
        "approved", $"{code} approval of {kind} {owner}");
}

// D-88: every CCP of the control sheet entered at once, then the sheet approved in one action by another person
async Task SheetAsync(Client enter, Client approve, string kind, int owner)
{
    var url = $"/CCPMonitoring/Sheet?kind={kind}&id={owner}";
    var ccps = await Db(db => kind == "raw"
        ? db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == owner && !ControlSheets.OwnPageLotCcps.Contains(a.CCP!.Code)).Select(a => new { Id = a.RawHoneyCCPAssignmentCCP, a.CCP!.Code }).ToListAsync()
        : db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == owner).Select(a => new { Id = a.FinalHoneyCCPAssignmentCCP, a.CCP!.Code }).ToListAsync());
    var form = new List<KeyValuePair<string, string>> { new("Kind", kind), new("Id", owner.ToString(inv)) };
    foreach (var c in ccps.OrderBy(c => c.Id))
    {
        form.Add(new("Ccps.Index", c.Id.ToString(inv)));
        form.Add(new($"Ccps[{c.Id}].CCPId", c.Id.ToString(inv)));
        var parameters = await ParametersAsync(c.Code);
        for (var i = 0; i < parameters.Count; i++)
        {
            form.Add(new($"Ccps[{c.Id}].Groups[0].Parameters[{i}].ParameterId", parameters[i].ParameterId.ToString(inv)));
            var v = Value(c.Code, parameters[i]);
            if (v.Length > 0)
                form.Add(new(parameters[i].DataType == CcpDataType.Number ? $"Ccps[{c.Id}].Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Ccps[{c.Id}].Groups[0].Parameters[{i}].MeasuredTextValue", v));
        }
    }
    var saved = await enter.PostAsync("/CCPMonitoring/Sheet", form, url);
    if (saved.StatusCode != HttpStatusCode.Redirect)
        throw new InvalidOperationException($"control sheet of {kind} {owner}: " + Errors(await saved.Content.ReadAsStringAsync()));
    Expect(await approve.PostFlashAsync("/CCPMonitoring/ApproveSheet", new() { new("kind", kind), new("id", owner.ToString(inv)), new("confirmOutOfLimit", "true") }, url),
        "Approved:", $"control sheet approval of {kind} {owner}");
}

async Task WriteLoginsAsync()
{
    var orgNames = cast.GetProperty("orgs").EnumerateArray().ToDictionary(o => o.GetProperty("key").GetString()!, o => o.GetProperty("name").GetString()!);
    orgNames["platform"] = "Platform administration";
    string Link(string phone) => $"http://localhost:5255/phone/{phoneKey}/{Regex.Replace(phone, "[^0-9]", "")}";
    var roleNames = new Dictionary<string, string>
    {
        [OrgRoles.OrgAdmin] = "Organization administrator", [OrgRoles.Staff] = "Staff", [OrgRoles.QualityApprover] = "Quality Assurance Officer",
        [OrgRoles.SampleAcceptanceOfficer] = "Reception (sample acceptance) officer", [OrgRoles.Analyst] = "Analyst", [OrgRoles.LabManager] = "Laboratory manager",
        [OrgRoles.FieldInspector] = "Field inspector", [OrgRoles.RegistrationOfficer] = "Beekeeper registration officer", [OrgRoles.ComplianceOfficer] = "Compliance Officer"
    };
    var b = new StringBuilder();
    b.AppendLine("TOP4HoneyChains - local logins (live-like cast, D-88)");
    b.AppendLine("=====================================================");
    b.AppendLine();
    b.AppendLine("Address:   http://localhost:5255");
    b.AppendLine($"Password:  {password}   (the same for every account below; local practice password only)");
    b.AppendLine("Start:     powershell -ExecutionPolicy Bypass -File .\\run-demo.ps1   (from the repository root; add -Lan for phones on the same Wi-Fi)");
    b.AppendLine("SMS:       no real SMS is sent. Each number has its own phone page (last column). The links keep working after a restart;");
    b.AppendLine("           run-demo.ps1 -NewKey makes new ones. With -Lan, run-demo.ps1 prints the same links on the PC's network address.");
    b.AppendLine();
    b.AppendLine($"{"User name",-18} {"Person",-15} {"Role",-38} {"Organization",-42} {"Phone",-18} Phone page");
    b.AppendLine($"{new string('-', 18)} {new string('-', 15)} {new string('-', 38)} {new string('-', 42)} {new string('-', 18)} {new string('-', 10)}");
    foreach (var p in people)
    {
        var role = p.GetProperty("user").GetString() == opUser ? "Platform administrator (operator)" : roleNames[p.GetProperty("role").GetString()!];
        b.AppendLine($"{p.GetProperty("user").GetString(),-18} {p.GetProperty("name").GetString(),-15} {role,-38} {orgNames[p.GetProperty("org").GetString()!],-42} {p.GetProperty("phone").GetString(),-18} {Link(p.GetProperty("phone").GetString()!)}");
    }
    b.AppendLine($"{portalUser,-18} {bee.GetProperty("name").GetString(),-15} {"Beekeeper (portal)",-38} {"registered by " + orgNames["directorate"],-42} {bee.GetProperty("phone").GetString(),-18} {Link(bee.GetProperty("phone").GetString()!)}");
    b.AppendLine($"{consumerUser,-18} {consumerSpec.GetProperty("name").GetString(),-15} {"Consumer account (D-87)",-38} {"-",-42} {"-",-18} -");
    b.AppendLine();
    var next = cast.GetProperty("beekeepers").EnumerateArray().First(x => x.GetProperty("key").GetString() == "new");
    b.AppendLine("Not registered yet (for a live registration):");
    b.AppendLine($"  {next.GetProperty("name").GetString()}, born {next.GetProperty("birth").GetString()}, registry number {next.GetProperty("registry").GetString()}, phone {next.GetProperty("phone").GetString()},");
    b.AppendLine($"  apiary Ordu / {next.GetProperty("district").GetString()} / {next.GetProperty("locality").GetString()} ({next.GetProperty("lat").GetString()}, {next.GetProperty("lng").GetString()}), {next.GetProperty("hives").GetString()} hives");
    b.AppendLine($"  Phone page: {Link(next.GetProperty("phone").GetString()!)}");
    b.AppendLine();
    b.AppendLine($"History:   two deliveries of {bee.GetProperty("name").GetString()} (lots {string.Join(", ", lots.Select(l => "#" + l.Lot))}), batch \"{history.GetProperty("batchTitle").GetString()}\" (#{batchId}) finalized,");
    b.AppendLine($"           {history.GetProperty("shipKg").GetString()} kg in stock at {orgNames["shop"]}.");
    b.AppendLine($"           Honey Story: http://localhost:5255/honeystory/{qr.ToString().ToUpperInvariant()}");
    b.AppendLine();
    b.AppendLine("Four-eyes pairs (always two different people)");
    b.AppendLine($"- Delivery and CCP 0:   {User("SampleAcceptanceOfficer", "plant")} records   -> {User("QualityApprover", "plant")} approves CCP 0");
    b.AppendLine($"- Lab report:           {User("Analyst", "lab")} enters        -> {User("LabManager", "lab")} approves");
    b.AppendLine($"- Control sheets:       {User("Staff", "plant")} enters      -> {User("QualityApprover", "plant")} approves (CCP 2-3 per lot, CCP 4-10 per batch)");
    b.AppendLine($"- New batch, finalize:  {User("Staff", "plant")} creates and requests -> {User("QualityApprover", "plant")} confirms");
    b.AppendLine($"- Shop storage:         {User("Staff", "shop")} records      -> {User("QualityApprover", "shop")} approves");
    b.AppendLine($"- Registration photo:   {User("RegistrationOfficer", "directorate")} uploads -> {User("ComplianceOfficer", "directorate")} accepts (only without a phone)");
    b.AppendLine();
    b.AppendLine("Keep this file private. It is outside the repository and must never be committed.");
    await File.WriteAllTextAsync(loginsFile, b.ToString(), new UTF8Encoding(true));
    Log($"logins file written: {loginsFile}");
}

void Log(string message) => Console.WriteLine($"[{watch.Elapsed:mm\\:ss}] {message}");

static void Expect(string actual, string expected, string step)
{
    if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"{step}: expected \"{expected}\", got \"{actual}\"");
}

static List<KeyValuePair<string, string>> With(List<KeyValuePair<string, string>> form, params (string Key, string Value)[] more) =>
    form.Concat(more.Select(m => new KeyValuePair<string, string>(m.Key, m.Value))).ToList();

static string CodeIn(string sms) => Regex.Match(sms, @"\b\d{6}\b").Value;

// the text of the error boxes of a page (the first 600 characters of each, tags removed)
static string Errors(string html) =>
    string.Join(" | ", Regex.Matches(WebUtility.HtmlDecode(html), "(?:class=\"[^\"]*(?:text-danger|alert-danger|validation-summary-errors)[^\"]*\"|id=\"[a-z-]*errors\")[^>]*>(.{0,600})", RegexOptions.Singleline)
        .Select(x => Regex.Replace(Regex.Replace(x.Groups[1].Value, "<[^>]+>", " "), "\\s+", " ").Trim()).Where(x => x.Length > 0).Distinct().Take(6));

static string E164(string phone) => "+" + Regex.Replace(phone, "[^0-9]", "");

static string FindRepo()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CLAUDE.md")))
        dir = dir.Parent;
    return dir?.FullName ?? throw new InvalidOperationException("repository not found");
}

// the practice password: the "Password:" line of the logins file in Downloads (never printed)
static string? PracticePassword(string downloads)
{
    foreach (var name in new[] { "TOP4-local-logins.txt", "TOP4-presenter-logins.txt" })
    {
        var file = Path.Combine(downloads, name);
        if (!File.Exists(file))
            continue;
        foreach (var line in File.ReadLines(file, Encoding.UTF8))
            if (Regex.Match(line, @"^Password:\s+(\S+)") is { Success: true } m)
                return m.Groups[1].Value;
    }
    return null;
}

// the phone page key in the web project's user secrets (as run-demo.ps1 keeps it); made and stored when missing
static string PhonePageKey(string webDir)
{
    var id = Regex.Match(File.ReadAllText(Path.Combine(webDir, "TOP4HoneyChains.Web.csproj")), "<UserSecretsId>([^<]+)</UserSecretsId>").Groups[1].Value;
    var secrets = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", id, "secrets.json");
    if (File.Exists(secrets))
    {
        using var json = JsonDocument.Parse(File.ReadAllText(secrets));
        if (json.RootElement.TryGetProperty("Sms:PhonePageKey", out var k) && Regex.IsMatch(k.GetString() ?? "", "^[A-Za-z0-9_-]{16,64}$"))
            return k.GetString()!;
    }
    var key = TOP4HoneyChains.Web.Services.Messaging.PhonePageKey.NewKey();
    var set = Process.Start(new ProcessStartInfo("dotnet", $"user-secrets set Sms:PhonePageKey {key} --project \"{webDir}\"") { RedirectStandardOutput = true, UseShellExecute = false })!;
    set.WaitForExit();
    if (set.ExitCode != 0)
        throw new InvalidOperationException("could not store the phone page key in the user secrets");
    return key;
}

// a one-page PDF with a few lines (Latin letters only: the standard Helvetica font)
static byte[] Pdf(string title, params string[] lines) => PdfBytes(title, lines);

static byte[] PdfBytes(string title, string[] lines)
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
    foreach (var line in lines.Append(DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)))
    {
        content.Append("BT /F1 11 Tf 56 ").Append(y.ToString(CultureInfo.InvariantCulture)).Append(" Td (").Append(Plain(line)).Append(") Tj ET\n");
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
        pdf.Append(o.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
    pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
    return Encoding.ASCII.GetBytes(pdf.ToString());
}

// ======================================================================== the app, hosted in-process with the settings of run-demo.ps1

sealed class LocalApp : WebApplicationFactory<web::Program>
{
    private readonly string _contentRoot, _connection, _phoneKey;

    public LocalApp(string contentRoot, string connection, string phoneKey)
    {
        _contentRoot = contentRoot;
        _connection = connection;
        _phoneKey = phoneKey;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseContentRoot(_contentRoot);
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _connection,
            ["Features:OrgRoleModel"] = "true", ["Features:CustodyIntake"] = "true", ["Features:BeekeeperAccounts"] = "true", ["Features:ConsumerAccounts"] = "true",
            ["Sms:Provider"] = "Fake", ["Sms:AllowFake"] = "true", ["Sms:PhonePageKey"] = _phoneKey,
            ["App:PublicBaseUrl"] = "http://localhost:5255"
        };
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(settings));
    }
}

// one signed-in person: own cookies, anti-forgery tokens from the pages, flash messages read back (English texts)
sealed class Client
{
    private readonly HttpClient _http;

    private Client(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
    }

    public static Client Anonymous(LocalApp app) => new(app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true, BaseAddress = new Uri("http://localhost:5255") }));

    public static async Task<(Client Client, HttpResponseMessage Response)> TrySignInAsync(LocalApp app, string user, string password)
    {
        var c = Anonymous(app);
        var response = await c.PostAsync("/Account/Login", new() { new("UserName", user), new("Password", password) }, "/Account/Login");
        return (c, response);
    }

    public static async Task<Client> SignInAsync(LocalApp app, string user, string password)
    {
        var (c, response) = await TrySignInAsync(app, user, password);
        if (response.StatusCode != HttpStatusCode.Redirect || (response.Headers.Location?.ToString() ?? "").Contains("/Account/Login"))
            throw new InvalidOperationException($"sign-in of {user} failed ({(int)response.StatusCode})");
        return c;
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
