using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class ProgramService : IProgramService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProgramService> _logger;

    public ProgramService(ApplicationDbContext context, ILogger<ProgramService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CareProgram>> GetCareProgramsAsync()
    {
        return await _context.CarePrograms.ToListAsync();
    }

    public async Task<CareProgram?> GetCareProgramByIdAsync(int id)
    {
        return await _context.CarePrograms.FindAsync(id);
    }

    public async Task<CareProgram> CreateCareProgramAsync(CareProgram careProgram)
    {
        careProgram.CreatedAt = DateTime.UtcNow;
        careProgram.UpdatedAt = DateTime.UtcNow;
        _context.CarePrograms.Add(careProgram);
        await _context.SaveChangesAsync();
        return careProgram;
    }

    public async Task<CareProgram?> UpdateCareProgramAsync(int id, CareProgram careProgram)
    {
        var existing = await _context.CarePrograms.FindAsync(id);
        if (existing == null) return null;

        existing.Name = careProgram.Name;
        existing.IsActive = careProgram.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteCareProgramAsync(int id)
    {
        var existing = await _context.CarePrograms.FindAsync(id);
        if (existing == null) return false;

        _context.CarePrograms.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
