namespace NgrApi.Services;

public interface IReportService
{
    Task<byte[]> GeneratePatientReportAsync(int? careProgramId, string? status, string format = "csv");
    Task<byte[]> GenerateProgramSummaryReportAsync(int careProgramId);
}
