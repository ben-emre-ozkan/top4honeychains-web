using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P2 (D78-SPEC 3.9-3.10, 4.4-4.6): what the beekeeper confirms (canonical content and its hash), the container checks,
    // the seal on a container and the laboratory's reception checks.
    public class CustodyIntakeContentTests
    {
        private static ContainerContent Tin(string tag, decimal net = 25m) => new("Tin", net, net + 1.5m, $"{tag}-O", true, null, $"{tag}-J", $"{tag}-K", true, $"{tag}-R");

        private static IntakeContent Content(params ContainerContent[] containers) =>
            new("bk-1", 7, 3, 2, "TR", new DateOnly(2026, 10, 27), "Giresun", "Bulancak", 11, "cold room", new DateOnly(2028, 10, 27), containers);


        [Fact]
        public void The_canonical_content_lists_every_container_with_all_its_seal_numbers()
        {
            var c = Content(Tin("A"), Tin("B", 24.5m));
            Assert.Equal("intake|bk-1|7|3|2|TR|2026-10-27|Giresun|Bulancak|11|cold room|2028-10-27"
                + "|C1:Tin;25;26.5;A-O;1;;A-J;A-K;1;A-R|C2:Tin;24.5;26;B-O;1;;B-J;B-K;1;B-R", c.Canonical());
            Assert.Equal(49.5m, c.TotalKg);
            Assert.Equal(64, c.Hash().Length);
            Assert.Equal(c.Hash(), Content(Tin("A"), Tin("B", 24.5m)).Hash());
        }

        [Theory]
        [InlineData("jar")]
        [InlineData("counter")]
        [InlineData("reseal")]
        [InlineData("origin")]
        [InlineData("weight")]
        [InlineData("order")]
        public void Any_change_of_a_seal_number_a_weight_or_the_order_changes_the_hash(string change)
        {
            var original = Content(Tin("A"), Tin("B"));
            var a = Tin("A");
            var changed = change switch
            {
                "jar" => Content(a with { JarSeal = "X" }, Tin("B")),
                "counter" => Content(a with { CounterSeal = "X" }, Tin("B")),
                "reseal" => Content(a with { Reseal = "X" }, Tin("B")),
                "origin" => Content(a with { OriginSeal = "X" }, Tin("B")),
                "weight" => Content(a with { Net = 25.01m }, Tin("B")),
                _ => Content(Tin("B"), a)
            };
            Assert.NotEqual(original.Hash(), changed.Hash());
        }

        // D-97: a delivery recorded since D-97 binds the jar cap, the closure, the association container's code and one closing seal
        private static ContainerContent Labelled(string tag, string closure = ClosureKind.SealLabel, string? box = null) =>
            new("Tin", 25m, 26.5m, null, null, null, $"{tag}-J", $"{tag}-K", true, $"{tag}-C", closure, box);

        [Fact]
        public void A_d97_delivery_binds_the_cap_the_closure_and_every_code_and_old_deliveries_keep_their_text()
        {
            var v2 = Content(Labelled("A"), Labelled("B", ClosureKind.OrgContainer, "TK-0001")) with { CapType = JarCap.SafetyButton };
            Assert.Equal("intake2|bk-1|7|3|2|TR|2026-10-27|Giresun|Bulancak|11|cold room|2028-10-27|SafetyButton"
                + "|C1:SealLabel;;Tin;25;26.5;;;A-J;A-K;1;A-C|C2:OrgContainer;TK-0001;Tin;25;26.5;;;B-J;B-K;1;B-C", v2.Canonical());
            Assert.NotEqual(v2.Hash(), (v2 with { CapType = JarCap.TamperRing }).Hash());
            Assert.NotEqual(v2.Hash(), (Content(Labelled("A"), Labelled("B", ClosureKind.OrgContainer, "TK-0002")) with { CapType = JarCap.SafetyButton }).Hash());
            Assert.NotEqual(v2.Hash(), (Content(Labelled("A", ClosureKind.PlasticSeal), Labelled("B", ClosureKind.OrgContainer, "TK-0001")) with { CapType = JarCap.SafetyButton }).Hash());
            // a delivery recorded before D-97 (no cap type) keeps the canonical text the beekeeper confirmed
            Assert.StartsWith("intake|", Content(Tin("A")).Canonical());
        }

        [Theory]
        [InlineData(" ts-000123 ", "TS-000123")]
        [InlineData("https://example.invalid/s/TS-000124", "TS-000124")]
        [InlineData("SAB-P-35 001", "SAB-P-35001")]
        [InlineData("   ", null)]
        public void A_typed_or_scanned_code_is_normalized(string raw, string? code) => Assert.Equal(code, SealPoolService.Normalize(raw));

        [Fact]
        public void A_seal_must_be_in_the_pool_unused_and_of_the_right_kind()
        {
            var pools = new[] { 5 };
            var free = new SealCode { Code = "TS-000001", OrgId = 5, Kind = SealKind.Label, Status = SealStatus.Issued };
            Assert.Null(SealPoolService.Refusal("TS-000001", free, pools, null));
            Assert.Equal("Seal TS-000009 is not in the seal pool of your organization.", SealPoolService.Refusal("TS-000009", null, pools, null));
            Assert.Equal("Seal TS-000001 is not in the seal pool of your organization.", SealPoolService.Refusal("TS-000001", free, new[] { 6 }, null));
            var used = new SealCode { Code = "TS-000002", OrgId = 5, Kind = SealKind.Label, Status = SealStatus.Used, Container = new Container { ContainerCode = "L9-C1" } };
            Assert.Equal("Seal TS-000002 was already used on L9-C1.", SealPoolService.Refusal("TS-000002", used, pools, null));
            var plastic = new SealCode { Code = "P-1", OrgId = 5, Kind = SealKind.Plastic, Status = SealStatus.Issued };
            Assert.Equal("Seal P-1 is a numbered plastic seal; a seal label is needed here.", SealPoolService.Refusal("P-1", plastic, pools, SealPoolService.ClosingKinds(ClosureKind.SealLabel)));
            Assert.Equal("Seal TS-000001 is a seal label; a numbered plastic seal is needed here.", SealPoolService.Refusal("TS-000001", free, pools, SealPoolService.ClosingKinds(ClosureKind.PlasticSeal)));
            // the association's container takes either kind
            Assert.Null(SealPoolService.Refusal("P-1", plastic, pools, SealPoolService.ClosingKinds(ClosureKind.OrgContainer)));
        }

        private static ContainerSealEvent E(int id, string ev, string seal) => new() { EventId = id, Event = ev, SealNumber = seal };

        [Fact]
        public void The_current_seal_is_the_last_one_applied_and_not_broken_since()
        {
            Assert.Equal("R", CustodyIntakeService.CurrentSeal(new[] { E(1, SealEvent.Checked, "O"), E(2, SealEvent.Broken, "O"), E(3, SealEvent.Applied, "R") }));
            Assert.Null(CustodyIntakeService.CurrentSeal(new[] { E(1, SealEvent.Applied, "P"), E(2, SealEvent.Broken, "P") }));
            Assert.Equal("N", CustodyIntakeService.CurrentSeal(new[] { E(3, SealEvent.Applied, "R"), E(1, SealEvent.Checked, "O"), E(2, SealEvent.Broken, "O"), E(4, SealEvent.Broken, "R"), E(5, SealEvent.Applied, "N") }));
        }

        [Fact]
        public void Reception_needs_three_answers_and_a_reason_for_any_no()
        {
            Assert.Null(LabCustodyService.CheckReception(true, true, true, null));
            Assert.StartsWith("Answer all three", LabCustodyService.CheckReception(true, null, true, null));
            Assert.StartsWith("A failed reception check", LabCustodyService.CheckReception(true, true, false, " "));
            Assert.Null(LabCustodyService.CheckReception(false, true, true, "number differs"));
            Assert.StartsWith("The reason can be at most", LabCustodyService.CheckReception(true, false, true, new string('x', 501)));
        }
    }
}
