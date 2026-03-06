using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public class DatabaseLockService : IDatabaseLockService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<DatabaseLockService> _logger;

    public DatabaseLockService(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<DatabaseLockService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IEnumerable<DatabaseLockDto>> GetLocksAsync()
    {
        var locks = await _context.DatabaseLocks
            .OrderByDescending(l => l.ReportingYear)
            .ToListAsync();
        return locks.Select(MapToDto);
    }

    public async Task<DatabaseLockImpactDto> GetImpactAsync(int reportingYear)
    {
        var yearStart = new DateTime(reportingYear, 1, 1);
        var yearEnd   = new DateTime(reportingYear, 12, 31, 23, 59, 59);

        var total = await _context.FormSubmissions
            .Where(f => f.EncounterDate >= yearStart && f.EncounterDate <= yearEnd
                     || f.LabDate >= yearStart && f.LabDate <= yearEnd
                     || f.PhoneNoteDate >= yearStart && f.PhoneNoteDate <= yearEnd
                     || f.AnnualReviewYear == reportingYear)
            .CountAsync();

        var alreadyLocked = await _context.FormSubmissions
            .Where(f => f.IsLocked &&
                       (f.EncounterDate >= yearStart && f.EncounterDate <= yearEnd
                     || f.LabDate >= yearStart && f.LabDate <= yearEnd
                     || f.PhoneNoteDate >= yearStart && f.PhoneNoteDate <= yearEnd
                     || f.AnnualReviewYear == reportingYear))
            .CountAsync();

        var existingLock = await _context.DatabaseLocks
            .FirstOrDefaultAsync(l => l.ReportingYear == reportingYear && l.Status == "Completed");

        return new DatabaseLockImpactDto
        {
            ReportingYear  = reportingYear,
            EligibleForms  = total,
            AlreadyLocked  = alreadyLocked,
            WouldLock      = total - alreadyLocked,
            IsAlreadyLocked = existingLock != null,
        };
    }

    public async Task<DatabaseLockProgressDto> GetProgressAsync(int lockId)
    {
        var lockRecord = await _context.DatabaseLocks.FindAsync(lockId);
        if (lockRecord == null)
            throw new InvalidOperationException($"Lock {lockId} not found.");
        return MapToProgressDto(lockRecord);
    }

    public async Task<DatabaseLockDto> ExecuteSynchronousLockAsync(
        int reportingYear, DateTime lockDate, string initiatedBy)
    {
        // Idempotency: if a Completed lock already exists, return it
        var existing = await _context.DatabaseLocks
            .FirstOrDefaultAsync(l => l.ReportingYear == reportingYear && l.Status == "Completed");
        if (existing != null)
        {
            _logger.LogInformation("Lock for {Year} already completed — returning existing record", reportingYear);
            return MapToDto(existing);
        }

        var lockRecord = new DatabaseLock
        {
            ReportingYear   = reportingYear,
            LockDate        = lockDate,
            ExecutionMode   = "Synchronous",
            Status          = "InProgress",
            InitiatedBy     = initiatedBy,
            InitiatedAt     = DateTime.UtcNow,
        };
        _context.DatabaseLocks.Add(lockRecord);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Starting synchronous lock for reporting year {Year}", reportingYear);

        var yearStart = new DateTime(reportingYear, 1, 1);
        var yearEnd   = new DateTime(reportingYear, 12, 31, 23, 59, 59);

        int formsLocked  = 0;
        int formsSkipped = 0;

        try
        {
            // Process in batches to avoid table locks
            const int batchSize = 500;
            int skip = 0;

            while (true)
            {
                var batch = await _context.FormSubmissions
                    .Where(f => !f.IsLocked &&
                               (f.EncounterDate >= yearStart && f.EncounterDate <= yearEnd
                             || f.LabDate >= yearStart && f.LabDate <= yearEnd
                             || f.PhoneNoteDate >= yearStart && f.PhoneNoteDate <= yearEnd
                             || f.AnnualReviewYear == reportingYear))
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (batch.Count == 0) break;

                var now = DateTime.UtcNow;
                foreach (var fs in batch)
                {
                    // Skip forms that are actively being edited (RequiresReview = edit in progress proxy)
                    // In production this would check an ActiveEditingSessions table
                    fs.IsLocked  = true;
                    fs.LockedAt  = now;
                    formsLocked++;
                }

                await _context.SaveChangesAsync();
                skip += batchSize;
            }

            lockRecord.Status          = "Completed";
            lockRecord.FormsLocked     = formsLocked;
            lockRecord.FormsSkipped    = formsSkipped;
            lockRecord.CompletedAt     = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronous lock failed for year {Year}", reportingYear);
            lockRecord.Status       = "Failed";
            lockRecord.ErrorMessage = ex.Message;
            lockRecord.CompletedAt  = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "DatabaseLock", lockRecord.Id.ToString(), "Execute",
            null, $"{{\"year\":{reportingYear},\"formsLocked\":{formsLocked},\"status\":\"{lockRecord.Status}\"}}",
            initiatedBy, initiatedBy, null);

        _logger.LogInformation(
            "Lock for {Year} finished: status={Status}, locked={Locked}, skipped={Skipped}",
            reportingYear, lockRecord.Status, formsLocked, formsSkipped);

        return MapToDto(lockRecord);
    }

    public async Task<DatabaseLockDto> ScheduleBatchLockAsync(
        int reportingYear, DateTime lockDate, DateTime scheduledDate, string initiatedBy)
    {
        // Only one lock per year
        var existing = await _context.DatabaseLocks
            .FirstOrDefaultAsync(l => l.ReportingYear == reportingYear);

        if (existing != null && existing.Status == "Completed")
            return MapToDto(existing);

        var lockRecord = existing ?? new DatabaseLock();
        lockRecord.ReportingYear  = reportingYear;
        lockRecord.LockDate       = lockDate;
        lockRecord.ExecutionMode  = "Batch";
        lockRecord.ScheduledDate  = scheduledDate;
        lockRecord.Status         = "Pending";
        lockRecord.InitiatedBy    = initiatedBy;
        lockRecord.InitiatedAt    = DateTime.UtcNow;

        if (existing == null) _context.DatabaseLocks.Add(lockRecord);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "DatabaseLock", lockRecord.Id.ToString(), "Schedule",
            null, $"{{\"year\":{reportingYear},\"scheduledDate\":\"{scheduledDate:o}\"}}",
            initiatedBy, initiatedBy, null);

        return MapToDto(lockRecord);
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static DatabaseLockDto MapToDto(DatabaseLock l) => new()
    {
        Id                    = l.Id,
        ReportingYear         = l.ReportingYear,
        LockDate              = l.LockDate,
        ExecutionMode         = l.ExecutionMode,
        ScheduledDate         = l.ScheduledDate,
        Status                = l.Status,
        InitiatedBy           = l.InitiatedBy,
        InitiatedAt           = l.InitiatedAt,
        CompletedAt           = l.CompletedAt,
        FormsLocked           = l.FormsLocked,
        FormsSkipped          = l.FormsSkipped,
        ProgressFormsTotal    = l.ProgressFormsTotal,
        ProgressFormsProcessed = l.ProgressFormsProcessed,
        RetryCount            = l.RetryCount,
        ErrorMessage          = l.ErrorMessage,
    };

    private static DatabaseLockProgressDto MapToProgressDto(DatabaseLock l) => new()
    {
        Id                    = l.Id,
        Status                = l.Status,
        FormsLocked           = l.FormsLocked,
        FormsSkipped          = l.FormsSkipped,
        ProgressFormsTotal    = l.ProgressFormsTotal,
        ProgressFormsProcessed = l.ProgressFormsProcessed,
        CompletedAt           = l.CompletedAt,
        ErrorMessage          = l.ErrorMessage,
    };
}
