using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Controllers;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-98 (migration 036): plastic seals only on the association's containers, seal labels printed as tear strips with the seal's
    // short address in the QR, the short address itself (members of the issuing organization only), every code field accepting
    // that address, and the seal check when a container is emptied into a batch.
    public partial class CustodyIntakeTests
    {
        private const string Base = "http://localhost";   // App:PublicBaseUrl of the test host

        [Fact]
        public async Task New_deliveries_offer_the_plastic_seal_only_for_association_containers() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("d98a");
            var form = await p.Sao.HtmlAsync($"/Intake/Create?number={p.BusinessNumber}");
            Assert.Contains("id=\"ci-cards\"", form);
            Assert.DoesNotContain("value=\"PlasticSeal\"", form);
            Assert.Contains("value=\"OrgContainer\"", form);
            await SealPoolAsync(_app, p.OrgId, SealKind.Plastic, new[] { "D98A-P1" });
            // the live check: a plastic seal is refused for the beekeeper's container and accepted for an association container
            var own = JsonDocument.Parse(await p.Sao.HtmlAsync("/Seals/Check?code=D98A-P1&use=closing&closure=SealLabel")).RootElement;
            Assert.False(own.GetProperty("ok").GetBoolean());
            var box = JsonDocument.Parse(await p.Sao.HtmlAsync("/Seals/Check?code=D98A-P1&use=closing&closure=OrgContainer")).RootElement;
            Assert.True(box.GetProperty("ok").GetBoolean());
            return 0;
        });

        [Fact]
        public async Task Seal_labels_print_as_tear_strips_with_the_short_address_in_the_QR()
        {
            var p = await PlantAsync("d98b");
            var oa = await OrgAdminAsync(p);
            await oa.PostFlashAsync("/Seals/Issue", new() { ["count"] = "2", ["note"] = "zz_test d98b" }, "/Seals/Index");
            var batch = await _app.WithContextAsync(db => db.SealBatches.Include(b => b.Codes).SingleAsync(b => b.OrgId == p.OrgId && b.Note == "zz_test d98b"));
            foreach (var size in new[] { "a4", "label" })
            {
                var print = await oa.HtmlAsync($"/Seals/Print?batchId={batch.BatchId}&size={size}");
                Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(print, "class=\"label seal-label tear\"").Count);
                Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(print, "class=\"split-label tear-split\"").Count);
                Assert.Contains("<span class=\"s-lid\">lid</span><span class=\"s-bar\">|</span><span class=\"s-body\">body</span>", print);
                foreach (var c in batch.Codes)
                {
                    Assert.Contains(IntakeController.QrSvg($"{Base}/s/{c.Code}"), print);   // the QR holds the short address
                    Assert.DoesNotContain(IntakeController.QrSvg(c.Code), print);
                }
            }
            Assert.Contains("@page { size: 100mm 50mm", await oa.HtmlAsync($"/Seals/Print?batchId={batch.BatchId}&size=label"));
            Assert.Contains("eggshell", WebUtility.HtmlDecode(await oa.HtmlAsync("/Seals/Index")));   // the materials name the label stock
        }

        [Fact]
        public async Task The_short_address_shows_the_seal_to_members_of_the_issuing_organization_only()
        {
            var p = await PlantAsync("d98c");
            var other = await PlantAsync("d98cx");
            var oa = await OrgAdminAsync(p);
            await oa.PostFlashAsync("/Seals/Issue", new() { ["count"] = "1", ["note"] = "zz_test d98c" }, "/Seals/Index");
            await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Drum, ["name"] = "zz_test d98c drum" }, "/Seals/Index");
            var code = await _app.WithContextAsync(db => db.SealCodes.Where(s => s.OrgId == p.OrgId && s.Batch!.Note == "zz_test d98c").Select(s => s.Code).SingleAsync());
            var tk = await _app.WithContextAsync(db => db.OrgContainers.Where(b => b.OrgId == p.OrgId).Select(b => b.Code).SingleAsync());

            // anonymous: the sign-in page, which comes back here
            var anonymous = await UserClient.Anonymous(_app).GetAsync($"/s/{code}");
            Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
            Assert.Contains("/Account/Login", anonymous.Headers.Location!.ToString());
            Assert.Contains(Uri.EscapeDataString($"/s/{code}"), anonymous.Headers.Location!.ToString());

            // a member of the issuing organization: the state of the seal and of the association container
            var page = await p.Sao.HtmlAsync($"/s/{code}");
            Assert.Contains($"<code id=\"seal-code\">{code}</code>", page);
            Assert.Contains("data-status=\"Issued\"", page);
            Assert.Contains("id=\"box-state\" data-in-use=\"false\"", await p.Sao.HtmlAsync($"/s/{tk.ToLowerInvariant()}"));

            // anyone else (signed in elsewhere) gets the sign-in page too, and so does an unknown code: nothing is revealed
            foreach (var url in new[] { $"/s/{code}", $"/s/{tk}" })
            {
                var r = await other.Sao.GetAsync(url);
                Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
                Assert.Contains("/Account/Login", r.Headers.Location!.ToString());
            }
            var unknown = await p.Sao.GetAsync("/s/TS-999999");
            Assert.Equal(HttpStatusCode.Redirect, unknown.StatusCode);
            Assert.Contains("/Account/Login", unknown.Headers.Location!.ToString());
        }

        [Fact]
        public async Task Every_code_field_accepts_the_short_address() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("d98d");
            var oa = await OrgAdminAsync(p);
            await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Bucket, ["name"] = "zz_test d98d bucket" }, "/Seals/Index");
            var tk = await _app.WithContextAsync(db => db.OrgContainers.Where(b => b.OrgId == p.OrgId).Select(b => b.Code).SingleAsync());
            string Url(string c) => Uri.EscapeDataString($"{Base}/s/{c}");

            var check = JsonDocument.Parse(await p.Sao.HtmlAsync($"/Seals/Check?code={Url("D98D-J0")}&use=jar")).RootElement;
            Assert.True(check.GetProperty("ok").GetBoolean());
            Assert.Equal("D98D-J0", check.GetProperty("code").GetString());
            var found = JsonDocument.Parse(await p.Sao.HtmlAsync($"/Seals/FindBox?code={Url(tk)}")).RootElement;
            Assert.Equal(tk, found.GetProperty("code").GetString());
            Assert.Contains("D98D-J1", await p.Sao.HtmlAsync($"/Seals/Index?q={Url("d98d-j1")}"));

            // the delivery form keeps the code of every scanned address
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 2, (i, f) =>
            {
                var c = $"Containers[{i}].";
                f[c + "JarSeal"] = $"{Base}/s/d98d-j{i}";
                f[c + "ClosingSeal"] = $"{Base}/s/D98D-R{i}/";
                if (i == 1) { f[c + "Closure"] = ClosureKind.OrgContainer; f[c + "OrgContainerCode"] = $"{Base}/s/{tk}"; }
            }));
            var stored = await _app.WithContextAsync(db => db.Containers.Include(c => c.Samples).Include(c => c.SealEvents).Where(c => c.IntakeId == intakeId).OrderBy(c => c.Seq).ToListAsync());
            Assert.Equal(new[] { "D98D-J0", "D98D-J1" }, stored.Select(c => c.Samples.Single().SealNumber));
            Assert.Equal(new[] { "D98D-R0", "D98D-R1" }, stored.Select(c => c.SealEvents.Single(e => e.Event == SealEvent.Applied).SealNumber));
            Assert.NotNull(stored[1].OrgContainerId);
            Assert.Contains("id=\"content-verified\"", await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}"));
            return lot;
        });

        private async Task<UserClient> StaffAsync(Plant p)
        {
            await _app.AddMemberAsync(p.OrgId, await _app.CreateUserAsync($"zz_test_{p.Tag}_staff"), OrgRoles.Staff);
            return await UserClient.LoginAsync(_app, $"zz_test_{p.Tag}_staff");
        }

        [Fact]
        public async Task A_container_is_emptied_into_a_batch_only_with_its_intact_closing_seal() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("d98e");
            var oa = await OrgAdminAsync(p);
            await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Drum, ["name"] = "zz_test d98e drum" }, "/Seals/Index");
            var drum = await _app.WithContextAsync(db => db.OrgContainers.SingleAsync(b => b.OrgId == p.OrgId));
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 2, (i, f) =>
            {
                if (i == 1) { f[$"Containers[{i}].Closure"] = ClosureKind.OrgContainer; f[$"Containers[{i}].OrgContainerCode"] = drum.Code; }
            }));
            var containers = await _app.WithContextAsync(db => db.Containers.Where(c => c.IntakeId == intakeId).OrderBy(c => c.Seq).ToListAsync());
            var (c1, c2) = (containers[0], containers[1]);
            var batch = await BatchWithAsync(p.OrgId, lot);
            var staff = await StaffAsync(p);
            var page = $"/OrgWorkspace/Batch/{batch}";
            Dictionary<string, string> Post(Container c, string? seal, bool intact = true, bool broken = false)
            {
                var f = new Dictionary<string, string> { ["batchId"] = batch.ToString(), ["containerId"] = c.ContainerId.ToString() };
                if (seal != null) f["seal"] = seal;
                if (intact) f["intact"] = "true";
                if (broken) f["broken"] = "true";
                return f;
            }

            // both containers are listed as sealed; the closing seal's code is not shown; finalizing is not blocked, only warned
            var html = await staff.HtmlAsync(page);
            Assert.Contains($"data-container=\"L{lot}-C1\" data-state=\"Sealed\"", html);
            Assert.Contains($"data-container=\"L{lot}-C2\" data-state=\"Sealed\"", html);
            Assert.Contains("id=\"emptying-warning\"", html);
            Assert.DoesNotContain("D98E-R0", html);
            Assert.Contains("id=\"box-state\" data-in-use=\"true\"", await oa.HtmlAsync($"/Seals/Box/{drum.OrgContainerId}"));

            // nothing scanned, or intact not confirmed: refused, nothing recorded
            Assert.Equal($"Scan or type the closing seal of container L{lot}-C1.", await staff.PostFlashAsync("/Emptying/Empty", Post(c1, null), page));
            Assert.Equal($"Confirm that the seal of container L{lot}-C1 is intact, or report it broken.", await staff.PostFlashAsync("/Emptying/Empty", Post(c1, "d98e-r0", intact: false), page));
            Assert.Equal(0, await _app.WithContextAsync(db => db.ContainerEmptyings.CountAsync(e => e.FinalHoneyProductionId == batch)));

            // a different code: the step stops and is recorded (here the other container's seal)
            Assert.Equal($"Seal D98E-R1 is not the closing seal of container L{lot}-C1. The container was not emptied; the check was recorded.",
                await staff.PostFlashAsync("/Emptying/Empty", Post(c1, "d98e-r1"), page));
            // a broken seal: the step stops and is recorded
            Assert.Equal($"The seal of container L{lot}-C2 was reported broken. The container was not emptied; the check was recorded.",
                await staff.PostFlashAsync("/Emptying/Empty", Post(c2, null, intact: false, broken: true), page));
            var checks = await _app.WithContextAsync(db => db.ContainerEmptyings.Where(e => e.FinalHoneyProductionId == batch).OrderBy(e => e.EmptyingId).ToListAsync());
            Assert.Equal(new[] { (c1.ContainerId, EmptyingOutcome.Mismatch, "D98E-R0", (string?)"D98E-R1"), (c2.ContainerId, EmptyingOutcome.FoundBroken, "D98E-R1", null) },
                checks.Select(e => (e.ContainerId, e.Outcome, e.ExpectedSeal, e.ScannedSeal)));
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Action == "SealCheckFailed" && a.EntityKey == c1.ContainerId.ToString())));
            Assert.Equal(SealStatus.Used, await _app.WithContextAsync(db => db.SealCodes.Where(s => s.Code == "D98E-R0").Select(s => s.Status).SingleAsync()));
            html = await staff.HtmlAsync(page);
            Assert.Contains($"data-container=\"L{lot}-C1\" data-state=\"Sealed\"", html);   // not emptied
            Assert.Contains("data-outcome=\"Mismatch\"", html);
            Assert.Contains("data-outcome=\"FoundBroken\"", html);
            var details = await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}");
            Assert.Contains("data-outcome=\"Mismatch\"", details);
            Assert.Contains("data-outcome=\"FoundBroken\"", details);

            // only those who enter batch data in the owner organization; another organization does not see the batch at all
            Assert.Equal("Containers are emptied into a batch by those who enter batch data in the organization that owns the batch.",
                await p.Sao.PostFlashAsync("/Emptying/Empty", Post(c1, "d98e-r0"), $"/Intake/Details/{intakeId}"));
            var other = await PlantAsync("d98ex");
            Assert.Equal(HttpStatusCode.NotFound, (await other.Sao.PostAsync("/Emptying/Empty", Post(c1, "d98e-r0"), "/Seals/Index")).StatusCode);

            // the right seal, intact, scanned as its short address: emptied, the seal opened, the association container free again
            Assert.Equal($"Container L{lot}-C1 emptied into batch #{batch}; seal D98E-R0 is now opened.",
                await staff.PostFlashAsync("/Emptying/Empty", Post(c1, $"{Base}/s/D98E-R0"), page));
            Assert.Equal($"Container L{lot}-C2 emptied into batch #{batch}; seal D98E-R1 is now opened.",
                await staff.PostFlashAsync("/Emptying/Empty", Post(c2, "d98e-r1"), page));
            Assert.Equal($"Container L{lot}-C1 was already emptied into batch #{batch}.", await staff.PostFlashAsync("/Emptying/Empty", Post(c1, "D98E-R0"), page));
            Assert.Equal(SealStatus.Broken, await _app.WithContextAsync(db => db.SealCodes.Where(s => s.Code == "D98E-R0").Select(s => s.Status).SingleAsync()));
            var events = await _app.WithContextAsync(db => db.ContainerSealEvents.Where(e => e.ContainerId == c1.ContainerId).OrderBy(e => e.EventId).ToListAsync());
            Assert.Equal((SealEvent.Broken, "D98E-R0", $"Emptied into batch #{batch}"), (events[^1].Event, events[^1].SealNumber, events[^1].Note));
            html = await staff.HtmlAsync(page);
            Assert.Contains($"data-container=\"L{lot}-C1\" data-state=\"Emptied\"", html);
            Assert.DoesNotContain("id=\"emptying-warning\"", html);
            Assert.Contains("id=\"box-state\" data-in-use=\"false\"", await oa.HtmlAsync($"/Seals/Box/{drum.OrgContainerId}"));
            // the delivery the beekeeper confirmed is unchanged
            Assert.Contains("id=\"content-verified\"", await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}"));
            Assert.Contains("data-status=\"Broken\"", await p.Sao.HtmlAsync("/s/D98E-R0"));
            return 0;
        });
    }
}
