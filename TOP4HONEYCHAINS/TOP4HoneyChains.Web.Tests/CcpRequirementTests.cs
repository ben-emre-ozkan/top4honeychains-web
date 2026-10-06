using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-62: a follow-up question is required only when its yes/no question has the given answer
    public class CcpRequirementTests
    {
        private static readonly CriticalControlPointParameter FollowUp = new() { ParameterId = 8, IsRequired = true, RequiredIfParameterId = 7, RequiredIfValue = "Yes" };

        [Theory]
        [InlineData("Yes", true)]
        [InlineData("yes ", true)]
        [InlineData("No", false)]
        [InlineData(null, false)]   // the question is not answered yet: the follow-up is not required on its own
        public void Follow_up_required_only_for_the_given_answer(string? answer, bool required)
        {
            Assert.Equal(required, CcpRequirement.IsRequired(FollowUp, id => id == 7 ? answer : null));
        }

        [Fact]
        public void Plain_parameters_keep_their_flag()
        {
            Assert.True(CcpRequirement.IsRequired(new CriticalControlPointParameter { IsRequired = true }, _ => null));
            Assert.False(CcpRequirement.IsRequired(new CriticalControlPointParameter { IsRequired = false }, _ => null));
            Assert.False(CcpRequirement.IsRequired(new CriticalControlPointParameter { IsRequired = false, RequiredIfParameterId = 7, RequiredIfValue = "Yes" }, _ => "Yes"));
        }

        [Fact]
        public void Answers_come_from_the_records_logs()
        {
            var answers = CcpRequirement.Answers(new[]
            {
                new CriticalControlPointMonitoringLog { ParameterId = 7, MeasuredTextValue = "Yes" },
                new CriticalControlPointMonitoringLog { ParameterId = null, MeasuredTextValue = "x" }
            });
            Assert.Equal("Yes", answers(7));
            Assert.Null(answers(8));
        }
    }
}
