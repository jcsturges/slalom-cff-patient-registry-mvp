using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IPatientFileService
{
    Task<IEnumerable<PatientFileDto>> GetFilesAsync(int patientId, int page = 1, int pageSize = 5);
    Task<PatientFileDto?> GetFileByIdAsync(int id);
    Task<PatientFileDto> UploadAsync(int patientId, int programId, Stream fileStream, string originalFileName, string contentType, long fileSize, string? description, string fileType, string? otherDescription, string uploadedBy);
    Task<PatientFileDto?> UpdateMetadataAsync(int id, UpdatePatientFileDto dto);
    Task<(Stream? Stream, string? ContentType, string? FileName)> DownloadAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class PatientFileService : IPatientFileService
{
    private readonly ApplicationDbContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly ILogger<PatientFileService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff" };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        { "application/pdf", "image/jpeg", "image/png", "image/tiff" };

    private static readonly Dictionary<string, string> FileTypePrefixes = new()
    {
        ["Informed Consent"] = "ICF",
        ["Genotype Results"] = "GTP",
        ["Sweat Test Results"] = "Swt",
        ["Lab Results"] = "Lab",
        ["Diagnosis"] = "DX",
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const string ContainerName = "patient-files";

    public PatientFileService(
        ApplicationDbContext context,
        IBlobStorageService blobService,
        ILogger<PatientFileService> logger)
    {
        _context = context;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<IEnumerable<PatientFileDto>> GetFilesAsync(int patientId, int page = 1, int pageSize = 5)
    {
        var files = await _context.PatientFiles
            .Include(f => f.Program)
            .Where(f => f.PatientId == patientId)
            .OrderByDescending(f => f.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return files.Select(MapToDto);
    }

    public async Task<PatientFileDto?> GetFileByIdAsync(int id)
    {
        var file = await _context.PatientFiles
            .Include(f => f.Program)
            .FirstOrDefaultAsync(f => f.Id == id);
        return file == null ? null : MapToDto(file);
    }

    public async Task<PatientFileDto> UploadAsync(
        int patientId, int programId, Stream fileStream,
        string originalFileName, string contentType, long fileSize,
        string? description, string fileType, string? otherDescription, string uploadedBy)
    {
        // Validate file extension
        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

        // Validate MIME type
        if (!AllowedMimeTypes.Contains(contentType))
            throw new ArgumentException($"MIME type '{contentType}' is not allowed.");

        // Validate file size
        if (fileSize > MaxFileSize)
            throw new ArgumentException($"File size ({fileSize / 1024 / 1024}MB) exceeds maximum of 10MB.");

        // Load patient for naming
        var patient = await _context.Patients.FindAsync(patientId)
            ?? throw new ArgumentException("Patient not found");

        // Generate stored file name per convention
        var prefix = fileType == "Other"
            ? "Oth" + SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName))[..Math.Min(5, Path.GetFileNameWithoutExtension(originalFileName).Length)]
            : FileTypePrefixes.GetValueOrDefault(fileType, "Oth");

        var now = DateTime.UtcNow;
        var storedFileName = $"{prefix}_{patient.RegistryId}_{programId}_{now:MMddyyyy}_{now:HHmmss}{extension}";
        storedFileName = SanitizeFileName(storedFileName);

        // Upload to blob storage
        var blobPath = $"patients/{patientId}/{storedFileName}";
        await _blobService.UploadAsync(ContainerName, blobPath, fileStream, contentType);

        var patientFile = new PatientFile
        {
            PatientId = patientId,
            ProgramId = programId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            BlobPath = blobPath,
            ContentType = contentType,
            FileExtension = extension,
            FileSize = fileSize,
            Description = description,
            FileType = fileType,
            OtherFileTypeDescription = otherDescription,
            UploadedAt = now,
            UploadedBy = uploadedBy,
        };

        _context.PatientFiles.Add(patientFile);
        await _context.SaveChangesAsync();

        _logger.LogInformation("File uploaded for patient {PatientId}: {FileName}", patientId, storedFileName);

        await _context.Entry(patientFile).Reference(f => f.Program).LoadAsync();
        return MapToDto(patientFile);
    }

    public async Task<PatientFileDto?> UpdateMetadataAsync(int id, UpdatePatientFileDto dto)
    {
        var file = await _context.PatientFiles.Include(f => f.Program).FirstOrDefaultAsync(f => f.Id == id);
        if (file == null) return null;

        file.Description = dto.Description;
        file.FileType = dto.FileType;
        file.OtherFileTypeDescription = dto.OtherFileTypeDescription;

        await _context.SaveChangesAsync();
        return MapToDto(file);
    }

    public async Task<(Stream? Stream, string? ContentType, string? FileName)> DownloadAsync(int id)
    {
        var file = await _context.PatientFiles.FindAsync(id);
        if (file == null) return (null, null, null);

        var stream = await _blobService.DownloadAsync(ContainerName, file.BlobPath);
        return (stream, file.ContentType, file.StoredFileName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var file = await _context.PatientFiles.FindAsync(id);
        if (file == null) return false;

        await _blobService.DeleteAsync(ContainerName, file.BlobPath);
        _context.PatientFiles.Remove(file);
        await _context.SaveChangesAsync();
        return true;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private static PatientFileDto MapToDto(PatientFile f) => new()
    {
        Id = f.Id,
        PatientId = f.PatientId,
        ProgramId = f.ProgramId,
        ProgramName = f.Program?.Name ?? "",
        OriginalFileName = f.OriginalFileName,
        StoredFileName = f.StoredFileName,
        ContentType = f.ContentType,
        FileExtension = f.FileExtension,
        FileSize = f.FileSize,
        Description = f.Description,
        FileType = f.FileType,
        OtherFileTypeDescription = f.OtherFileTypeDescription,
        UploadedAt = f.UploadedAt,
        UploadedBy = f.UploadedBy,
    };
}
