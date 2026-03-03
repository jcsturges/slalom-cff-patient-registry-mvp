using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

/// <summary>
/// Service implementation for patient operations (SRS Section 4)
/// </summary>
public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<PatientService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // ── Query Methods ────────────────────────────────────────────

    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(
        int? careProgramId,
        string? status,
        string? searchTerm,
        int page,
        int pageSize)
    {
        var query = _context.Patients
            .Include(p => p.CareProgram)
            .Include(p => p.ProgramAssignments)
                .ThenInclude(pa => pa.Program)
            .Where(p => !p.ConsentWithdrawn)
            .AsQueryable();

        // Filter by program via PatientProgramAssignment
        if (careProgramId.HasValue)
        {
            query = query.Where(p =>
                p.ProgramAssignments.Any(pa =>
                    pa.ProgramId == careProgramId.Value && pa.Status == "Active"));
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                (p.MedicalRecordNumber != null && p.MedicalRecordNumber.ToLower().Contains(term)) ||
                p.CffId.ToString().Contains(term) ||
                p.RegistryId.ToLower().Contains(term));
        }

        var patients = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return patients.Select(p => MapToDto(p, careProgramId));
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.CareProgram)
            .Include(p => p.ProgramAssignments)
                .ThenInclude(pa => pa.Program)
            .FirstOrDefaultAsync(p => p.Id == id);

        return patient == null ? null : MapToDto(patient, null);
    }

    public async Task<int> GetPatientCountAsync(int? careProgramId)
    {
        var query = _context.Patients.Where(p => !p.ConsentWithdrawn).AsQueryable();

        if (careProgramId.HasValue)
        {
            query = query.Where(p =>
                p.ProgramAssignments.Any(pa =>
                    pa.ProgramId == careProgramId.Value && pa.Status == "Active"));
        }

        return await query.CountAsync();
    }

    // ── CRUD Methods ─────────────────────────────────────────────

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto, string createdBy)
    {
        CareProgram careProgram;
        if (dto.CareProgramId.HasValue)
        {
            var found = await _context.CarePrograms.FindAsync(dto.CareProgramId.Value);
            if (found == null)
                throw new ArgumentException($"Care program with ID {dto.CareProgramId} not found");
            careProgram = found;
        }
        else
        {
            // Default to ORH if no program specified
            careProgram = await _context.CarePrograms
                .FirstAsync(c => c.IsOrphanHoldingProgram);
        }

        // Generate unique CFF ID (sequential)
        var maxCffId = await _context.Patients.MaxAsync(p => (long?)p.CffId) ?? 100000;
        var cffId = maxCffId + 1;

        var patient = new Patient
        {
            RegistryId = Guid.NewGuid().ToString("N")[..10].ToUpper(),
            CffId = cffId,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            LastNameAtBirth = dto.LastNameAtBirth,
            DateOfBirth = dto.DateOfBirth,
            BiologicalSexAtBirth = dto.BiologicalSexAtBirth,
            Gender = dto.Gender ?? dto.BiologicalSexAtBirth,
            SsnLast4 = dto.SsnLast4,
            MedicalRecordNumber = dto.MedicalRecordNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Status = "Active",
            VitalStatus = "Alive",
            CareProgramId = careProgram.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Create program association
        var association = new PatientProgramAssignment
        {
            PatientId = patient.Id,
            ProgramId = careProgram.Id,
            Status = "Active",
            IsPrimaryProgram = true,
            EnrollmentDate = DateTime.UtcNow,
        };
        _context.PatientProgramAssignments.Add(association);
        await _context.SaveChangesAsync();

        // Reload with includes
        var created = await GetPatientByIdAsync(patient.Id);
        return created!;
    }

    public async Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto dto, string updatedBy)
    {
        var patient = await _context.Patients
            .Include(p => p.CareProgram)
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new ArgumentException($"Patient with ID {id} not found");

        patient.FirstName = dto.FirstName;
        patient.MiddleName = dto.MiddleName;
        patient.LastName = dto.LastName;
        patient.LastNameAtBirth = dto.LastNameAtBirth;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.BiologicalSexAtBirth = dto.BiologicalSexAtBirth;
        patient.Gender = dto.Gender;
        patient.MedicalRecordNumber = dto.MedicalRecordNumber;
        patient.Email = dto.Email;
        patient.Phone = dto.Phone;
        patient.Status = dto.Status;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = updatedBy;

        // Update vital status if status changed to Deceased
        if (dto.Status == "Deceased")
        {
            patient.IsDeceased = true;
            patient.DeceasedDate ??= DateTime.UtcNow;
            patient.VitalStatus = "Deceased";
        }

        await _context.SaveChangesAsync();
        return MapToDto(patient, null);
    }

    public async Task DeletePatientAsync(int id, string deletedBy)
    {
        var patient = await _context.Patients.FindAsync(id)
            ?? throw new ArgumentException($"Patient with ID {id} not found");

        patient.Status = "Inactive";
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
    }

    // ── Program Association Methods (04-002, 04-006, 04-007) ────

    public async Task<IEnumerable<PatientProgramAssociationDto>> GetProgramAssociationsAsync(int patientId)
    {
        var associations = await _context.PatientProgramAssignments
            .Include(pa => pa.Program)
            .Where(pa => pa.PatientId == patientId)
            .OrderByDescending(pa => pa.IsPrimaryProgram)
            .ThenBy(pa => pa.Program.Name)
            .ToListAsync();

        return associations.Select(a => new PatientProgramAssociationDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            ProgramId = a.ProgramId,
            ProgramName = a.Program.Name,
            LocalMRN = a.LocalMRN,
            Status = a.Status,
            IsPrimaryProgram = a.IsPrimaryProgram,
            EnrollmentDate = a.EnrollmentDate,
            DisenrollmentDate = a.DisenrollmentDate,
            RemovalReason = a.RemovalReason,
        });
    }

    public async Task<PatientProgramAssociationDto> AddToProgramAsync(
        int patientId, AddPatientToProgramDto dto, string addedBy)
    {
        // Check if association already exists
        var existing = await _context.PatientProgramAssignments
            .FirstOrDefaultAsync(pa =>
                pa.PatientId == patientId && pa.ProgramId == dto.ProgramId && pa.Status == "Active");

        if (existing != null)
            throw new InvalidOperationException("Patient is already associated with this program.");

        var program = await _context.CarePrograms.FindAsync(dto.ProgramId)
            ?? throw new ArgumentException($"Program {dto.ProgramId} not found");

        var association = new PatientProgramAssignment
        {
            PatientId = patientId,
            ProgramId = dto.ProgramId,
            LocalMRN = dto.LocalMRN,
            Status = "Active",
            IsPrimaryProgram = dto.IsPrimaryProgram,
            EnrollmentDate = DateTime.UtcNow,
        };

        _context.PatientProgramAssignments.Add(association);

        // If re-acquiring from ORH, remove ORH association
        var orhProgram = await _context.CarePrograms.FirstOrDefaultAsync(c => c.IsOrphanHoldingProgram);
        if (orhProgram != null && !program.IsOrphanHoldingProgram)
        {
            var orhAssoc = await _context.PatientProgramAssignments
                .FirstOrDefaultAsync(pa =>
                    pa.PatientId == patientId && pa.ProgramId == orhProgram.Id && pa.Status == "Active");
            if (orhAssoc != null)
            {
                orhAssoc.Status = "Removed";
                orhAssoc.DisenrollmentDate = DateTime.UtcNow;
                orhAssoc.RemovalReason = "Re-acquired by clinical program";
                orhAssoc.RemovedBy = addedBy;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Patient {PatientId} added to program {ProgramId} by {User}",
            patientId, dto.ProgramId, addedBy);

        return new PatientProgramAssociationDto
        {
            Id = association.Id,
            PatientId = association.PatientId,
            ProgramId = association.ProgramId,
            ProgramName = program.Name,
            LocalMRN = association.LocalMRN,
            Status = association.Status,
            IsPrimaryProgram = association.IsPrimaryProgram,
            EnrollmentDate = association.EnrollmentDate,
        };
    }

    public async Task RemoveFromProgramAsync(
        int patientId, int programId, RemovePatientFromProgramDto dto, string removedBy)
    {
        var association = await _context.PatientProgramAssignments
            .FirstOrDefaultAsync(pa =>
                pa.PatientId == patientId && pa.ProgramId == programId && pa.Status == "Active")
            ?? throw new ArgumentException("Active association not found.");

        // Handle consent withdrawal — remove from ALL programs
        if (dto.RemovalReason == "Patient withdrew consent")
        {
            var allAssociations = await _context.PatientProgramAssignments
                .Where(pa => pa.PatientId == patientId && pa.Status == "Active")
                .ToListAsync();

            foreach (var assoc in allAssociations)
            {
                assoc.Status = "Removed";
                assoc.DisenrollmentDate = DateTime.UtcNow;
                assoc.RemovalReason = dto.RemovalReason;
                assoc.RemovedBy = removedBy;
            }

            // Mark patient as consent withdrawn
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient != null)
            {
                patient.ConsentWithdrawn = true;
                patient.UpdatedAt = DateTime.UtcNow;
                patient.UpdatedBy = removedBy;
            }

            // Auto-associate with ORH
            await EnsureOrhAssociationAsync(patientId, removedBy);
        }
        else
        {
            // Remove from single program
            association.Status = "Removed";
            association.DisenrollmentDate = DateTime.UtcNow;
            association.RemovalReason = dto.RemovalReason;
            association.RemovedBy = removedBy;

            // Check if patient has any remaining active clinical associations
            var remainingActive = await _context.PatientProgramAssignments
                .Include(pa => pa.Program)
                .Where(pa =>
                    pa.PatientId == patientId &&
                    pa.Status == "Active" &&
                    pa.Id != association.Id &&
                    !pa.Program.IsOrphanHoldingProgram)
                .AnyAsync();

            if (!remainingActive)
                await EnsureOrhAssociationAsync(patientId, removedBy);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Patient {PatientId} removed from program {ProgramId}. Reason: {Reason}. By: {User}",
            patientId, programId, dto.RemovalReason, removedBy);
    }

    // ── Duplicate Detection (04-005) ─────────────────────────────

    public async Task<IEnumerable<DuplicateMatchDto>> CheckDuplicatesAsync(DuplicateCheckDto dto)
    {
        var matches = new List<DuplicateMatchDto>();

        var candidates = await _context.Patients
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .Where(p => !p.ConsentWithdrawn)
            .ToListAsync();

        // Approach A: Registry ID provided
        if (!string.IsNullOrEmpty(dto.RegistryId))
        {
            var regMatch = candidates.FirstOrDefault(p =>
                p.RegistryId.Equals(dto.RegistryId, StringComparison.OrdinalIgnoreCase) ||
                p.CffId.ToString() == dto.RegistryId);

            if (regMatch != null)
            {
                // Require one additional field match
                bool additionalMatch =
                    (dto.FirstName != null && regMatch.FirstName.Equals(dto.FirstName, StringComparison.OrdinalIgnoreCase)) ||
                    (dto.LastName != null && regMatch.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase)) ||
                    (dto.DateOfBirth.HasValue && regMatch.DateOfBirth.Date == dto.DateOfBirth.Value.Date);

                if (additionalMatch)
                {
                    matches.Add(MapToMatch(regMatch, 1.0, "Exact Registry ID match with field confirmation"));
                    return matches;
                }
            }
        }

        // Approach B: Fuzzy matching
        if (dto.FirstName == null && dto.LastName == null && !dto.DateOfBirth.HasValue)
            return matches;

        foreach (var p in candidates)
        {
            double score = 0;
            var reasons = new List<string>();

            // Rule 1: Exact name + exact DOB (highest confidence)
            if (dto.FirstName != null && dto.LastName != null && dto.DateOfBirth.HasValue &&
                p.FirstName.Equals(dto.FirstName, StringComparison.OrdinalIgnoreCase) &&
                p.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                p.DateOfBirth.Date == dto.DateOfBirth.Value.Date)
            {
                score = 0.95;
                reasons.Add("Exact name + exact DOB");
            }
            // Rule 2: Exact last name + exact DOB + sex match
            else if (dto.LastName != null && dto.DateOfBirth.HasValue && dto.BiologicalSexAtBirth != null &&
                     p.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                     p.DateOfBirth.Date == dto.DateOfBirth.Value.Date &&
                     (p.BiologicalSexAtBirth ?? p.Gender ?? "").Equals(dto.BiologicalSexAtBirth, StringComparison.OrdinalIgnoreCase))
            {
                score = 0.85;
                reasons.Add("Exact last name + exact DOB + sex match");
            }
            // Rule 3: Exact last name + close DOB (within 2 days)
            else if (dto.LastName != null && dto.DateOfBirth.HasValue &&
                     p.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                     Math.Abs((p.DateOfBirth.Date - dto.DateOfBirth.Value.Date).TotalDays) <= 2)
            {
                score = 0.75;
                reasons.Add("Exact last name + close DOB (±2 days)");
            }
            // Rule 4: Similar last name (Levenshtein ≤ 2) + exact DOB
            else if (dto.LastName != null && dto.DateOfBirth.HasValue &&
                     LevenshteinDistance(p.LastName.ToLower(), dto.LastName.ToLower()) <= 2 &&
                     p.DateOfBirth.Date == dto.DateOfBirth.Value.Date)
            {
                score = 0.7;
                reasons.Add("Similar last name + exact DOB");
            }
            // Rule 5: Exact first name + exact last name + same year of birth
            else if (dto.FirstName != null && dto.LastName != null && dto.DateOfBirth.HasValue &&
                     p.FirstName.Equals(dto.FirstName, StringComparison.OrdinalIgnoreCase) &&
                     p.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                     p.DateOfBirth.Year == dto.DateOfBirth.Value.Year)
            {
                score = 0.65;
                reasons.Add("Exact name + same birth year");
            }
            // Rule 6: Last name at birth match + DOB match
            else if (dto.LastName != null && dto.DateOfBirth.HasValue &&
                     p.LastNameAtBirth != null &&
                     p.LastNameAtBirth.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                     p.DateOfBirth.Date == dto.DateOfBirth.Value.Date)
            {
                score = 0.6;
                reasons.Add("Last name at birth match + DOB");
            }
            // Rule 7: Phonetic last name match (Soundex) + close DOB
            else if (dto.LastName != null && dto.DateOfBirth.HasValue &&
                     Soundex(p.LastName) == Soundex(dto.LastName) &&
                     Math.Abs((p.DateOfBirth.Date - dto.DateOfBirth.Value.Date).TotalDays) <= 30)
            {
                score = 0.5;
                reasons.Add("Phonetic last name match + close DOB (±30 days)");
            }
            // Rule 8: Similar first name + exact last name + close DOB
            else if (dto.FirstName != null && dto.LastName != null && dto.DateOfBirth.HasValue &&
                     LevenshteinDistance(p.FirstName.ToLower(), dto.FirstName.ToLower()) <= 2 &&
                     p.LastName.Equals(dto.LastName, StringComparison.OrdinalIgnoreCase) &&
                     Math.Abs((p.DateOfBirth.Date - dto.DateOfBirth.Value.Date).TotalDays) <= 7)
            {
                score = 0.45;
                reasons.Add("Similar first name + exact last name + close DOB");
            }

            if (score >= 0.4)
                matches.Add(MapToMatch(p, score, string.Join("; ", reasons)));
        }

        return matches.OrderByDescending(m => m.ConfidenceScore).Take(10);
    }

    // ── Merge Methods (04-008, 04-009, 04-010) ───────────────────

    public async Task<MergeResultDto> MergeAsync(MergeRequestDto dto, string mergedBy)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var primary = await _context.Patients
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .Include(p => p.Aliases)
            .FirstOrDefaultAsync(p => p.Id == dto.PrimaryPatientId)
            ?? throw new ArgumentException("Primary patient not found");

        var secondary = await _context.Patients
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .FirstOrDefaultAsync(p => p.Id == dto.SecondaryPatientId)
            ?? throw new ArgumentException("Secondary patient not found");

        int aliasesCreated = 0;
        int associationsMerged = 0;

        // 1. Create aliases from secondary record
        _context.PatientAliases.Add(new PatientAlias
        {
            PatientId = primary.Id,
            AliasType = "CffId",
            AliasValue = secondary.CffId.ToString(),
            Source = $"Merge from patient #{secondary.Id}",
            CreatedAt = DateTime.UtcNow,
        });
        _context.PatientAliases.Add(new PatientAlias
        {
            PatientId = primary.Id,
            AliasType = "RegistryId",
            AliasValue = secondary.RegistryId,
            Source = $"Merge from patient #{secondary.Id}",
            CreatedAt = DateTime.UtcNow,
        });
        if (secondary.LastName != primary.LastName)
        {
            _context.PatientAliases.Add(new PatientAlias
            {
                PatientId = primary.Id,
                AliasType = "Name",
                AliasValue = $"{secondary.FirstName} {secondary.LastName}",
                Source = $"Merge from patient #{secondary.Id}",
                CreatedAt = DateTime.UtcNow,
            });
        }
        aliasesCreated = 2 + (secondary.LastName != primary.LastName ? 1 : 0);

        // 2. Merge program associations (union)
        foreach (var secAssoc in secondary.ProgramAssignments.Where(a => a.Status == "Active"))
        {
            var alreadyExists = primary.ProgramAssignments
                .Any(pa => pa.ProgramId == secAssoc.ProgramId && pa.Status == "Active");

            if (!alreadyExists)
            {
                _context.PatientProgramAssignments.Add(new PatientProgramAssignment
                {
                    PatientId = primary.Id,
                    ProgramId = secAssoc.ProgramId,
                    LocalMRN = secAssoc.LocalMRN,
                    Status = "Active",
                    IsPrimaryProgram = false,
                    EnrollmentDate = secAssoc.EnrollmentDate,
                });
                associationsMerged++;
            }
        }

        // 3. Mark secondary as merged (inactive, not visible)
        secondary.Status = "Merged";
        secondary.UpdatedAt = DateTime.UtcNow;
        secondary.UpdatedBy = mergedBy;

        // Remove secondary from all programs
        foreach (var assoc in secondary.ProgramAssignments.Where(a => a.Status == "Active"))
        {
            assoc.Status = "Removed";
            assoc.DisenrollmentDate = DateTime.UtcNow;
            assoc.RemovalReason = $"Merged into patient #{primary.Id}";
            assoc.RemovedBy = mergedBy;
        }

        primary.UpdatedAt = DateTime.UtcNow;
        primary.UpdatedBy = mergedBy;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation(
            "Patient merge completed: primary={PrimaryId}, secondary={SecondaryId}, by={User}",
            primary.Id, secondary.Id, mergedBy);

        return new MergeResultDto
        {
            PrimaryPatientId = primary.Id,
            SecondaryPatientId = secondary.Id,
            AliasesCreated = aliasesCreated,
            AssociationsMerged = associationsMerged,
            Status = "Completed",
        };
    }

    // ── Private Helpers ──────────────────────────────────────────

    private async Task EnsureOrhAssociationAsync(int patientId, string createdBy)
    {
        var orhProgram = await _context.CarePrograms.FirstOrDefaultAsync(c => c.IsOrphanHoldingProgram);
        if (orhProgram == null) return;

        var existing = await _context.PatientProgramAssignments
            .FirstOrDefaultAsync(pa =>
                pa.PatientId == patientId && pa.ProgramId == orhProgram.Id && pa.Status == "Active");

        if (existing == null)
        {
            _context.PatientProgramAssignments.Add(new PatientProgramAssignment
            {
                PatientId = patientId,
                ProgramId = orhProgram.Id,
                Status = "Active",
                IsPrimaryProgram = false,
                EnrollmentDate = DateTime.UtcNow,
            });
        }
    }

    private PatientDto MapToDto(Patient p, int? excludeProgramId)
    {
        var activeAssociations = p.ProgramAssignments
            .Where(pa => pa.Status == "Active")
            .ToList();

        var otherPrograms = activeAssociations
            .Where(pa => pa.ProgramId != (excludeProgramId ?? -1) && !pa.Program.IsOrphanHoldingProgram)
            .Select(pa => pa.Program.Name)
            .ToList();

        return new PatientDto
        {
            Id = p.Id,
            RegistryId = p.RegistryId,
            CffId = p.CffId,
            FirstName = p.FirstName,
            MiddleName = p.MiddleName,
            LastName = p.LastName,
            LastNameAtBirth = p.LastNameAtBirth,
            DateOfBirth = p.DateOfBirth,
            BiologicalSexAtBirth = p.BiologicalSexAtBirth,
            Gender = p.Gender,
            MedicalRecordNumber = p.MedicalRecordNumber,
            Email = p.Email,
            Phone = p.Phone,
            Status = p.Status,
            Diagnosis = p.Diagnosis,
            VitalStatus = p.VitalStatus,
            CareProgramId = p.CareProgramId,
            CareProgramName = p.CareProgram?.Name ?? "",
            LastModifiedBy = p.UpdatedBy,
            LastModifiedDate = p.UpdatedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            OtherPrograms = otherPrograms,
        };
    }

    private static DuplicateMatchDto MapToMatch(Patient p, double score, string reason)
    {
        var activeAssociations = p.ProgramAssignments
            .Where(pa => pa.Status == "Active")
            .ToList();

        return new DuplicateMatchDto
        {
            PatientId = p.Id,
            RegistryId = p.RegistryId,
            CffId = p.CffId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            LastNameAtBirth = p.LastNameAtBirth,
            DateOfBirth = p.DateOfBirth,
            BiologicalSexAtBirth = p.BiologicalSexAtBirth,
            ProgramAssociations = activeAssociations.Select(pa => pa.Program.Name).ToList(),
            IsOrh = activeAssociations.Any(pa => pa.Program.IsOrphanHoldingProgram) &&
                    activeAssociations.Count(pa => !pa.Program.IsOrphanHoldingProgram) == 0,
            ConfidenceScore = score,
            MatchReason = reason,
        };
    }

    private static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
        if (string.IsNullOrEmpty(t)) return s.Length;

        var d = new int[s.Length + 1, t.Length + 1];
        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        for (int j = 1; j <= t.Length; j++)
        {
            int cost = s[i - 1] == t[j - 1] ? 0 : 1;
            d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
        }

        return d[s.Length, t.Length];
    }

    private static string Soundex(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var result = new char[4];
        result[0] = char.ToUpper(s[0]);
        var map = "01230120022455012623010202";
        int idx = 1;
        char last = GetSoundexCode(s[0], map);

        for (int i = 1; i < s.Length && idx < 4; i++)
        {
            char code = GetSoundexCode(s[i], map);
            if (code != '0' && code != last)
            {
                result[idx++] = code;
            }
            last = code;
        }

        while (idx < 4) result[idx++] = '0';
        return new string(result);
    }

    private static char GetSoundexCode(char c, string map)
    {
        c = char.ToUpper(c);
        return c >= 'A' && c <= 'Z' ? map[c - 'A'] : '0';
    }
}
