using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-51: translation of a reference-data text (honey type, CCP title, parameter name, …) for one language.
    // The base column of the reference table is the fallback.
    public class ReferenceTranslation
    {
        [StringLength(40)]
        public string Entity { get; set; } = null!;

        [StringLength(100)]
        public string EntityKey { get; set; } = null!;

        [StringLength(40)]
        public string Field { get; set; } = null!;

        [StringLength(10)]
        public string Culture { get; set; } = null!;

        public string Value { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedByUserId { get; set; }
    }

    // Entity and field names used in ReferenceTranslations
    public static class RefEntity
    {
        public const string HoneyType = "HoneyType";
        public const string ProductionPeriod = "ProductionPeriod";
        public const string EducationLevel = "EducationLevel";
        public const string BeekeepingPurposeType = "BeekeepingPurposeType";
        public const string BeekeepingType = "BeekeepingType";
        public const string AssignmentStatus = "AssignmentStatus";
        public const string Capability = "Capability";
        public const string OrgRole = "OrgRole";
        public const string Ccp = "Ccp";
        public const string ParameterCategory = "ParameterCategory";
        public const string Parameter = "Parameter";
        public const string ParameterLimit = "ParameterLimit";
        public const string ParameterContent = "ParameterContent";
        public const string Country = "Country";

        // entity -> translatable fields, for the translation screen
        public static readonly IReadOnlyDictionary<string, string[]> Fields = new Dictionary<string, string[]>
        {
            [HoneyType] = new[] { "Title", "Description" },
            [ProductionPeriod] = new[] { "Title" },
            [EducationLevel] = new[] { "Title" },
            [BeekeepingPurposeType] = new[] { "Title" },
            [BeekeepingType] = new[] { "Title" },
            [AssignmentStatus] = new[] { "Name" },
            [Capability] = new[] { "Name", "Description" },
            [OrgRole] = new[] { "Name" },
            [Ccp] = new[] { "Title", "ProcessingStep", "CriticalityJustification" },
            [ParameterCategory] = new[] { "Name", "Description" },
            [Parameter] = new[] { "Name", "Unit", "LimitNote", "LimitSource" },
            [ParameterLimit] = new[] { "Note" },
            [ParameterContent] = new[] { "WhatIsIt", "WhyImportant", "ExtraInfo" },
            [Country] = new[] { "Name" }
        };
    }
}
