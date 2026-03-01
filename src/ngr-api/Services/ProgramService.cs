using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class ProgramService : IProgramService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProgramService> _logger;

    public ProgramService(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<ProgramService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<(IEnumerable<CareProgram> Programs, int TotalCount)> SearchProgramsAsync(
        int? programId = null,
        string? name = null,
        string? city = null,
        string? state = null,
        bool includeInactive = false,
        bool includeOrh = false,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.CarePrograms.AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!includeOrh)
            query = query.Where(p => !p.IsOrphanHoldingProgram);

        if (programId.HasValue)
            query = query.Where(p => p.ProgramId == programId.Value);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City != null && p.City.Contains(city));

        if (!string.IsNullOrWhiteSpace(state))
            query = query.Where(p => p.State == state);

        var totalCount = await query.CountAsync();

        var programs = await query
            .OrderBy(p => p.ProgramId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (programs, totalCount);
    }

    public async Task<CareProgram?> GetCareProgramByIdAsync(int id)
    {
        return await _context.CarePrograms.FindAsync(id);
    }

    public async Task<bool> ProgramIdExistsAsync(int programId)
    {
        return await _context.CarePrograms.AnyAsync(p => p.ProgramId == programId);
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _context.CarePrograms.AnyAsync(p => p.Code == code);
    }

    public async Task<CareProgram> CreateCareProgramAsync(
        CareProgram careProgram, string userId, string userEmail)
    {
        careProgram.CreatedAt = DateTime.UtcNow;
        careProgram.UpdatedAt = DateTime.UtcNow;
        _context.CarePrograms.Add(careProgram);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "CareProgram",
            careProgram.Id.ToString(),
            "Created",
            oldValues: null,
            newValues: new
            {
                careProgram.ProgramId,
                careProgram.Code,
                careProgram.Name,
                careProgram.ProgramType,
                careProgram.City,
                careProgram.State,
                careProgram.IsTrainingProgram,
            },
            userId,
            userEmail,
            ipAddress: null);

        _logger.LogInformation("Care program created: {ProgramId} {Name}", careProgram.ProgramId, careProgram.Name);
        return careProgram;
    }

    public async Task<CareProgram?> UpdateCareProgramAsync(
        int id, CareProgram updates, string userId, string userEmail)
    {
        var existing = await _context.CarePrograms.FindAsync(id);
        if (existing == null) return null;

        // ORH program cannot be edited
        if (existing.IsOrphanHoldingProgram)
        {
            _logger.LogWarning("Attempt to edit ORH program blocked");
            throw new InvalidOperationException("The Orphaned Record Holding program cannot be edited.");
        }

        // Capture old values for audit
        var oldValues = new
        {
            existing.Name,
            existing.ProgramType,
            existing.City,
            existing.State,
            existing.Address1,
            existing.Address2,
            existing.ZipCode,
            existing.Phone,
            existing.Email,
            existing.IsActive,
        };

        // ProgramId is immutable — never update it
        existing.Name = updates.Name;
        existing.ProgramType = updates.ProgramType;
        existing.City = updates.City;
        existing.State = updates.State;
        existing.Address1 = updates.Address1;
        existing.Address2 = updates.Address2;
        existing.ZipCode = updates.ZipCode;
        existing.Phone = updates.Phone;
        existing.Email = updates.Email;
        existing.IsActive = updates.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var newValues = new
        {
            existing.Name,
            existing.ProgramType,
            existing.City,
            existing.State,
            existing.Address1,
            existing.Address2,
            existing.ZipCode,
            existing.Phone,
            existing.Email,
            existing.IsActive,
        };

        var action = !updates.IsActive && (bool)oldValues.IsActive ? "Deactivated" : "Updated";

        await _auditService.LogActionAsync(
            "CareProgram",
            existing.Id.ToString(),
            action,
            oldValues,
            newValues,
            userId,
            userEmail,
            ipAddress: null);

        _logger.LogInformation("Care program {Action}: {ProgramId} {Name}", action, existing.ProgramId, existing.Name);
        return existing;
    }
}
