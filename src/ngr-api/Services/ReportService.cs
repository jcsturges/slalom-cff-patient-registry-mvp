using System.Text;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;

namespace NgrApi.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePatientReportAsync(int? careProgramId, string? status, string format = "csv")
    {
        var query = _context.Patients.Include(p => p.CareProgram).AsQueryable();

        if (careProgramId.HasValue)
            query = query.Where(p => p.CareProgramId == careProgramId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var patients = await query.ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,FirstName,LastName,DateOfBirth,Status,CareProgram,MedicalRecordNumber,Email,Phone");

        foreach (var p in patients)
        {
            sb.AppendLine($"{p.Id},{p.FirstName},{p.LastName},{p.DateOfBirth:yyyy-MM-dd},{p.Status},{p.CareProgram?.Name},{p.MedicalRecordNumber},{p.Email},{p.Phone}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateProgramSummaryReportAsync(int careProgramId)
    {
        var program = await _context.CarePrograms.FindAsync(careProgramId);
        var patientCount = await _context.Patients.CountAsync(p => p.CareProgramId == careProgramId);
        var activeCount = await _context.Patients.CountAsync(p => p.CareProgramId == careProgramId && p.Status == "Active");

        var sb = new StringBuilder();
        sb.AppendLine($"Program Summary Report - {program?.Name}");
        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"Total Patients: {patientCount}");
        sb.AppendLine($"Active Patients: {activeCount}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
