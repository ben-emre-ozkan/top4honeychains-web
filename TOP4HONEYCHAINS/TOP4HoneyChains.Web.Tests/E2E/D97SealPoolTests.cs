using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-97 (migration 033): the seal pool of an organization (seal labels issued by the platform, purchased plastic seal ranges),
    // its permanent containers, the three ways to close a container, the jar cap types and every refusal of a seal code.
    public partial class CustodyIntakeTests
    {
        private async Task<UserClient> OrgAdminAsync(Plant p)
        {
            await _app.AddMemberAsync(p.OrgId, await _app.CreateUserAsync($"zz_test_{p.Tag}_oa"), OrgRoles.OrgAdmin);
            return await UserClient.LoginAsync(_app, $"zz_test_{p.Tag}_oa");
        }

        [Fact]
        public async Task The_administrator_issues_seal_labels_adds_plastic_seals_and_registers_containers()
        {
            var p = await PlantAsync("sp1");
            var oa = await OrgAdminAsync(p);
            Assert.Contains("id=\"issue-form\"", await oa.HtmlAsync("/Seals/Index"));

            // seal labels: the next free platform numbers, printable on A4 or 100 x 50 mm labels with a QR code
            var issued = await oa.PostFlashAsync("/Seals/Issue", new() { ["count"] = "3", ["note"] = "zz_test" }, "/Seals/Index");
            Assert.Contains("3 seal labels issued", issued);
            var batch = await _app.WithContextAsync(db => db.SealBatches.Include(b => b.Codes).Where(b => b.OrgId == p.OrgId && b.Kind == SealKind.Label && b.Note == "zz_test").SingleAsync());
            Assert.Equal(3, batch.Codes.Count);
            Assert.All(batch.Codes, c => Assert.Matches("^TS-\\d{6}$", c.Code));
            Assert.Contains($"Print them on VOID label stock", issued);
            var print = await oa.HtmlAsync($"/Seals/Print?batchId={batch.BatchId}&size=label");
            Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(print, "class=\"label seal-label ").Count);
            Assert.Contains("@page { size: 100mm 50mm", print);
            Assert.Contains("<svg", print);
            Assert.Contains(batch.FirstCode, print);
            Assert.Equal("Issue between 1 and 500 seal labels at a time.", await oa.PostFlashAsync("/Seals/Issue", new() { ["count"] = "501" }, "/Seals/Index"));

            // purchased plastic seals: a number range; overlaps and the platform's prefixes are refused
            Assert.Contains("5 plastic seals added", await oa.PostFlashAsync("/Seals/AddRange", new() { ["prefix"] = "zz-sp1-", ["from"] = "0098", ["to"] = "0102" }, "/Seals/Index"));
            Assert.True(await _app.WithContextAsync(db => db.SealCodes.AnyAsync(c => c.Code == "ZZ-SP1-0102" && c.Kind == SealKind.Plastic && c.OrgId == p.OrgId)));
            Assert.Equal("Some of these seals are already in a seal pool (first: ZZ-SP1-0100).",
                await oa.PostFlashAsync("/Seals/AddRange", new() { ["prefix"] = "ZZ-SP1-", ["from"] = "0100", ["to"] = "0110" }, "/Seals/Index"));
            Assert.Equal("TS- and TK- are kept for the platform's seal labels and containers.",
                await oa.PostFlashAsync("/Seals/AddRange", new() { ["prefix"] = "TS-", ["from"] = "1", ["to"] = "2" }, "/Seals/Index"));
            Assert.Equal("The last number cannot be lower than the first.", await oa.PostFlashAsync("/Seals/AddRange", new() { ["from"] = "9", ["to"] = "2" }, "/Seals/Index"));

            // a permanent container: code TK-nnnn, its label and its page
            Assert.Contains("registered", await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Drum, ["name"] = "zz_test drum", ["capacityKg"] = "300", ["tareKg"] = "18.5" }, "/Seals/Index"));
            var box = await _app.WithContextAsync(db => db.OrgContainers.SingleAsync(b => b.OrgId == p.OrgId));
            Assert.Matches("^TK-\\d{4}$", box.Code);
            Assert.Contains(box.Code, await oa.HtmlAsync("/Seals/Print?what=containers"));
            Assert.Contains(box.Code, await oa.HtmlAsync($"/Seals/Box/{box.OrgContainerId}"));

            // the sample acceptance officer sees the pool but cannot issue; another organization sees nothing of it
            var page = WebUtility.HtmlDecode(await p.Sao.HtmlAsync("/Seals/Index"));
            Assert.Contains("id=\"seal-admin-only\"", page);
            Assert.Contains("ZZ-SP1-0098", page);
            Assert.Contains("/Account/AccessDenied", (await p.Sao.PostAsync("/Seals/Issue", new() { ["count"] = "5" }, "/Seals/Index")).Headers.Location?.ToString());
            var other = await PlantAsync("sp1x");
            Assert.Equal(404, await other.Sao.StatusAsync($"/Seals/Box/{box.OrgContainerId}"));
            Assert.Equal(404, await other.Sao.StatusAsync($"/Seals/Print?batchId={batch.BatchId}"));
            Assert.DoesNotContain("ZZ-SP1-0098", await other.Sao.HtmlAsync("/Seals/Index"));
        }

        [Fact]
        public async Task The_three_closures_and_the_tamper_ring_cap_are_recorded_and_bound_by_the_code() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("sp2");
            await SealPoolAsync(_app, p.OrgId, SealKind.Plastic, new[] { "SP2-P1", "SP2-P2" });
            var oa = await OrgAdminAsync(p);
            await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Bucket, ["name"] = "zz_test bucket", ["tareKg"] = "1.2" }, "/Seals/Index");
            await oa.PostFlashAsync("/Seals/Register", new() { ["kind"] = ContainerKind.Drum, ["name"] = "zz_test drum" }, "/Seals/Index");
            var boxes = await _app.WithContextAsync(db => db.OrgContainers.Where(b => b.OrgId == p.OrgId).OrderBy(b => b.OrgContainerId).ToListAsync());
            var (box, drum) = (boxes[0], boxes[1]);

            var form = Form(p, 3, (i, f) =>
            {
                var c = $"Containers[{i}].";
                // D-98: a plastic seal closes only an association container (through the eyelet that ties the lid to the body)
                if (i == 1) { f[c + "Closure"] = ClosureKind.OrgContainer; f[c + "OrgContainerCode"] = drum.Code; f[c + "ClosingSeal"] = "sp2-p1"; }
                if (i == 2) { f[c + "Closure"] = ClosureKind.OrgContainer; f[c + "OrgContainerCode"] = box.Code.ToLowerInvariant(); f[c + "Kind"] = ContainerKind.Tin; }
            });
            form["CapType"] = JarCap.TamperRing;
            // the live check answers the form while a code is typed or scanned
            var ok = JsonDocument.Parse(await p.Sao.HtmlAsync("/Seals/Check?code=sp2-j0&use=jar")).RootElement;
            Assert.True(ok.GetProperty("ok").GetBoolean());
            Assert.Equal("SP2-J0", ok.GetProperty("code").GetString());
            var plasticForLabel = JsonDocument.Parse(await p.Sao.HtmlAsync("/Seals/Check?code=SP2-P2&use=closing&closure=SealLabel")).RootElement;
            Assert.False(plasticForLabel.GetProperty("ok").GetBoolean());
            Assert.Equal("Seal SP2-P2 is a numbered plastic seal; a seal label is needed here.", plasticForLabel.GetProperty("message").GetString());
            var found = JsonDocument.Parse(await p.Sao.HtmlAsync($"/Seals/FindBox?code={box.Code}")).RootElement;
            Assert.Equal(ContainerKind.Bucket, found.GetProperty("kind").GetString());

            var (intakeId, lot) = await ConfirmBySmsAsync(p, form);
            var intake = await _app.WithContextAsync(db => db.LotIntakes.Include(i => i.Containers).ThenInclude(c => c.Samples).FirstAsync(i => i.IntakeId == intakeId));
            Assert.Equal(JarCap.TamperRing, intake.JarCapType);
            var cs = intake.Containers.OrderBy(c => c.Seq).ToList();
            Assert.Equal(new[] { ClosureKind.SealLabel, ClosureKind.OrgContainer, ClosureKind.OrgContainer }, cs.Select(c => c.Closure));
            Assert.Equal(SealKind.Plastic, await _app.WithContextAsync(db => db.SealCodes.Where(s => s.Code == "SP2-P1" && s.ContainerId == cs[1].ContainerId).Select(s => s.Kind).SingleAsync()));
            Assert.Equal((ContainerKind.Bucket, (int?)box.OrgContainerId), (cs[2].Kind, cs[2].OrgContainerId));   // the registered kind wins
            Assert.All(cs, c => Assert.Equal(JarCap.TamperRing, c.Samples.Single().CapType));
            Assert.Contains($"L{lot}-C3", await p.Sao.HtmlAsync($"/Seals/Box/{box.OrgContainerId}"));
            var details = await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}");
            Assert.Contains("id=\"content-verified\"", details);
            Assert.Contains("data-closure=\"OrgContainer\"", details);
            Assert.Contains(box.Code, details);

            // the laboratory asks about the ring, not the button
            var lab = await LabUsersAsync(p);
            var reception = WebUtility.HtmlDecode(await lab.Analyst.HtmlAsync("/Samples/Index"));
            Assert.Contains("The tamper ring is unbroken", reception);
            Assert.Contains("id=\"reception-info\"", reception);
            return 0;
        });

        [Fact]
        public async Task Every_seal_must_come_from_the_pool_unused_and_of_the_right_kind() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("sp3");
            var other = await PlantAsync("sp3x");
            await SealPoolAsync(_app, p.OrgId, SealKind.Plastic, new[] { "SP3-P1" });
            async Task<string> SendAsync(Dictionary<string, string> f) =>
                WebUtility.HtmlDecode(await (await p.Sao.PostAsync("/Intake/Record", Op(f, "send"), $"/Intake/Create?number={p.BusinessNumber}")).Content.ReadAsStringAsync());

            Assert.Contains("Container 1: Seal ZZ-NOWHERE is not in the seal pool of your organization.",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].JarSeal"] = "zz-nowhere")));
            Assert.Contains("Container 1: Seal SP3X-J0 is not in the seal pool of your organization.",   // another organization's seal
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].CounterSeal"] = "sp3x-j0")));
            Assert.Contains("Container 1: Seal SP3-P1 is a numbered plastic seal; a seal label is needed here.",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].JarSeal"] = "sp3-p1")));
            Assert.Contains("Container 1: a numbered plastic seal closes only an association container; close the beekeeper's container with a seal label.",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].Closure"] = ClosureKind.PlasticSeal)));   // D-98
            Assert.Contains("Container 1: Seal SP3-P1 is a numbered plastic seal; a seal label is needed here.",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].ClosingSeal"] = "sp3-p1")));
            Assert.Contains("Container 1: Container TK-9999 is not registered for your organization.",
                await SendAsync(Form(p, 1, (i, f) => { f[$"Containers[{i}].Closure"] = ClosureKind.OrgContainer; f[$"Containers[{i}].OrgContainerCode"] = "TK-9999"; })));
            Assert.Contains("Container 1: choose how the container is closed.",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].Closure"] = "Tape")));
            Assert.Contains("Container 1: enter the code of the seal the field inspector put on at harvest.",
                await SendAsync(Form(p, 1, (i, f) => f.Remove($"Containers[{i}].OriginSeal"))));
            Assert.Contains("Container 1: the tamper ring of the jar's cap must be unbroken.",
                await SendAsync(new Dictionary<string, string>(Form(p, 1, (i, f) => f.Remove($"Containers[{i}].CapIntact"))) { ["CapType"] = JarCap.TamperRing }));
            Assert.Contains("Choose the cap of the sample jars.", await SendAsync(new Dictionary<string, string>(Form(p, 1)) { ["CapType"] = "Cork" }));

            // a code used once is refused for the next delivery, with where it was used
            var (_, lot) = await ConfirmBySmsAsync(p, Form(p, 1));
            Assert.Contains($"Container 1: Seal SP3-J0 was already used on S-{lot}-C1.", await SendAsync(Form(p, 1)));
            Assert.Contains("Container 1: Seal SP3-R0 was already used on L" + lot + "-C1.", await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].JarSeal"] = "sp3-j1")));
            var used = JsonDocument.Parse(await p.Sao.HtmlAsync("/Seals/Check?code=SP3-O0&use=origin")).RootElement;
            Assert.False(used.GetProperty("ok").GetBoolean());

            // nothing went to the beekeeper for the refused forms: one delivery, one code
            Assert.Equal(1, await _app.WithContextAsync(db => db.LotIntakes.CountAsync(i => i.ReceivingOrgId == p.OrgId)));
            Assert.Equal(4, await _app.WithContextAsync(db => db.SealCodes.CountAsync(s => s.OrgId == p.OrgId && s.Status != SealStatus.Issued)));
            return 0;
        });
    }
}
