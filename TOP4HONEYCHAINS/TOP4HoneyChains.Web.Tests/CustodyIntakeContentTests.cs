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

        private static List<string> Check(params ContainerRow[] rows) =>
            new CustodyIntakeService(null!, TimeProvider.System).CheckContainers(new IntakeForm { Containers = rows.ToList() });

        private static ContainerRow Row(string tag, bool sealedOnArrival = true) => new()
        {
            Kind = ContainerKind.Tin, NetKg = 25, GrossKg = 26, ArrivedSealed = sealedOnArrival, OriginSeal = sealedOnArrival ? tag + "-O" : null, OriginSealIntact = sealedOnArrival,
            PlantSeal = sealedOnArrival ? null : tag + "-P", JarSeal = tag + "-J", CounterSeal = tag + "-K", CapIntact = true, Reseal = tag + "-R"
        };

        [Fact]
        public void Container_rows_need_weight_seals_an_intact_cap_and_different_seal_numbers()
        {
            Assert.Empty(Check(Row("A"), Row("B", sealedOnArrival: false)));
            Assert.Contains("Record at least one container.", Check());
            var popped = Row("A");
            popped.CapIntact = false;
            Assert.Contains(Check(popped), e => e.StartsWith("Container 1: the jar's cap must be intact"));
            var light = Row("A");
            light.NetKg = 0;
            Assert.Contains("Container 1: enter the net weight (more than 0 kg).", Check(light));
            var heavy = Row("A");
            heavy.GrossKg = 10;
            Assert.Contains("Container 1: the gross weight cannot be lower than the net weight.", Check(heavy));
            var unsealed = Row("B", sealedOnArrival: false);
            unsealed.PlantSeal = " ";
            Assert.Contains("Container 2: apply a plant seal in front of the beekeeper and enter its number.", Check(Row("A"), unsealed));
            var twin = Row("B");
            twin.JarSeal = "a-j";   // case does not make a seal different
            Assert.Contains("Every seal number of the delivery must be different.", Check(Row("A"), twin));
            Assert.Contains("At most 100 containers per delivery.", Check(Enumerable.Range(0, 101).Select(i => Row("T" + i)).ToArray()));
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
