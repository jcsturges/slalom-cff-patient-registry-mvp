using NgrApi.Models;

namespace NgrApi.Services;

public interface IProgramService
{
    Task<(IEnumerable<CareProgram> Programs, int TotalCount)> SearchProgramsAsync(
        int? programId = null,
        string? name = null,
        string? city = null,
        string? state = null,
        bool includeInactive = false,
        bool includeOrh = false,
        int page = 1,
        int pageSize = 50);
    Task<CareProgram?> GetCareProgramByIdAsync(int id);
    Task<CareProgram> CreateCareProgramAsync(CareProgram careProgram, string userId, string userEmail);
    Task<CareProgram?> UpdateCareProgramAsync(int id, CareProgram updates, string userId, string userEmail);
    Task<bool> ProgramIdExistsAsync(int programId);
    Task<bool> CodeExistsAsync(string code);
}
