namespace NgrApi.DTOs;

public class DatabaseLockDto
{
    public int Id { get; set; }
    public int ReportingYear { get; set; }
    public DateTime LockDate { get; set; }
    public string ExecutionMode { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int FormsLocked { get; set; }
    public int FormsSkipped { get; set; }
    public int? ProgressFormsTotal { get; set; }
    public int ProgressFormsProcessed { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CreateDatabaseLockDto
{
    public int ReportingYear { get; set; }
    public DateTime LockDate { get; set; }

    /// <summary>"Synchronous" | "Batch"</summary>
    public string ExecutionMode { get; set; } = "Synchronous";

    /// <summary>Required when ExecutionMode is "Batch"</summary>
    public DateTime? ScheduledDate { get; set; }
}

public class DatabaseLockImpactDto
{
    public int ReportingYear { get; set; }

    /// <summary>Total form submissions eligible for locking</summary>
    public int EligibleForms { get; set; }

    /// <summary>Forms already locked for this year</summary>
    public int AlreadyLocked { get; set; }

    /// <summary>Forms that would be newly locked</summary>
    public int WouldLock { get; set; }

    /// <summary>Whether this reporting year already has a completed lock</summary>
    public bool IsAlreadyLocked { get; set; }
}

public class DatabaseLockProgressDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int FormsLocked { get; set; }
    public int FormsSkipped { get; set; }
    public int? ProgressFormsTotal { get; set; }
    public int ProgressFormsProcessed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public double? ProgressPercent => ProgressFormsTotal.HasValue && ProgressFormsTotal > 0
        ? Math.Round((double)ProgressFormsProcessed / ProgressFormsTotal.Value * 100, 1)
        : null;
}
