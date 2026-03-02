using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _service;
    private readonly ILogger<AnnouncementsController> _logger;

    public AnnouncementsController(IAnnouncementService service, ILogger<AnnouncementsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get all announcements (optionally including inactive)</summary>
    [HttpGet]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var announcements = await _service.GetAllAsync(includeInactive);
        return Ok(announcements);
    }

    /// <summary>Get active announcements (public for all authenticated users)</summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetActive()
    {
        var announcements = await _service.GetActiveAsync();
        return Ok(announcements);
    }

    /// <summary>Get a single announcement by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int id)
    {
        var announcement = await _service.GetByIdAsync(id);
        if (announcement == null) return NotFound();
        return Ok(announcement);
    }

    /// <summary>Create a new announcement (Foundation Admin only)</summary>
    [HttpPost]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<AnnouncementDto>> Create([FromBody] CreateAnnouncementDto dto)
    {
        var createdBy = User.Identity?.Name ?? "unknown";
        var announcement = await _service.CreateAsync(dto, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, announcement);
    }

    /// <summary>Update an existing announcement (Foundation Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<AnnouncementDto>> Update(int id, [FromBody] UpdateAnnouncementDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Delete an announcement (Foundation Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
