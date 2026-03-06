using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IEmrMappingService
{
    /// <summary>Returns all mappings (global + program-specific)</summary>
    Task<IEnumerable<EmrFieldMapping>> GetAllMappingsAsync();

    /// <summary>
    /// Returns the effective mapping dictionary for a program: program-level records
    /// override global defaults; keyed by CsvColumnName (case-insensitive).
    /// </summary>
    Task<Dictionary<string, EmrFieldMapping>> GetEffectiveMappingsAsync(int programId);

    /// <summary>Upsert a program-level override mapping</summary>
    Task<EmrFieldMapping> UpsertMappingAsync(EmrFieldMappingDto dto, string createdBy);

    /// <summary>Delete a program-level mapping (global defaults cannot be deleted)</summary>
    Task<bool> DeleteMappingAsync(int id);

    /// <summary>Upsert an MRN crosswalk entry for a program</summary>
    Task<InstitutionMrnCrosswalk> UpsertMrnCrosswalkAsync(int programId, string mrn, int patientId, long cffId);

    /// <summary>Resolve an MRN to a patient for a given program</summary>
    Task<InstitutionMrnCrosswalk?> ResolveMrnAsync(int programId, string mrn);

    /// <summary>Seed global default field mappings if none exist yet</summary>
    Task EnsureDefaultMappingsAsync();
}
