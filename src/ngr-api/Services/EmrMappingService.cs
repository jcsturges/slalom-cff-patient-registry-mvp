using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public class EmrMappingService : IEmrMappingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmrMappingService> _logger;

    public EmrMappingService(ApplicationDbContext context, ILogger<EmrMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<EmrFieldMapping>> GetAllMappingsAsync()
        => await _context.EmrFieldMappings.OrderBy(m => m.FormType).ThenBy(m => m.CsvColumnName).ToListAsync();

    public async Task<Dictionary<string, EmrFieldMapping>> GetEffectiveMappingsAsync(int programId)
    {
        var all = await _context.EmrFieldMappings
            .Where(m => m.IsActive && (m.ProgramId == null || m.ProgramId == programId))
            .ToListAsync();

        // Program-level overrides win over global defaults (same CsvColumnName, case-insensitive)
        var effective = new Dictionary<string, EmrFieldMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in all.OrderBy(m => m.ProgramId.HasValue ? 1 : 0)) // globals first
            effective[m.CsvColumnName] = m;

        return effective;
    }

    public async Task<EmrFieldMapping> UpsertMappingAsync(EmrFieldMappingDto dto, string createdBy)
    {
        var existing = await _context.EmrFieldMappings
            .FirstOrDefaultAsync(m => m.ProgramId == dto.ProgramId
                                   && m.CsvColumnName == dto.CsvColumnName
                                   && m.FormType == dto.FormType);

        if (existing != null)
        {
            existing.FieldPath = dto.FieldPath;
            existing.DataType = dto.DataType;
            existing.IsRequired = dto.IsRequired;
            existing.TransformHint = dto.TransformHint;
            existing.IsActive = dto.IsActive;
        }
        else
        {
            existing = new EmrFieldMapping
            {
                ProgramId = dto.ProgramId,
                CsvColumnName = dto.CsvColumnName,
                FormType = dto.FormType,
                FieldPath = dto.FieldPath,
                DataType = dto.DataType,
                IsRequired = dto.IsRequired,
                TransformHint = dto.TransformHint,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
            };
            _context.EmrFieldMappings.Add(existing);
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMappingAsync(int id)
    {
        var mapping = await _context.EmrFieldMappings.FindAsync(id);
        if (mapping == null) return false;
        if (mapping.ProgramId == null)
        {
            _logger.LogWarning("Attempt to delete global default EMR mapping {Id} rejected", id);
            return false; // global defaults are immutable at runtime
        }
        _context.EmrFieldMappings.Remove(mapping);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<InstitutionMrnCrosswalk> UpsertMrnCrosswalkAsync(
        int programId, string mrn, int patientId, long cffId)
    {
        var existing = await _context.InstitutionMrnCrosswalks
            .FirstOrDefaultAsync(c => c.ProgramId == programId && c.MedicalRecordNumber == mrn);

        if (existing != null)
        {
            existing.PatientId = patientId;
            existing.CffId = cffId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new InstitutionMrnCrosswalk
            {
                ProgramId = programId,
                MedicalRecordNumber = mrn,
                PatientId = patientId,
                CffId = cffId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.InstitutionMrnCrosswalks.Add(existing);
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<InstitutionMrnCrosswalk?> ResolveMrnAsync(int programId, string mrn)
        => await _context.InstitutionMrnCrosswalks
            .FirstOrDefaultAsync(c => c.ProgramId == programId && c.MedicalRecordNumber == mrn);

    // ── Default field mappings ────────────────────────────────────────────────
    // Seeded once via EnsureDefaultMappingsAsync called at startup.
    // Covers ~30 representative fields across Demographics, Encounter, Labs forms.
    // Full ~240-field mapping requires CFF-supplied eCRF specs (see known limitations).

    public async Task EnsureDefaultMappingsAsync()
    {
        if (await _context.EmrFieldMappings.AnyAsync(m => m.ProgramId == null))
            return; // already seeded

        var defaults = GetDefaultMappings().ToList();
        _context.EmrFieldMappings.AddRange(defaults);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} default EMR field mappings", defaults.Count);
    }

    public static IEnumerable<EmrFieldMapping> GetDefaultMappings() =>
    [
        // ── Demographics ─────────────────────────────────────────────────────
        new() { CsvColumnName = "CFF_ID",             FormType = FormTypes.Demographics, FieldPath = "cffId",               DataType = "integer", IsRequired = true,  IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "MRN",                FormType = FormTypes.Demographics, FieldPath = "mrn",                 DataType = "string",  IsRequired = true,  IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "FIRST_NAME",         FormType = FormTypes.Demographics, FieldPath = "firstName",           DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "LAST_NAME",          FormType = FormTypes.Demographics, FieldPath = "lastName",            DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "DATE_OF_BIRTH",      FormType = FormTypes.Demographics, FieldPath = "dateOfBirth",         DataType = "date",    IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow, TransformHint = "date:yyyy-MM-dd" },
        new() { CsvColumnName = "SEX_AT_BIRTH",       FormType = FormTypes.Demographics, FieldPath = "biologicalSexAtBirth",DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "ADDRESS_LINE1",      FormType = FormTypes.Demographics, FieldPath = "address1",            DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "ADDRESS_LINE2",      FormType = FormTypes.Demographics, FieldPath = "address2",            DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "CITY",               FormType = FormTypes.Demographics, FieldPath = "city",                DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "STATE",              FormType = FormTypes.Demographics, FieldPath = "state",               DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "ZIP_CODE",           FormType = FormTypes.Demographics, FieldPath = "zipCode",             DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "ETHNICITY",          FormType = FormTypes.Demographics, FieldPath = "ethnicity",           DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "RACE",               FormType = FormTypes.Demographics, FieldPath = "race",                DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "INSURANCE_TYPE",     FormType = FormTypes.Demographics, FieldPath = "insuranceType",       DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "INSURANCE_PROVIDER", FormType = FormTypes.Demographics, FieldPath = "insuranceProvider",   DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },

        // ── Encounter ─────────────────────────────────────────────────────────
        new() { CsvColumnName = "ENCOUNTER_DATE",     FormType = FormTypes.Encounter, FieldPath = "encounterDate",  DataType = "date",    IsRequired = true,  IsActive = true, CreatedAt = DateTime.UtcNow, TransformHint = "date:yyyy-MM-dd" },
        new() { CsvColumnName = "ENCOUNTER_TYPE",     FormType = FormTypes.Encounter, FieldPath = "encounterType",  DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "PROVIDER_NAME",      FormType = FormTypes.Encounter, FieldPath = "providerName",   DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "HEIGHT_CM",          FormType = FormTypes.Encounter, FieldPath = "height_cm",      DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "WEIGHT_KG",          FormType = FormTypes.Encounter, FieldPath = "weight_kg",      DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "BMI",                FormType = FormTypes.Encounter, FieldPath = "bmi",            DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "FEV1_PERCENT",       FormType = FormTypes.Encounter, FieldPath = "fev1Percent",    DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "FVC_PERCENT",        FormType = FormTypes.Encounter, FieldPath = "fvcPercent",     DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },

        // ── Labs and Tests ────────────────────────────────────────────────────
        new() { CsvColumnName = "LAB_DATE",           FormType = FormTypes.LabsAndTests, FieldPath = "labDate",          DataType = "date",    IsRequired = true,  IsActive = true, CreatedAt = DateTime.UtcNow, TransformHint = "date:yyyy-MM-dd" },
        new() { CsvColumnName = "LAB_TEST_NAME",      FormType = FormTypes.LabsAndTests, FieldPath = "testName",         DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "LAB_RESULT_VALUE",   FormType = FormTypes.LabsAndTests, FieldPath = "resultValue",      DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "LAB_RESULT_UNIT",    FormType = FormTypes.LabsAndTests, FieldPath = "resultUnit",       DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "CULTURE_ORGANISM",   FormType = FormTypes.LabsAndTests, FieldPath = "cultureOrganism",  DataType = "string",  IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "HBA1C",              FormType = FormTypes.LabsAndTests, FieldPath = "hba1c",            DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        new() { CsvColumnName = "BLOOD_GLUCOSE",      FormType = FormTypes.LabsAndTests, FieldPath = "bloodGlucose",     DataType = "decimal", IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
    ];
}
