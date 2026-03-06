using Microsoft.AspNetCore.Http;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IImportService
{
    /// <summary>Accept a CSV upload, validate format, persist metadata, queue processing</summary>
    Task<ImportJobDto> UploadCsvAsync(IFormFile file, int programId, string uploadedBy);

    /// <summary>Run file-level and field-level validation without persisting</summary>
    Task<EmrValidationResult> ValidateCsvAsync(Stream csvStream, int programId);

    Task<ImportJobDto?> GetImportJobAsync(int id);
    Task<ImportJobDetailDto?> GetImportJobDetailAsync(int id);
    Task<IEnumerable<ImportJobDto>> GetImportJobsByProgramAsync(int programId);

    /// <summary>Process a previously uploaded import job: map fields, update forms</summary>
    Task ProcessImportAsync(int importJobId);
}
