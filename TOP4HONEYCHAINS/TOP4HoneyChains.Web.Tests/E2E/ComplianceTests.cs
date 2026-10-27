using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P3 (D78-SPEC 2.5, 4.8, O-9, D-82): the Compliance Officer reviews sampling quality (never their own item), keeps
    // data-subject requests with the 30-day deadline (a consent withdrawal takes effect at once), reads the organization's audit
    // entries and keeps the agreements; a new organization is verified only with a Compliance Officer, an existing one is warned.
    public partial class CustodyIntakeTests
    {
        [Fact]
        public async Task The_compliance_officer_reviews_sampling_quality_but_never_an_item_they_recorded() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("co1", hasPhone: false);
            var form = Form(p, 1);
            form["FallbackReason"] = "zz_test no phone";
            var saved = await p.Sao.PostMultipartAsync("/Intake/Record", Op(form, "photo"), Pdf.File("deliveryForm"), $"/Intake/Create?number={p.BusinessNumber}");
            var intakeId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());

            Assert.Equal(404, await p.Sao.StatusAsync("/Compliance"));
            var page = await p.Co.HtmlAsync("/Compliance");
            Assert.Contains($"data-subject=\"Intake-{intakeId}\" data-open=\"true\"", page);
            Assert.Contains("/Compliance#quality", await p.Co.HtmlAsync("/MyWork/Index"));
            Assert.Contains("id=\"menu-compliance\"", page);

            Dictionary<string, string> Review(string outcome, string finding, string? action = null) => new()
            {
                ["subjectType"] = "Intake", ["subjectId"] = intakeId.ToString(), ["outcome"] = outcome, ["finding"] = finding, ["action"] = action ?? ""
            };
            Assert.Contains("A finding needs the action", await p.Co.PostFlashAsync("/Compliance/Review", Review("Finding", "zz photo blurred"), "/Compliance"));
            // the recorder never reviews (four-eyes): the same person as Compliance Officer
            var coId = await _app.WithContextAsync(db => db.Memberships.Where(m => m.OrgId == p.OrgId && m.OrgRole == OrgRoles.ComplianceOfficer).Select(m => m.UserId).FirstAsync());
            var recorder = await _app.WithContextAsync(db => db.LotIntakes.Where(i => i.IntakeId == intakeId).Select(i => i.ReceivedByUserId).FirstAsync());
            // test setup only: the delivery lock (installed by other tests) would refuse it
            static string Set(string user, int id) => $@"IF OBJECT_ID(N'dbo.TR_LotIntakes_DeliveryLocked') IS NOT NULL DISABLE TRIGGER dbo.TR_LotIntakes_DeliveryLocked ON dbo.LotIntakes;
                UPDATE dbo.LotIntakes SET ReceivedByUserId = N'{user}' WHERE IntakeId = {id};
                IF OBJECT_ID(N'dbo.TR_LotIntakes_DeliveryLocked') IS NOT NULL ENABLE TRIGGER dbo.TR_LotIntakes_DeliveryLocked ON dbo.LotIntakes;";
            await using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_app.ConnectionString))
            {
                await conn.OpenAsync();
                await new Microsoft.Data.SqlClient.SqlCommand(Set(coId, intakeId), conn).ExecuteNonQueryAsync();
                Assert.Contains("Four-eyes", await p.Co.PostFlashAsync("/Compliance/Review", Review("InOrder", "zz"), "/Compliance"));
                await new Microsoft.Data.SqlClient.SqlCommand(Set(recorder, intakeId), conn).ExecuteNonQueryAsync();
            }
            Assert.Contains("Review recorded.", await p.Co.PostFlashAsync("/Compliance/Review", Review("InOrder", "zz form and weights checked"), "/Compliance"));
            Assert.Contains($"data-subject=\"Intake-{intakeId}\" data-open=\"false\"", await p.Co.HtmlAsync("/Compliance"));
            Assert.DoesNotContain("/Compliance#quality", await p.Co.HtmlAsync("/MyWork/Index"));
            // another organization's item cannot be reviewed
            var other = await PlantAsync("co1x");
            Assert.Contains("not one of your organization", await other.Co.PostFlashAsync("/Compliance/Review", Review("InOrder", "zz"), "/Compliance"));
            // the audit entries of the organization
            Assert.Contains("id=\"audit\"", await p.Co.HtmlAsync("/Compliance"));
            return 0;
        });

        [Fact]
        public async Task Data_subject_requests_have_a_30_day_deadline_and_a_consent_withdrawal_takes_effect_at_once()
        {
            var p = await PlantAsync("co2");
            await _app.WithContextAsync(async db =>
            {
                db.Consents.Add(new Consent
                {
                    BeekeeperId = p.BeekeeperId, Purpose = ConsentPurpose.PublicName, NameConsented = true, TextVersion = "zz", TextSha256 = new string('0', 64),
                    GivenOn = new DateOnly(2026, 10, 1), Method = "SmsCode", RecordedByUserId = "seed", RecordedAt = DateTime.UtcNow
                });
                return await db.SaveChangesAsync();
            });
            var received = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd");
            Assert.Contains("Request recorded", await p.Co.PostFlashAsync("/Compliance/RecordRequest", new()
            {
                ["subjectName"] = "zz_test subject", ["kind"] = "Access", ["channel"] = "letter", ["receivedOn"] = received, ["details"] = "zz copy of my data"
            }, "/Compliance"));
            var dsr = await _app.WithContextAsync(db => db.DataSubjectRequests.SingleAsync(r => r.OrgId == p.OrgId));
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(27)), dsr.DueOn);
            Assert.Contains("/Compliance#requests", await p.Co.HtmlAsync("/MyWork/Index"));
            Assert.Contains("Describe the answer", await p.Co.PostFlashAsync("/Compliance/CloseRequest", new() { ["requestId"] = dsr.RequestId.ToString(), ["status"] = "Answered" }, "/Compliance"));
            Assert.Contains("Request closed.", await p.Co.PostFlashAsync("/Compliance/CloseRequest", new() { ["requestId"] = dsr.RequestId.ToString(), ["status"] = "Answered", ["outcome"] = "zz sent" }, "/Compliance"));

            // a withdrawal received by letter: recorded and effective at once
            Assert.Contains("needs the beekeeper's number", await p.Co.PostFlashAsync("/Compliance/RecordRequest", new()
            {
                ["subjectName"] = "zz_test bee", ["kind"] = "ConsentWithdrawal", ["channel"] = "phone", ["receivedOn"] = received, ["details"] = "zz"
            }, "/Compliance"));
            Assert.Contains("Request recorded", await p.Co.PostFlashAsync("/Compliance/RecordRequest", new()
            {
                ["subjectName"] = "zz_test bee", ["beekeeperNumber"] = p.BusinessNumber, ["kind"] = "ConsentWithdrawal", ["consentPurpose"] = ConsentPurpose.PublicName,
                ["channel"] = "phone", ["receivedOn"] = received, ["details"] = "zz no name please"
            }, "/Compliance"));
            Assert.False(await _app.WithContextAsync(db => db.Consents.AnyAsync(c => c.BeekeeperId == p.BeekeeperId && c.Purpose == ConsentPurpose.PublicName && c.WithdrawnAt == null)));
            Assert.Equal(DataSubjectRequestStatus.Answered, await _app.WithContextAsync(db => db.DataSubjectRequests.Where(r => r.Kind == "ConsentWithdrawal" && r.OrgId == p.OrgId).Select(r => r.Status).FirstAsync()));
            // not for other roles
            Assert.Equal(HttpStatusCode.NotFound, (await p.Sao.PostAsync("/Compliance/CloseRequest", new() { ["requestId"] = dsr.RequestId.ToString(), ["status"] = "Refused", ["outcome"] = "x" }, "/Intake/Index")).StatusCode);
        }

        [Fact]
        public async Task A_new_organization_is_verified_only_with_a_compliance_officer_and_an_existing_one_is_warned()
        {
            await _app.CreateUserAsync("zz_test_co3_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_co3_admin");
            var pending = await _app.WithContextAsync(async db =>
            {
                var o = new Organization { DisplayName = "zz_test co3 pending", Status = OrgStatus.Pending, CreatedAt = DateTime.UtcNow };
                db.Organizations.Add(o);
                await db.SaveChangesAsync();
                db.OrgDocuments.Add(new OrgDocument { OrgId = o.OrgId, Kind = "Licence", OriginalFileName = "zz.pdf", StoredFileName = "zz.pdf", Sha256 = new string('c', 64), UploadedByUserId = "seed", UploadedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                return o.OrgId;
            });
            var verify = new Dictionary<string, string> { ["toStatus"] = OrgStatus.Verified };
            Assert.Contains("Name a Compliance Officer", await admin.PostFlashAsync($"/Organizations/ChangeStatus/{pending}", verify, $"/Organizations/Details/{pending}"));
            await _app.AddMemberAsync(pending, await _app.CreateUserAsync("zz_test_co3_co"), OrgRoles.ComplianceOfficer);
            Assert.Contains("now Verified", await admin.PostFlashAsync($"/Organizations/ChangeStatus/{pending}", verify, $"/Organizations/Details/{pending}"));

            // an organization verified before: warned with the deadline, never blocked
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Compliance:GraceStart"] = "2026-11-01";
            try
            {
                var existing = await _app.CreateOrgAsync("zz_test co3 existing", null, (Capabilities.Processing, null));
                await _app.AddMemberAsync(existing, await _app.CreateUserAsync("zz_test_co3_oa"), OrgRoles.OrgAdmin);
                var oa = await UserClient.LoginAsync(_app, "zz_test_co3_oa");
                var workspace = WebUtility.HtmlDecode(await oa.HtmlAsync("/OrgWorkspace"));
                Assert.Contains("id=\"missing-compliance-officer\"", workspace);
                Assert.Contains("31.12.2026", workspace);
                // the data controller details are visible to members
                Assert.Contains("id=\"data-controller\"", await oa.HtmlAsync($"/Organizations/Details/{existing}"));
            }
            finally
            {
                config["Compliance:GraceStart"] = "";
            }
        }

        [Fact]
        public async Task The_compliance_officer_keeps_the_agreements()
        {
            var p = await PlantAsync("co4");
            var upload = await p.Co.PostMultipartAsync($"/Organizations/UploadDocument/{p.OrgId}", new() { ["kind"] = AgreementKinds.DataProcessingAgreement },
                Pdf.File("file"), $"/Organizations/Details/{p.OrgId}");
            Assert.Contains("Document uploaded.", await p.Co.FlashAfterAsync(upload));
            var doc = await _app.WithContextAsync(db => db.OrgDocuments.SingleAsync(d => d.OrgId == p.OrgId));
            Assert.Equal(("Other", AgreementKinds.DataProcessingAgreement), (doc.Kind, doc.AgreementKind));
            Assert.Contains("Data processing agreement", WebUtility.HtmlDecode(await p.Co.HtmlAsync("/Compliance")));
            // the sample officer does not manage documents
            var refused = await p.Sao.PostMultipartAsync($"/Organizations/UploadDocument/{p.OrgId}", new() { ["kind"] = AgreementKinds.SupplyAgreement },
                Pdf.File("file"), $"/Organizations/Details/{p.OrgId}");
            Assert.Contains("/Account/AccessDenied", refused.Headers.Location?.ToString());
            Assert.Equal(1, await _app.WithContextAsync(db => db.OrgDocuments.CountAsync(d => d.OrgId == p.OrgId)));
        }
    }
}
