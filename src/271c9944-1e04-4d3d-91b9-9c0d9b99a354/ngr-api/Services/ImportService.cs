using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class ImportService : IImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImportService> _logger;

    public ImportService(ApplicationDbContext context, ILogger<ImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportJob> StartImportAsync(string fileName, string uploadedBy)
    {
        var job = new ImportJob
        {
            FileName = fileName,
            CreatedBy = uploadedBy,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.ImportJobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<ImportJob?> GetImportJobAsync(int id)
    {
        return await _context.ImportJobs.FindAsync(id);
    }

    public async Task<IEnumerable<ImportJob>> GetImportJobsAsync()
    {
        return await _context.ImportJobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task ProcessImportAsync(int importJobId)
    {
        var job = await _context.ImportJobs.FindAsync(importJobId);
        if (job == null) return;

        job.Status = "Processing";
        await _context.SaveChangesAsync();

        try
        {
            // Stub: actual CSV parsing and patient import logic goes here
            _logger.LogInformation("Processing import job {ImportJobId} for file {FileName}", importJobId, job.FileName);

            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            job.ProcessedRows = 0;
            job.ErrorRows = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process import job {ImportJobId}", importJobId);
            job.Status = "Failed";
            job.ErrorsJson = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
