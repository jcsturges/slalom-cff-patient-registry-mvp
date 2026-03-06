namespace NgrApi.DTOs;

// ── Data Feed DTOs ─────────────────────────────────────────────────────────────

public class FeedRunDto
{
    public int Id { get; set; }
    public string RunType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalRecords { get; set; }
    public int ErrorCount { get; set; }
    public string? BlobPath { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public ReconciliationReportDto? Reconciliation { get; set; }
}

public class ReconciliationReportDto
{
    public DateTime ExtractionWindowStart { get; set; }
    public DateTime ExtractionWindowEnd { get; set; }
    public string RunType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public int TotalRecords { get; set; }
    public int ErrorCount { get; set; }
    public double QualityRate { get; set; }
    public List<EntityReconciliationDto> Entities { get; set; } = new();
}

public class EntityReconciliationDto
{
    public string EntityType { get; set; } = string.Empty;
    public int Creates { get; set; }
    public int Updates { get; set; }
    public int Deletes { get; set; }
    public int Total => Creates + Updates + Deletes;
}

public class TriggerFeedRunDto
{
    /// <summary>"Delta" or "Full"</summary>
    public string RunType { get; set; } = "Delta";

    /// <summary>Override the extraction window start (optional; for reprocessing a specific window)</summary>
    public DateTime? WindowOverrideStart { get; set; }

    /// <summary>Override the extraction window end (optional)</summary>
    public DateTime? WindowOverrideEnd { get; set; }
}

public class FeedFieldMappingDto
{
    public int Id { get; set; }
    public string NgrEntity { get; set; } = string.Empty;
    public string NgrProperty { get; set; } = string.Empty;
    public string CffColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? TransformHint { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateFeedFieldMappingDto
{
    public string CffColumnName { get; set; } = string.Empty;
    public string? TransformHint { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── Migration DTOs ─────────────────────────────────────────────────────────────

public class MigrationRunDto
{
    public int Id { get; set; }
    public string Phase { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SourceCount { get; set; }
    public int TargetCount { get; set; }
    public int ErrorCount { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public ValidationReportDto? ValidationReport { get; set; }
}

public class TriggerMigrationDto
{
    /// <summary>Phase to execute: "Demographics", "Diagnosis", "SweatTest", "ALD",
    /// "Transplant", "AnnualReview", "Encounter", "Labs", "CareEpisode", "PhoneNote", "Files"</summary>
    public string Phase { get; set; } = string.Empty;
}

public class ValidationReportDto
{
    public DateTime GeneratedAt { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public int TotalChecks { get; set; }
    public int PassedChecks { get; set; }
    public int FailedChecks { get; set; }
    public List<ValidationCheckDto> Checks { get; set; } = new();
}

public class ValidationCheckDto
{
    public string CheckName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public int? ExpectedCount { get; set; }
    public int? ActualCount { get; set; }
}
