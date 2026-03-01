using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Models;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "FoundationAnalyst")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProgramsController> _logger;

    public ProgramsController(
        IProgramService programService,
        IMapper mapper,
        ILogger<ProgramsController> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>Search and list care programs with filtering and pagination</summary>
    [HttpGet]
    [Authorize] // Override controller-level policy — all authenticated users can list programs
    public async Task<ActionResult<IEnumerable<CareProgramDto>>> GetPrograms(
        [FromQuery] int? programId,
        [FromQuery] string? name,
        [FromQuery] string? city,
        [FromQuery] string? state,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeOrh = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var (programs, totalCount) = await _programService.SearchProgramsAsync(
            programId, name, city, state, includeInactive, includeOrh, page, pageSize);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());

        return Ok(_mapper.Map<IEnumerable<CareProgramDto>>(programs));
    }

    /// <summary>Get a single care program by internal ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CareProgramDto>> GetProgram(int id)
    {
        var program = await _programService.GetCareProgramByIdAsync(id);
        if (program == null)
            return NotFound();

        return Ok(_mapper.Map<CareProgramDto>(program));
    }

    /// <summary>Create a new care program</summary>
    [HttpPost]
    public async Task<ActionResult<CareProgramDto>> CreateProgram(
        [FromBody] CreateCareProgramDto dto)
    {
        // Check ProgramId and Code uniqueness
        if (await _programService.ProgramIdExistsAsync(dto.ProgramId))
            return Conflict($"Program ID {dto.ProgramId} already exists");
        if (await _programService.CodeExistsAsync(dto.Code))
            return Conflict($"Program code '{dto.Code}' already exists");

        var careProgram = _mapper.Map<CareProgram>(dto);
        var (userId, userEmail) = GetUserInfo();
        var created = await _programService.CreateCareProgramAsync(careProgram, userId, userEmail);

        return CreatedAtAction(
            nameof(GetProgram),
            new { id = created.Id },
            _mapper.Map<CareProgramDto>(created));
    }

    /// <summary>Update an existing care program. Program ID cannot be changed.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CareProgramDto>> UpdateProgram(
        int id,
        [FromBody] UpdateCareProgramDto dto)
    {
        var updates = new CareProgram
        {
            Name = dto.Name,
            ProgramType = dto.ProgramType,
            City = dto.City,
            State = dto.State,
            Address1 = dto.Address1,
            Address2 = dto.Address2,
            ZipCode = dto.ZipCode,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive,
        };

        var (userId, userEmail) = GetUserInfo();

        CareProgram? updated;
        try
        {
            updated = await _programService.UpdateCareProgramAsync(id, updates, userId, userEmail);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        if (updated == null)
            return NotFound();

        return Ok(_mapper.Map<CareProgramDto>(updated));
    }

    /// <summary>Programs cannot be deleted — only deactivated via PUT.</summary>
    [HttpDelete("{id:int}")]
    public IActionResult DeleteProgram(int id)
    {
        return StatusCode(405, "Programs cannot be deleted. Use PUT to deactivate.");
    }

    private (string UserId, string UserEmail) GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? "unknown";
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                       ?? User.FindFirstValue("email")
                       ?? "unknown";
        return (userId, userEmail);
    }
}
