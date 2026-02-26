using NgrApi.Models;

namespace NgrApi.Services;

public interface IProgramService
{
    Task<IEnumerable<CareProgram>> GetCareProgramsAsync();
    Task<CareProgram?> GetCareProgramByIdAsync(int id);
    Task<CareProgram> CreateCareProgramAsync(CareProgram careProgram);
    Task<CareProgram?> UpdateCareProgramAsync(int id, CareProgram careProgram);
    Task<bool> DeleteCareProgramAsync(int id);
}
