using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

/// <summary>
/// Service implementation for patient operations
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

    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(
        int? careProgramId,
        string? status,
        string? searchTerm,
        int page,
        int pageSize)
    {
        try
        {
            var query = _context.Patients
                .Include(p => p.CareProgram)
                .AsQueryable();

            if (careProgramId.HasValue)
            {
                query = query.Where(p => p.CareProgramId == careProgramId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    p.LastName.ToLower().Contains(lowerSearchTerm) ||
                    (p.MedicalRecordNumber != null && p.MedicalRecordNumber.ToLower().Contains(lowerSearchTerm)));
            }

            var patients = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                MedicalRecordNumber = p.MedicalRecordNumber,
                Gender = p.Gender,
                Email = p.Email,
                Phone = p.Phone,
                Status = p.Status,
                CareProgramId = p.CareProgramId,
                CareProgramName = p.CareProgram.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            throw;
        }
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.CareProgram)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return null;
            }

            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                MedicalRecordNumber = patient.MedicalRecordNumber,
                Gender = patient.Gender,
                Email = patient.Email,
                Phone = patient.Phone,
                Status = patient.Status,
                CareProgramId = patient.CareProgramId,
                CareProgramName = patient.CareProgram.Name,
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto, string createdBy)
    {
        try
        {
            var careProgram = await _context.CarePrograms.FindAsync(createPatientDto.CareProgramId);
            if (careProgram == null)
            {
                throw new ArgumentException($"Care program with ID {createPatientDto.CareProgramId} not found");
            }

            var patient = new Patient
            {
                RegistryId = Guid.NewGuid().ToString("N")[..10].ToUpper(),
                FirstName = createPatientDto.FirstName,
                LastName = createPatientDto.LastName,
                DateOfBirth = createPatientDto.DateOfBirth,
                MedicalRecordNumber = createPatientDto.MedicalRecordNumber,
                Gender = createPatientDto.Gender,
                Email = createPatientDto.Email,
                Phone = createPatientDto.Phone,
                Status = "Active",
                CareProgramId = createPatientDto.CareProgramId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                MedicalRecordNumber = patient.MedicalRecordNumber,
                Gender = patient.Gender,
                Email = patient.Email,
                Phone = patient.Phone,
                Status = patient.Status,
                CareProgramId = patient.CareProgramId,
                CareProgramName = careProgram.Name,
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            throw;
        }
    }

    public async Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto, string updatedBy)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.CareProgram)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new ArgumentException($"Patient with ID {id} not found");
            }

            patient.FirstName = updatePatientDto.FirstName;
            patient.LastName = updatePatientDto.LastName;
            patient.DateOfBirth = updatePatientDto.DateOfBirth;
            patient.MedicalRecordNumber = updatePatientDto.MedicalRecordNumber;
            patient.Gender = updatePatientDto.Gender;
            patient.Email = updatePatientDto.Email;
            patient.Phone = updatePatientDto.Phone;
            patient.Status = updatePatientDto.Status;
            patient.UpdatedAt = DateTime.UtcNow;
            patient.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                MedicalRecordNumber = patient.MedicalRecordNumber,
                Gender = patient.Gender,
                Email = patient.Email,
                Phone = patient.Phone,
                Status = patient.Status,
                CareProgramId = patient.CareProgramId,
                CareProgramName = patient.CareProgram.Name,
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task DeletePatientAsync(int id, string deletedBy)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                throw new ArgumentException($"Patient with ID {id} not found");
            }

            // Soft delete
            patient.Status = "Inactive";
            patient.UpdatedAt = DateTime.UtcNow;
            patient.UpdatedBy = deletedBy;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task<int> GetPatientCountAsync(int? careProgramId)
    {
        try
        {
            var query = _context.Patients.AsQueryable();

            if (careProgramId.HasValue)
            {
                query = query.Where(p => p.CareProgramId == careProgramId.Value);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient count");
            throw;
        }
    }
}
