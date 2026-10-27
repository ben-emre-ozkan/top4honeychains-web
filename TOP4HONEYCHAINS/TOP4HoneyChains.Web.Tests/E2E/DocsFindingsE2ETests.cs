using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-75: the findings of the documentation pass, each checked against the running application
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class DocsFindingsE2ETests
    {
        private readonly E2EApp _app;

        public DocsFindingsE2ETests(E2EApp app) => _app = app;

        private record Seed(string Suffix, int Processor, int Market, int Lab, int Unlinked, int Wholesaler, int LotNa, int LotSealed, int LotNoWholesaler,
            int LotOtherWholesaler, int LotUnlinked, int SealedBatch, Guid SealedQr, int OpenBatch, int OtherBatchQrLot, Guid OtherQr, int Photo, int Sample,
            int Ccp2, string BeekeeperLastName, string ApiaryTitle);

        private static byte[] PhotoWithGps()
        {
            using var image = new Image<Rgba32>(40, 30, new Rgba32(255, 181, 71));
            image.Metadata.ExifProfile = new ExifProfile();
            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitudeRef, "N");
            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitude, new[] { new Rational(40, 1), new Rational(51, 1), new Rational(30, 1) });
            image.Metadata.ExifProfile.SetValue(ExifTag.Make, "D75Camera");
            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            return ms.ToArray();
        }

        private async Task<Seed> SeedAsync()
        {
            var suffix = Random.Shared.Next(10000, 99999).ToString(CultureInfo.InvariantCulture);
            var wholesalerId = await _app.WithContextAsync(async db =>
            {
                var w = new Wholesaler { WholesalerTitle = "D75 Wholesaler " + suffix, WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                await db.SaveChangesAsync();
                return w.WholesalerId;
            });
            var processor = await _app.CreateOrgAsync("D75 Processor " + suffix, wholesalerId, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var market = await _app.CreateOrgAsync("D75 Market " + suffix, null, (Capabilities.Retail, null));
            var lab = await _app.CreateOrgAsync("D75 Lab " + suffix, null, (Capabilities.Laboratory, null));
            var unlinked = await _app.CreateOrgAsync("D75 Unlinked " + suffix, null, (Capabilities.Processing, null));
            return await _app.WithContextAsync(async db =>
            {
                var ccp0 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => c.Id).FirstAsync();
                var ccp2 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 2").Select(c => c.Id).FirstAsync();
                var other = new Wholesaler { WholesalerTitle = "D75 Other " + suffix, WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(other);
                var bk = new Beekeeper
                {
                    BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "D75bee", BeekeeperLastName = "Keeper" + suffix, BeekeeperBirthDate = new DateOnly(1980, 1, 1),
                    BeekeeperBusinessNumber = "D75-" + suffix, IsActive = true
                };
                db.Beekeepers.Add(bk);
                await db.SaveChangesAsync();
                var apiary = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId!, ApiaryTitle = "D75 Apiary " + suffix, ApiaryBeehivesCount = 5, ApiaryQRCode = Guid.NewGuid(), ApiaryLocationLatitude = 40.9m, ApiaryLocationLongitude = 31.7m };
                var otherApiary = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId!, ApiaryTitle = "D75 Other Apiary " + suffix, ApiaryBeehivesCount = 5, ApiaryQRCode = Guid.NewGuid(), ApiaryLocationLatitude = 40.9m, ApiaryLocationLongitude = 31.7m };
                db.Apiaries.AddRange(apiary, otherApiary);
                await db.SaveChangesAsync();
                RawHoneyProduction Lot(int? owner, int? wholesaler, Apiary a) => new()
                {
                    RawHoneyProductionApiaryId = a.ApiaryId, RawHoneyProductionAmount = 50, RawHoneyProductionDate = new DateOnly(2026, 7, 1),
                    RawHoneyProductionHoneyType = db.HoneyTypes.Select(h => h.HoneyTypeId).First(),
                    RawHoneyProductionPeriodId = db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).First(),
                    OwnerOrgId = owner, OriginCountry = "TR", RawHoneyProductionWholesalerId = wholesaler
                };
                var lotNa = Lot(processor, wholesalerId, apiary);
                var lotSealed = Lot(processor, wholesalerId, apiary);
                var lotNoWholesaler = Lot(processor, null, apiary);
                var lotOther = Lot(processor, other.WholesalerId, apiary);
                var lotUnlinked = Lot(unlinked, null, apiary);
                var lotOtherBatch = Lot(processor, wholesalerId, otherApiary);
                db.RawHoneyProductions.AddRange(lotNa, lotSealed, lotNoWholesaler, lotOther, lotUnlinked, lotOtherBatch);
                await db.SaveChangesAsync();
                RawHoneyCCPAssignment Done(int lot, int ccp) => new()
                {
                    RawHoneyProductionId = lot, RawHoneyCCPAssignmentCCP = ccp, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now, ApprovedAt = DateTime.UtcNow, ApprovedByUserId = "seed", ApprovedByOrgId = processor
                };
                db.RawHoneyCCPAssignments.AddRange(Done(lotSealed.RawHoneyProductionId, ccp0), Done(lotNoWholesaler.RawHoneyProductionId, ccp2),
                    Done(lotOther.RawHoneyProductionId, ccp2));
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = lotNa.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = ccp2, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.NotApplicable,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now, NotApplicableReason = "not heated", NotApplicableBy = "e2e_d75_qa1_" + suffix, NotApplicableAt = DateTime.Now
                });
                FinalHoneyProduction Batch(bool sealedBatch) => new()
                {
                    FinalHoneyProductionTitle = "D75 Batch " + suffix, FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 50,
                    FinalHoneyProductionIsFinalized = sealedBatch, FinalHoneyProductionFinalizedDate = sealedBatch ? DateTime.Today : null,
                    FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = processor, FinalHoneyProductionWholesalerId = wholesalerId
                };
                var sealedBatch = Batch(true);
                var openBatch = Batch(false);
                var otherBatch = Batch(true);
                db.FinalHoneyProductions.AddRange(sealedBatch, openBatch, otherBatch);
                await db.SaveChangesAsync();
                db.FinalHoneyProductionDetails.AddRange(
                    new FinalHoneyProductionDetail { FinalHoneyProductionId = sealedBatch.FinalHoneyProductionId, RawHoneyProductionId = lotSealed.RawHoneyProductionId, UsedAmount = 50, CreatedAt = DateTime.Now },
                    new FinalHoneyProductionDetail { FinalHoneyProductionId = otherBatch.FinalHoneyProductionId, RawHoneyProductionId = lotOtherBatch.RawHoneyProductionId, UsedAmount = 50, CreatedAt = DateTime.Now });
                db.Shipments.Add(new Shipment
                {
                    FinalHoneyProductionId = sealedBatch.FinalHoneyProductionId, FromOrgId = processor, ToOrgId = market, Quantity = 10, Status = ShipmentStatus.Received,
                    ShippedByUserId = "seed", ShippedAt = DateTime.UtcNow, DecidedByUserId = "seed", DecidedAt = DateTime.UtcNow
                });
                var photo = new ApiaryPhoto { ApiaryPhotoApiaryId = apiary.ApiaryId, ApiaryPhotoData = PhotoWithGps(), ApiaryPhotoContentType = "image/jpeg", ApiaryPhotoFileName = "d75.jpg", ApiaryPhotoCreatedDate = DateTime.Now };
                db.ApiaryPhotos.Add(photo);
                var sample = new Sample
                {
                    SampleCode = "D75-" + suffix, RawHoneyProductionId = lotSealed.RawHoneyProductionId, TakingOrgId = processor, LabOrgId = lab, SealNumber = "D75-S" + suffix,
                    CounterSealNumber = "D75-C" + suffix, CounterLocation = "D75", RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), TakenByUserId = "seed",
                    TakenAt = DateTime.UtcNow, Status = SampleStatus.Received, SealIntact = true
                };
                db.Samples.Add(sample);
                await db.SaveChangesAsync();
                return new Seed(suffix, processor, market, lab, unlinked, wholesalerId, lotNa.RawHoneyProductionId, lotSealed.RawHoneyProductionId,
                    lotNoWholesaler.RawHoneyProductionId, lotOther.RawHoneyProductionId, lotUnlinked.RawHoneyProductionId, sealedBatch.FinalHoneyProductionId,
                    sealedBatch.FinalHoneyProductionQR, openBatch.FinalHoneyProductionId, lotOtherBatch.RawHoneyProductionId, otherBatch.FinalHoneyProductionQR,
                    photo.ApiaryPhotoId, sample.SampleId, ccp2, bk.BeekeeperLastName, apiary.ApiaryTitle);
            });
        }

        private async Task<UserClient> MemberAsync(string name, int orgId, string role)
        {
            var id = await _app.CreateUserAsync(name);
            await _app.AddMemberAsync(orgId, id, role);
            return await UserClient.LoginAsync(_app, name);
        }

        // A1
        [Fact]
        public async Task Roles_the_platform_relies_on_cannot_be_renamed_or_deleted()
        {
            var name = "e2e_d75_roles_" + Random.Shared.Next(10000, 99999);
            await _app.CreateUserAsync(name, "Administrator");
            var admin = await UserClient.LoginAsync(_app, name);
            var adminRoleId = await _app.WithContextAsync(db => Task.FromResult(_app.Services.CreateScope().ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>().Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First()));

            Assert.Contains("cannot be renamed or deleted", await admin.PostFlashAsync("/RoleManagement/Delete", new() { ["id"] = adminRoleId }, "/RoleManagement/Index"));
            Assert.Contains("cannot be renamed or deleted", await admin.PostFlashAsync("/RoleManagement/Edit", new() { ["id"] = adminRoleId, ["roleName"] = "Boss" }, "/RoleManagement/Index"));
            using var scope = _app.Services.CreateScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Assert.True(await roles.RoleExistsAsync("Administrator"));
            Assert.False(await roles.RoleExistsAsync("Boss"));
            // the page marks the role as protected instead of offering edit and delete
            Assert.Contains("mdi-lock-outline", await admin.HtmlAsync("/RoleManagement/Index"));
        }

        // A2, A3
        [Fact]
        public async Task Whoever_marked_a_CCP_not_applicable_cannot_approve_it_and_my_work_offers_it_to_another_approver()
        {
            var s = await SeedAsync();
            var qa1 = await MemberAsync("e2e_d75_qa1_" + s.Suffix, s.Processor, OrgRoles.QualityApprover);
            var qa2 = await MemberAsync("e2e_d75_qa2_" + s.Suffix, s.Processor, OrgRoles.QualityApprover);
            var url = $"rawHoneyProductionId={s.LotNa}&amp;ccpId={s.Ccp2}";

            Assert.DoesNotContain(url, await qa1.HtmlAsync("/MyWork/Index"));
            Assert.Contains(url, await qa2.HtmlAsync("/MyWork/Index"));
            var form = new Dictionary<string, string> { ["ownerKind"] = "raw", ["ownerId"] = s.LotNa.ToString(), ["ccpId"] = s.Ccp2.ToString() };
            Assert.Contains("you marked this CCP \"Not applicable\"", await qa1.PostFlashAsync("/CCPMonitoring/Approve", new(form), "/MyWork/Index"));
            Assert.Null(await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == s.LotNa).Select(a => a.ApprovedAt).FirstAsync()));
            Assert.Contains("CCP record approved.", await qa2.PostFlashAsync("/CCPMonitoring/Approve", new(form), "/MyWork/Index"));
            Assert.DoesNotContain(url, await qa2.HtmlAsync("/MyWork/Index"));
        }

        // A3: corrections show a translated record kind
        [Fact]
        public async Task Correction_items_show_a_translated_record_kind()
        {
            var s = await SeedAsync();
            var name = "e2e_d75_corr_" + s.Suffix;
            await _app.CreateUserAsync(name, "Administrator");
            var admin = await UserClient.LoginAsync(_app, name);
            await _app.WithContextAsync(async db =>
            {
                var parameter = await db.CriticalControlPointParameters.Where(p => p.CCPId == s.Ccp2).Select(p => p.ParameterId).FirstAsync();
                db.CcpCorrections.Add(new CcpCorrection
                {
                    OwnerKind = "raw", OwnerId = s.LotNoWholesaler, CcpId = s.Ccp2, ParameterId = parameter, NewTextValue = "x", Reason = "D75 test",
                    RequestedByUserId = "another-admin", RequestedAt = DateTime.UtcNow
                });
                return await db.SaveChangesAsync();
            });
            var html = await admin.HtmlAsync("/MyWork/Index");
            Assert.Contains($"#{s.LotNoWholesaler} · lot", System.Net.WebUtility.HtmlDecode(html));
            Assert.DoesNotContain(" · raw<", html);
        }

        // A4
        [Fact]
        public async Task A_holder_reads_the_producers_CCPs_without_beekeeper_names_and_has_a_link_to_them()
        {
            var s = await SeedAsync();
            var holder = await MemberAsync("e2e_d75_market_" + s.Suffix, s.Market, OrgRoles.Staff);
            var owner = await MemberAsync("e2e_d75_staff_" + s.Suffix, s.Processor, OrgRoles.Staff);
            var ccp0 = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => c.Id).FirstAsync());
            var page = $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={s.LotSealed}&ccpId={ccp0}";

            var held = await holder.HtmlAsync(page);
            Assert.DoesNotContain(s.BeekeeperLastName, held);
            Assert.DoesNotContain(s.ApiaryTitle, held);
            Assert.Contains(s.BeekeeperLastName, await owner.HtmlAsync(page));
            Assert.Contains($"href=\"/Flow/Batch/{s.SealedBatch}\"", await holder.HtmlAsync("/Shipments/Index"));
        }

        // A5
        [Fact]
        public async Task The_public_honey_story_serves_only_its_own_photos_without_metadata()
        {
            var s = await SeedAsync();
            var visitor = UserClient.Anonymous(_app);
            var response = await visitor.GetAsync($"/honeystory/photo/{s.Photo}?qr={s.SealedQr}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
            using (var image = Image.Load(await response.Content.ReadAsByteArrayAsync()))
                Assert.Null(image.Metadata.ExifProfile);
            Assert.Equal(404, await visitor.StatusAsync($"/honeystory/photo/{s.Photo}"));
            Assert.Equal(404, await visitor.StatusAsync($"/honeystory/photo/{s.Photo}?qr={s.OtherQr}"));
            Assert.Contains($"/honeystory/photo/{s.Photo}?qr={s.SealedQr}", await visitor.HtmlAsync($"/honeystory/{s.SealedQr}"));
        }

        // A7, A6
        [Fact]
        public async Task The_lock_screen_is_gone()
        {
            var s = await SeedAsync();
            var staff = await MemberAsync("e2e_d75_lock_" + s.Suffix, s.Processor, OrgRoles.Staff);
            var response = await staff.GetAsync("/Account/LockScreen");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/MyWork/Index", response.Headers.Location?.ToString());
            Assert.DoesNotContain("LockScreen", await staff.HtmlAsync("/MyWork/Index"));
        }

        // A8
        [Fact]
        public async Task An_administrator_unlocks_an_account_and_deletions_are_audited()
        {
            var suffix = Random.Shared.Next(10000, 99999).ToString(CultureInfo.InvariantCulture);
            var adminName = "e2e_d75_admin_" + suffix;
            var adminId = await _app.CreateUserAsync(adminName, "Administrator");
            var admin = await UserClient.LoginAsync(_app, adminName);
            var lockedId = await _app.CreateUserAsync("e2e_d75_locked_" + suffix);
            using (var scope = _app.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                await users.SetLockoutEndDateAsync((await users.FindByIdAsync(lockedId))!, DateTimeOffset.UtcNow.AddMinutes(10));
            }
            Assert.Contains("unlock-user", await admin.HtmlAsync("/UserManagement/Index"));
            Assert.Contains("Account unlocked.", await admin.PostFlashAsync($"/UserManagement/Unlock/{lockedId}", new(), "/UserManagement/Index"));
            using (var scope = _app.Services.CreateScope())
                Assert.Null((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(lockedId))!.LockoutEnd);

            Assert.Contains("You cannot delete your own account.", await admin.PostFlashAsync("/UserManagement/Delete", new() { ["id"] = adminId }, "/UserManagement/Index"));
            Assert.Contains("User deleted.", await admin.PostFlashAsync("/UserManagement/Delete", new() { ["id"] = lockedId }, "/UserManagement/Index"));
            var audit = await _app.WithContextAsync(db => db.AuditEntries.Where(a => a.Entity == "AspNetUsers" && a.EntityKey == lockedId).Select(a => a.Action).ToListAsync());
            Assert.Contains("Unlock", audit);
            Assert.Contains("Delete", audit);
        }

        // A9, A12
        [Fact]
        public async Task Manage_batch_explains_what_it_refuses_and_takes_the_lots_of_its_plant()
        {
            var s = await SeedAsync();
            var name = "e2e_d75_batch_" + s.Suffix;
            await _app.CreateUserAsync(name, "Administrator");
            var admin = await UserClient.LoginAsync(_app, name);
            var page = $"/FinalHoneyProductions/ManageBatch/{s.OpenBatch}";

            var html = await admin.HtmlAsync(page);
            Assert.Contains($"name=\"usedAmounts[{s.LotNoWholesaler}]\"", html);         // a lot without a wholesaler, owned by the linked plant
            Assert.DoesNotContain($"name=\"usedAmounts[{s.LotOtherWholesaler}]\"", html);
            Assert.Contains($"class=\"alert alert-warning unlinked-owner\" data-org=\"{s.Unlinked}\"", html);
            Assert.Contains($"/Organizations/Edit/{s.Unlinked}", html);

            var refused = await admin.PostFlashAsync("/FinalHoneyProductions/ManageBatchPost",
                new() { ["batchId"] = s.OpenBatch.ToString(), [$"usedAmounts[{s.LotOtherWholesaler}]"] = "5", [$"usedAmounts[{s.LotNoWholesaler}]"] = "999" }, page);
            Assert.Contains($"Lot #{s.LotOtherWholesaler} cannot be used in this batch", refused);
            Assert.Contains($"Lot #{s.LotNoWholesaler}: at most 50 kg is available.", refused);
            Assert.Contains("Nothing was saved.", refused);
            Assert.False(await _app.WithContextAsync(db => db.FinalHoneyProductionDetails.AnyAsync(d => d.FinalHoneyProductionId == s.OpenBatch)));

            Assert.Contains("Batch composition saved: 20 kg in total.", await admin.PostFlashAsync("/FinalHoneyProductions/ManageBatchPost",
                new() { ["batchId"] = s.OpenBatch.ToString(), [$"usedAmounts[{s.LotNoWholesaler}]"] = "20" }, page));

            var ccp4 = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 4").Select(c => c.Id).FirstAsync());
            var response = await admin.PostPairsAsync("/FinalHoneyProductions/ManageCCP",
                new[] { new KeyValuePair<string, string>("FinalHoneyProductionId", s.OpenBatch.ToString()), new("SelectedCCPIds", ccp4.ToString()) },
                $"/FinalHoneyProductions/ManageCCP/{s.OpenBatch}");
            Assert.Contains("CCP assignments saved: 1 added, 0 removed.", await admin.FlashAfterAsync(response));
        }

        // A10
        [Fact]
        public async Task A_lab_draft_cannot_be_saved_for_a_lot_in_a_finalized_batch()
        {
            var s = await SeedAsync();
            var analyst = await MemberAsync("e2e_d75_analyst_" + s.Suffix, s.Lab, OrgRoles.Analyst);
            var response = await analyst.PostMultipartAsync($"/LabReports/Save/{s.Sample}", new() { ["reportNumber"] = "D75" }, null, $"/LabReports/Edit/{s.Sample}");
            Assert.Contains("The lot is locked in a finalized batch", await analyst.FlashAfterAsync(response));
            Assert.False(await _app.WithContextAsync(db => db.LabReports.AnyAsync(r => r.SampleId == s.Sample)));
        }

        // A11
        [Fact]
        public async Task A_suspended_organization_looks_different_from_a_pending_one()
        {
            var suffix = Random.Shared.Next(10000, 99999).ToString(CultureInfo.InvariantCulture);
            var suspended = await _app.CreateOrgAsync("D75 Suspended " + suffix, null, (Capabilities.Processing, null));
            var pending = await _app.CreateOrgAsync("D75 Pending " + suffix, null, (Capabilities.Processing, null));
            await _app.WithContextAsync(async db =>
            {
                (await db.Organizations.FirstAsync(o => o.OrgId == suspended)).Status = OrgStatus.Suspended;
                (await db.Organizations.FirstAsync(o => o.OrgId == pending)).Status = OrgStatus.Pending;
                return await db.SaveChangesAsync();
            });
            var a = await MemberAsync("e2e_d75_susp_" + suffix, suspended, OrgRoles.Staff);
            var b = await MemberAsync("e2e_d75_pend_" + suffix, pending, OrgRoles.Staff);
            var suspendedPage = await a.HtmlAsync("/MyWork/Index");
            Assert.Contains("role-chip is-suspended", suspendedPage);
            Assert.Contains("Your organization is suspended.", suspendedPage);
            var pendingPage = await b.HtmlAsync("/MyWork/Index");
            Assert.Contains("role-chip is-pending", pendingPage);
            Assert.DoesNotContain("Your organization is suspended.", pendingPage);
        }

        // A1-A13: every new text exists in the five languages; the outdated sign-in text is gone; "To" is translated
        [Fact]
        public void The_new_texts_exist_in_every_language()
        {
            var keys = new[]
            {
                "Login.NoPanelAccess", "Org.SuspendedBanner", "Suspended organization", "Protected", "Unlock", "Account unlocked.", "User deleted.",
                "You cannot delete your own account.", "Nothing was saved.", "Lot #{0}: at most {1} kg is available.", "CCP assignments saved: {0} added, {1} removed.",
                "Batch composition saved: {0} kg in total.", "Four-eyes: you marked this CCP \"Not applicable\", so another person must approve it.",
                "This CCP is marked \"Not applicable\". Remove that mark first to record values.", "\"Not applicable\" was removed; values can be recorded again.",
                "{0}: enter text, not a number.", "{0}: enter either a number or a text, not both.", "The note can be at most 500 characters.", "Lot {0} / {1} (#{2})",
                "The password must be at least {0} characters long.", "The link is invalid or has expired. Request a new password reset.", "Allowed: {0}"
            };
            foreach (var lang in new[] { "tr", "es", "pl", "lv" })
            {
                var table = Resx(lang);
                foreach (var key in keys)
                    Assert.True(table.TryGetValue(key, out var value) && value.Length > 0, $"{lang}: {key}");
                Assert.NotEqual("To", table["Cust.To"]);
            }
            foreach (var lang in new[] { "en", "tr", "es", "pl", "lv" })
                Assert.DoesNotMatch("(Only administrators|Yalnızca yöneticiler|Solo los administradores|Zalogować się mogą tylko administratorzy|Pieteikties var tikai administratori)", Resx(lang)["Login.NoPanelAccess"]);
        }

        private static Dictionary<string, string> Resx(string lang)
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null && !Directory.Exists(Path.Combine(dir, "TOP4HoneyChains.Web")))
                dir = Path.GetDirectoryName(dir);
            var doc = System.Xml.Linq.XDocument.Load(Path.Combine(dir!, "TOP4HoneyChains.Web", "Resources", $"SharedResource.{lang}.resx"));
            return doc.Root!.Elements("data").ToDictionary(d => (string)d.Attribute("name")!, d => (string?)d.Element("value") ?? "");
        }
    }
}
