using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/help-pages")]
[Authorize]
public class HelpPagesController : ControllerBase
{
    private readonly IHelpPageService _service;
    private readonly ILogger<HelpPagesController> _logger;

    public HelpPagesController(IHelpPageService service, ILogger<HelpPagesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get all help pages (flat list)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HelpPageDto>>> GetAll(
        [FromQuery] bool includeUnpublished = false)
    {
        // Only Foundation Admins can see unpublished pages
        if (includeUnpublished && !User.IsInRole("FoundationAnalyst") && !User.IsInRole("SystemAdmin"))
            includeUnpublished = false;

        var pages = await _service.GetAllAsync(includeUnpublished);
        return Ok(pages);
    }

    /// <summary>Get help pages as a hierarchical tree</summary>
    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<HelpPageDto>>> GetTree(
        [FromQuery] bool includeUnpublished = false)
    {
        if (includeUnpublished && !User.IsInRole("FoundationAnalyst") && !User.IsInRole("SystemAdmin"))
            includeUnpublished = false;

        var tree = await _service.GetTreeAsync(includeUnpublished);
        return Ok(tree);
    }

    /// <summary>Get a single help page by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HelpPageDto>> GetById(int id)
    {
        var page = await _service.GetByIdAsync(id);
        if (page == null) return NotFound();
        return Ok(page);
    }

    /// <summary>Get a help page by slug</summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<HelpPageDto>> GetBySlug(string slug)
    {
        var page = await _service.GetBySlugAsync(slug);
        if (page == null) return NotFound();
        return Ok(page);
    }

    /// <summary>Get a help page by context key (for context-sensitive help)</summary>
    [HttpGet("context/{contextKey}")]
    public async Task<ActionResult<HelpPageDto>> GetByContextKey(string contextKey)
    {
        var page = await _service.GetByContextKeyAsync(contextKey);
        if (page == null) return NotFound();
        return Ok(page);
    }

    /// <summary>Create a new help page (Foundation Admin only)</summary>
    [HttpPost]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<HelpPageDto>> Create([FromBody] CreateHelpPageDto dto)
    {
        var createdBy = User.Identity?.Name ?? "unknown";
        var page = await _service.CreateAsync(dto, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    /// <summary>Update an existing help page (Foundation Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<HelpPageDto>> Update(int id, [FromBody] UpdateHelpPageDto dto)
    {
        var updatedBy = User.Identity?.Name ?? "unknown";
        var result = await _service.UpdateAsync(id, dto, updatedBy);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Delete a help page (Foundation Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
