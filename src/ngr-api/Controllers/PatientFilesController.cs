using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/patients/{patientId:int}/files")]
[Authorize]
public class PatientFilesController : ControllerBase
{
    private readonly IPatientFileService _fileService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PatientFilesController> _logger;

    public PatientFilesController(
        IPatientFileService fileService,
        IAuditService auditService,
        ILogger<PatientFilesController> logger)
    {
        _fileService = fileService;
        _auditService = auditService;
        _logger = logger;
    }

    private (string UserId, string UserEmail) GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub") ?? "unknown";
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                       ?? User.Identity?.Name ?? "unknown";
        return (userId, userEmail);
    }

    /// <summary>List files for a patient (paginated)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientFileDto>>> GetFiles(
        int patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        var files = await _fileService.GetFilesAsync(patientId, page, pageSize);
        return Ok(files);
    }

    /// <summary>Upload a file for a patient</summary>
    [HttpPost]
    [Authorize(Policy = "ClinicalUser")]
    [RequestSizeLimit(11 * 1024 * 1024)] // Slightly above 10MB to account for form data
    public async Task<ActionResult<PatientFileDto>> Upload(
        int patientId,
        [FromForm] IFormFile file,
        [FromForm] int programId,
        [FromForm] string fileType,
        [FromForm] string? description = null,
        [FromForm] string? otherFileTypeDescription = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            var (_, userEmail) = GetUserInfo();
            using var stream = file.OpenReadStream();

            var result = await _fileService.UploadAsync(
                patientId, programId, stream,
                file.FileName, file.ContentType, file.Length,
                description, fileType, otherFileTypeDescription,
                userEmail);

            return CreatedAtAction(nameof(GetFiles), new { patientId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Download a file</summary>
    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> Download(int patientId, int fileId)
    {
        var (stream, contentType, fileName) = await _fileService.DownloadAsync(fileId);
        if (stream == null) return NotFound();

        var (userId, userEmail) = GetUserInfo();
        await _auditService.LogActionAsync(
            "PatientFile", fileId.ToString(), "Download",
            null, new { PatientId = patientId, FileId = fileId },
            userId, userEmail, HttpContext.Connection.RemoteIpAddress?.ToString());

        return File(stream, contentType!, fileName!);
    }

    /// <summary>Update file metadata</summary>
    [HttpPut("{fileId:int}")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<PatientFileDto>> UpdateMetadata(
        int patientId, int fileId, [FromBody] UpdatePatientFileDto dto)
    {
        var result = await _fileService.UpdateMetadataAsync(fileId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Delete a file</summary>
    [HttpDelete("{fileId:int}")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<IActionResult> Delete(int patientId, int fileId)
    {
        var (userId, userEmail) = GetUserInfo();
        var file = await _fileService.GetFileByIdAsync(fileId);
        if (file == null) return NotFound();

        var deleted = await _fileService.DeleteAsync(fileId);
        if (!deleted) return NotFound();

        await _auditService.LogActionAsync(
            "PatientFile", fileId.ToString(), "Delete",
            file, null, userId, userEmail,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return NoContent();
    }
}
