using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-98: the seal's short address in every code field, the closures of a new delivery and the state of a container at emptying
    public class D98SealRulesTests
    {
        [Theory]
        [InlineData("TS-000123", "TS-000123")]
        [InlineData(" ts-000123 ", "TS-000123")]
        [InlineData("https://top4.example.invalid/s/TS-000123", "TS-000123")]
        [InlineData("http://localhost:5255/s/tk-0007/", "TK-0007")]
        [InlineData("https://top4.example.invalid/s/ZZ-0042?utm=x", "ZZ-0042")]
        [InlineData("https://top4.example.invalid/s/ZZ-0042#top", "ZZ-0042")]
        [InlineData("   ", null)]
        public void A_scanned_short_address_becomes_the_code(string raw, string? code) => Assert.Equal(code, SealPoolService.Normalize(raw));

        [Fact]
        public void The_short_path_is_built_from_the_code()
        {
            Assert.Equal("/s/TS-000123", SealPoolService.ShortPath("TS-000123"));
            Assert.Equal("TS-000123", SealPoolService.Normalize("https://x.example.invalid" + SealPoolService.ShortPath("TS-000123")));
        }

        [Fact]
        public void New_deliveries_close_the_beekeepers_container_with_a_seal_label_only()
        {
            Assert.Equal(new[] { ClosureKind.SealLabel, ClosureKind.OrgContainer }, ClosureKind.New);
            Assert.Contains(ClosureKind.PlasticSeal, ClosureKind.All);   // deliveries recorded before keep it
            Assert.Equal(new[] { SealKind.Label }, SealPoolService.ClosingKinds(ClosureKind.SealLabel));
            Assert.Null(SealPoolService.ClosingKinds(ClosureKind.OrgContainer));   // a seal label or a plastic seal
        }

        [Fact]
        public void A_failed_check_stays_visible_until_the_container_is_emptied()
        {
            var c = new Container { ContainerId = 1, ContainerCode = "L1-C1" };
            ContainerEmptying Check(string outcome) => new() { ContainerId = 1, Outcome = outcome, ExpectedSeal = "TS-000001" };
            var failed = new ContainerEmptyingService.Row(c, "TS-000001", null, new[] { Check(EmptyingOutcome.Mismatch) });
            Assert.True(failed.Sealed);
            Assert.Equal(EmptyingOutcome.Mismatch, failed.Failed?.Outcome);
            var emptied = Check(EmptyingOutcome.Emptied);
            var done = new ContainerEmptyingService.Row(c, null, emptied, new[] { Check(EmptyingOutcome.FoundBroken), emptied });
            Assert.False(done.Sealed);
            Assert.Null(done.Failed);
            Assert.False(new ContainerEmptyingService.Row(c, null, null, Array.Empty<ContainerEmptying>()).Sealed);   // no closing seal on record
        }
    }
}
