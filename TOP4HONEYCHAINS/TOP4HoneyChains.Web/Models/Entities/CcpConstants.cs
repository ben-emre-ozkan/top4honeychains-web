namespace TOP4HoneyChains.Web.Models.Entities
{
    // Fixed ids of AssignmentStatuses (rows 1-4 pre-exist, 5 is added by db/migrations/002)
    public static class AssignmentStatusIds
    {
        public const int Assigned = 1;
        public const int InProgress = 2;
        public const int Completed = 3;
        public const int Rejected = 4;
        public const int NotApplicable = 5;

        // A CCP that no longer blocks finalizing
        public static bool IsDone(int status) => status == Completed || status == NotApplicable;
    }

    public static class CcpAppliesTo
    {
        public const string RawLot = "RawLot";
        public const string Batch = "Batch";
    }

    public static class CcpDataType
    {
        public const string Number = "Number";
        public const string YesNo = "YesNo";
        public const string Text = "Text";
        public const string File = "File";

        public static readonly string[] All = { Number, YesNo, Text, File };
    }

    public static class HoneyClasses
    {
        public static readonly string[] All = { "Floral", "Honeydew", "Pine" };
    }
}
