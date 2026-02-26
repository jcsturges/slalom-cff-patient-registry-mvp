using NgrApi.Models;

namespace NgrApi.Services;

public interface IImportService
{
    Task<ImportJob> StartImportAsync(string fileName, string uploadedBy);
    Task<ImportJob?> GetImportJobAsync(int id);
    Task<IEnumerable<ImportJob>> GetImportJobsAsync();
    Task ProcessImportAsync(int importJobId);
}
