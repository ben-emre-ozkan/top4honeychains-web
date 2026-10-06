using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // D-62: a parameter marked required can be required only when another parameter of the same CCP has a given
    // answer (e.g. "If yes, which methods…?" only when the yes/no question before it is "Yes")
    public static class CcpRequirement
    {
        // answerOf: the recorded text answer of a parameter of the same record (null when not answered)
        public static bool IsRequired(CriticalControlPointParameter p, Func<int, string?> answerOf) =>
            p.IsRequired && (p.RequiredIfParameterId == null
                             || string.Equals(answerOf(p.RequiredIfParameterId.Value)?.Trim(), p.RequiredIfValue?.Trim(), StringComparison.OrdinalIgnoreCase));

        public static Func<int, string?> Answers(IEnumerable<CriticalControlPointMonitoringLog> logs)
        {
            var map = logs.Where(l => l.ParameterId != null).GroupBy(l => l.ParameterId!.Value)
                .ToDictionary(g => g.Key, g => g.First().MeasuredTextValue);
            return id => map.GetValueOrDefault(id);
        }
    }
}
