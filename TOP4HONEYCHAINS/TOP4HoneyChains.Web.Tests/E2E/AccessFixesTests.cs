using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P5 (D78-SPEC 6.4, PLANNED-APP-CHANGES C1-C6, D-84): each fix with its allowed case and its refused case
    public partial class CustodyIntakeTests
    {
        private async Task<UserClient> MemberClientAsync(int orgId, string name, string role)
        {
            await _app.AddMemberAsync(orgId, await _app.CreateUserAsync(name), role);
            return await UserClient.LoginAsync(_app, name);
        }

        private Task<bool> DownloadAuditedAsync(string entity, string key) =>
            _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Action == "Download" && a.Entity == entity && a.EntityKey == key));

        [Fact]
        public async Task C2_the_signed_delivery_form_is_downloaded_only_by_the_roles_that_need_it_and_audited() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("c2a", hasPhone: false);
            var form = Form(p, 1);
            form["FallbackReason"] = "zz_test no phone";
            var saved = await p.Sao.PostMultipartAsync("/Intake/Record", Op(form, "photo"), Pdf.File("deliveryForm"), $"/Intake/Create?number={p.BusinessNumber}");
            var intakeId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());
            var url = $"/Intake/Form/{intakeId}";
            Assert.Equal(200, await p.Sao.StatusAsync(url));
            Assert.Equal(200, await p.Co.StatusAsync(url));
            Assert.True(await DownloadAuditedAsync("LotIntakes", intakeId.ToString()));
            var staff = await MemberClientAsync(p.OrgId, "zz_test_c2a_staff", OrgRoles.Staff);
            Assert.Equal(403, await staff.StatusAsync(url));
            var other = await PlantAsync("c2b");
            Assert.Equal(404, await other.Sao.StatusAsync(url));
            return 0;
        });

        [Fact]
        public async Task C2_organization_documents_are_downloaded_by_the_admin_and_the_compliance_officer_only()
        {
            var p = await PlantAsync("c2c");
            var upload = await p.Co.PostMultipartAsync($"/Organizations/UploadDocument/{p.OrgId}", new() { ["kind"] = "Licence" }, Pdf.File("file"), $"/Organizations/Details/{p.OrgId}");
            Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
            var docId = await _app.WithContextAsync(db => db.OrgDocuments.Where(d => d.OrgId == p.OrgId).Select(d => d.DocumentId).SingleAsync());
            var url = $"/Organizations/Document/{docId}";
            Assert.Equal(200, await p.Co.StatusAsync(url));
            var oa = await MemberClientAsync(p.OrgId, "zz_test_c2c_oa", OrgRoles.OrgAdmin);
            Assert.Equal(200, await oa.StatusAsync(url));
            Assert.True(await DownloadAuditedAsync("OrgDocuments", docId.ToString()));
            Assert.Equal(403, await p.Sao.StatusAsync(url));
            var other = await PlantAsync("c2d");
            Assert.Equal(404, await other.Co.StatusAsync(url));
        }

        [Fact]
        public async Task C2_the_storefront_consent_document_is_for_the_association_admin_and_compliance_officer()
        {
            var p = await PlantAsync("c2e");
            var association = await _app.CreateOrgAsync("zz_test c2e association", null, (Capabilities.Association, null));
            var oa = await MemberClientAsync(association, "zz_test_c2e_oa", OrgRoles.OrgAdmin);
            var staff = await MemberClientAsync(association, "zz_test_c2e_staff", OrgRoles.Staff);
            await _app.WithContextAsync(async db =>
            {
                db.BeekeeperAssociations.Add(new BeekeeperAssociation { BeekeeperId = p.BeekeeperId, AssociationOrgId = association, ValidFrom = new DateOnly(2026, 1, 1), CreatedAt = DateTime.UtcNow });
                return await db.SaveChangesAsync();
            });
            var recorded = await oa.PostMultipartAsync($"/Storefronts/Consent/{p.BeekeeperId}", new() { ["nameConsented"] = "true", ["givenOn"] = "2026-10-01", ["textRead"] = "true" },
                Pdf.File("document"), $"/Storefronts/Consent/{p.BeekeeperId}");
            Assert.Equal(HttpStatusCode.Redirect, recorded.StatusCode);
            var consentId = await _app.WithContextAsync(db => db.Consents.Where(c => c.BeekeeperId == p.BeekeeperId && c.Purpose == ConsentPurpose.Storefront).Select(c => c.ConsentId).SingleAsync());
            var url = $"/Storefronts/Document/{consentId}";
            Assert.Equal(200, await oa.StatusAsync(url));
            Assert.True(await DownloadAuditedAsync("Consents", consentId.ToString()));
            Assert.Equal(403, await staff.StatusAsync(url));
            Assert.Equal(404, await p.Co.StatusAsync(url));
            // C1: the association's beekeeper pages need the read right (Staff does not have it)
            Assert.Equal(200, await oa.StatusAsync("/Storefronts/Index"));
            var denied = await staff.GetAsync("/Storefronts/Index");
            Assert.StartsWith("/MyWork", denied.Headers.Location?.ToString());
            Assert.Contains("cannot open this page", await staff.FlashAfterAsync(denied));
        }

        [Fact]
        public async Task C1_the_shipments_page_needs_the_read_right()
        {
            var p = await PlantAsync("c1a");
            var oa = await MemberClientAsync(p.OrgId, "zz_test_c1a_oa", OrgRoles.OrgAdmin);
            Assert.Equal(200, await oa.StatusAsync("/Shipments/Index"));
            var inspector = await MemberClientAsync(p.OrgId, "zz_test_c1a_fi", OrgRoles.FieldInspector);
            var denied = await inspector.GetAsync("/Shipments/Index");
            Assert.StartsWith("/MyWork", denied.Headers.Location?.ToString());
            Assert.Contains("cannot open this page", await inspector.FlashAfterAsync(denied));
        }

        [Fact]
        public async Task C3_sampling_shipping_and_finalizing_check_the_organizations_capability()
        {
            // a retailer with the right roles but without the processing or packing capability
            var retailer = await _app.CreateOrgAsync("zz_test c3 retailer", null, (Capabilities.Retail, null));
            var sao = await MemberClientAsync(retailer, "zz_test_c3r_sao", OrgRoles.SampleAcceptanceOfficer);
            var staff = await MemberClientAsync(retailer, "zz_test_c3r_staff", OrgRoles.Staff);
            var p = await PlantAsync("c3");
            var (lot, batch) = await _app.WithContextAsync(async db =>
            {
                var r = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = retailer, OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(r);
                var w = new Wholesaler { WholesalerTitle = "zz_test c3 wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                await db.SaveChangesAsync();
                var b = new FinalHoneyProduction
                {
                    FinalHoneyProductionTitle = "zz_test c3 batch", FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 10,
                    FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = retailer, FinalHoneyProductionWholesalerId = w.WholesalerId
                };
                db.FinalHoneyProductions.Add(b);
                await db.SaveChangesAsync();
                return (r.RawHoneyProductionId, b.FinalHoneyProductionId);
            });
            var sample = await sao.GetAsync($"/Samples/Create?lotId={lot}");
            Assert.Contains("Only a processing organization takes samples.", await sao.FlashAfterAsync(sample));
            Assert.Contains("Only a processing or packing organization finalizes batches.",
                await staff.PostFlashAsync($"/OrgWorkspace/RequestFinalize/{batch}", new(), $"/OrgWorkspace/Batch/{batch}"));
            // a laboratory has the shipping roles but does not ship honey
            var lab = await MemberClientAsync(p.LabId, "zz_test_c3_lab_oa", OrgRoles.OrgAdmin);
            var ship = await lab.GetAsync($"/Shipments/Create?batchId={batch}");
            var index = await lab.GetAsync(ship.Headers.Location!.ToString());   // the shipments page sends the laboratory on to My work
            Assert.Contains("Only an organization that processes, packs or trades honey", await lab.FlashAfterAsync(index));
            // the processing plant may take samples (allowed case)
            var own = await _app.WithContextAsync(async db =>
            {
                var r = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = p.OrgId, OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(r);
                await db.SaveChangesAsync();
                return r.RawHoneyProductionId;
            });
            Assert.Equal(200, await p.Sao.StatusAsync($"/Samples/Create?lotId={own}"));
        }

        [Fact]
        public async Task C4_the_public_verify_page_answers_only_for_approved_reports() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("c4");
            var lab = await LabUsersAsync(p);
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Lab:AnalysisMode"] = "PerContainer";
            try
            {
                var (_, lot) = await ConfirmBySmsAsync(p, Form(p, 2));
                await Ccp0DoneAsync(lot);
                var jars = await JarsAsync(lot);
                foreach (var j in jars)
                    await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(j.SampleId, true, true, true), "/Samples/Index");
                // jar 1: submitted, not approved
                var ccp1 = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 1").Select(c => c.Id).FirstAsync());
                var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(x => x.CCPId == ccp1 && x.DataType != CcpDataType.File).ToListAsync());
                var results = new Dictionary<string, string> { ["reportNumber"] = "ZZ-C4", ["issuedOn"] = DateTime.Today.ToString("yyyy-MM-dd") };
                foreach (var x in parameters)
                    results[$"values[{x.ParameterId}]"] = x.DataType == CcpDataType.Text ? "zz" : "1";
                await lab.Analyst.PostMultipartAsync($"/LabReports/Save/{jars[0].SampleId}", results, Pdf.File("reportFile"), $"/LabReports/Edit/{jars[0].SampleId}");
                await lab.Analyst.PostFlashAsync($"/LabReports/Submit/{jars[0].SampleId}", new(), $"/LabReports/Edit/{jars[0].SampleId}");
                // jar 2: approved
                await ReportAsync(lab, jars[1].SampleId);
                var codes = await _app.WithContextAsync(db => db.LabReports.Where(r => r.Sample!.RawHoneyProductionId == lot).OrderBy(r => r.SampleId).Select(r => new { r.ReportCode, r.Status }).ToListAsync());
                Assert.Equal(LabReportStatus.Submitted, codes[0].Status);
                var anonymous = UserClient.Anonymous(_app);
                var submitted = await anonymous.HtmlAsync($"/verify-report?code={codes[0].ReportCode}");
                var approved = await anonymous.HtmlAsync($"/verify-report?code={codes[1].ReportCode}");
                Assert.Contains("id=\"verify-result\"", approved);
                Assert.DoesNotContain("id=\"verify-result\"", submitted);
            }
            finally
            {
                config["Lab:AnalysisMode"] = "Composite";
            }
            return 0;
        });

        [Fact]
        public async Task C6_with_the_central_setting_only_the_central_association_verifies_associations()
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            await _app.CreateUserAsync("zz_test_c6_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_c6_admin");
            var central = await _app.CreateOrgAsync("zz_test c6 central", null, (Capabilities.Association, null), (Capabilities.CentralAssociation, null));
            var centralOa = await MemberClientAsync(central, "zz_test_c6_central_oa", OrgRoles.OrgAdmin);
            async Task<int> PendingAssociationAsync(string tag)
            {
                var org = await _app.WithContextAsync(async db =>
                {
                    var o = new Organization { DisplayName = $"zz_test c6 {tag}", Status = OrgStatus.Pending, CreatedAt = DateTime.UtcNow };
                    db.Organizations.Add(o);
                    await db.SaveChangesAsync();
                    db.OrgCapabilities.Add(new OrgCapability { OrgId = o.OrgId, Capability = Capabilities.Association, AddedAt = DateTime.UtcNow });
                    db.OrgDocuments.Add(new OrgDocument { OrgId = o.OrgId, Kind = "Licence", OriginalFileName = "zz.pdf", StoredFileName = "zz.pdf", Sha256 = new string('c', 64), UploadedByUserId = "seed", UploadedAt = DateTime.UtcNow });
                    await db.SaveChangesAsync();
                    return o.OrgId;
                });
                await _app.AddMemberAsync(org, await _app.CreateUserAsync($"zz_test_c6_{tag}_co"), OrgRoles.ComplianceOfficer);
                return org;
            }
            var verify = new Dictionary<string, string> { ["toStatus"] = OrgStatus.Verified };
            config["OrgModel:AssociationVerifier"] = Capabilities.CentralAssociation;
            try
            {
                var a1 = await PendingAssociationAsync("a1");
                var refused = await admin.PostAsync($"/Organizations/ChangeStatus/{a1}", verify, $"/Organizations/Details/{a1}");
                Assert.Contains("/Account/AccessDenied", refused.Headers.Location?.ToString());
                Assert.DoesNotContain($"/Organizations/Details/{a1}", await admin.HtmlAsync("/MyWork/Index"));
                Assert.Contains("now Verified", await centralOa.PostFlashAsync($"/Organizations/ChangeStatus/{a1}", verify, $"/Organizations/Details/{a1}"));
            }
            finally
            {
                config["OrgModel:AssociationVerifier"] = "";
            }
            // without the setting the platform administrators verify associations as before
            var a2 = await PendingAssociationAsync("a2");
            Assert.Contains("now Verified", await admin.PostFlashAsync($"/Organizations/ChangeStatus/{a2}", verify, $"/Organizations/Details/{a2}"));
        }
    }
}
