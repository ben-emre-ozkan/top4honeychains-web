using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-82: in per-container mode the lot's CCP 1 value of a parameter is the worst container value (the conservative one for limits)
    public class WorstValueTests
    {
        private static readonly IReadOnlyDictionary<int, decimal> None = new Dictionary<int, decimal>();

        private static CriticalControlPointParameter P(decimal? min, decimal? max) => new()
        {
            ParameterId = 1, ParameterName = "p", DataType = CcpDataType.Number, MinValue = min, MaxValue = max
        };

        private static decimal Worst(CriticalControlPointParameter p, params decimal[] values)
        {
            var list = values.Select((v, i) => new WorstValue.Candidate($"C{i + 1}", v, null, None)).ToList();
            return list[WorstValue.Pick(p, list, HoneyProfile.Unknown)!.Value].Number!.Value;
        }

        [Theory]
        [InlineData(18, 19.5, 19.5)]   // max 20 (moisture): the highest inside
        [InlineData(21, 18, 21)]       // a value outside beats one inside
        [InlineData(21, 22, 22)]       // the furthest outside
        public void Under_a_maximum_the_highest_value_is_the_worst(decimal a, decimal b, decimal expected) => Assert.Equal(expected, Worst(P(null, 20), a, b));

        [Theory]
        [InlineData(12, 9, 9)]         // min 8 (diastase): the lowest inside
        [InlineData(7, 9, 7)]
        [InlineData(7, 6, 6)]
        public void Under_a_minimum_the_lowest_value_is_the_worst(decimal a, decimal b, decimal expected) => Assert.Equal(expected, Worst(P(8, null), a, b));

        [Theory]
        [InlineData(3.9, 4.4, 4.4)]    // pH 3.2-4.5: the smallest margin
        [InlineData(3.3, 3.9, 3.3)]
        [InlineData(4.4, 4.7, 4.7)]
        [InlineData(3.0, 4.6, 3.0)]    // 0.2 below beats 0.1 above
        public void Between_two_limits_the_smallest_margin_is_the_worst(decimal a, decimal b, decimal expected) => Assert.Equal(expected, Worst(P(3.2m, 4.5m), a, b));

        [Fact]
        public void Without_a_limit_the_highest_value_is_taken_and_a_tie_keeps_the_first_container()
        {
            Assert.Equal(33m, Worst(P(null, null), 30, 33, 31));
            var list = new[] { new WorstValue.Candidate("C1", 5, null, None), new WorstValue.Candidate("C2", 5, null, None) };
            Assert.Equal(0, WorstValue.Pick(P(null, 20), list, HoneyProfile.Unknown));
        }

        [Fact]
        public void Texts_are_all_kept_with_their_container()
        {
            Assert.Equal("typical", WorstValue.CombineTexts(new[] { new WorstValue.Candidate("C1", null, "typical", None), new WorstValue.Candidate("C2", null, "Typical", None) }));
            Assert.Equal("C1: typical; C2: fermented", WorstValue.CombineTexts(new[] { new WorstValue.Candidate("C1", null, "typical", None), new WorstValue.Candidate("C2", null, "fermented", None) }));
            Assert.Null(WorstValue.CombineTexts(new[] { new WorstValue.Candidate("C1", 1, null, None) }));
        }
    }
}
