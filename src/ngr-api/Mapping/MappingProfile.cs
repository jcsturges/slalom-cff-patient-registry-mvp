using AutoMapper;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Mapping;

/// <summary>
/// AutoMapper profile for entity to DTO mappings.
/// Note: PatientDto mapping is done manually in PatientService for complex logic
/// (OtherPrograms, LastModifiedBy). This profile is kept for CareProgram mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Patient → PatientDto (basic mapping; complex mapping done in PatientService)
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.CareProgramName, opt => opt.MapFrom(src => src.CareProgram.Name))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.OtherPrograms, opt => opt.Ignore());

        CreateMap<CreatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RegistryId, opt => opt.Ignore())
            .ForMember(dest => dest.CffId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.VitalStatus, opt => opt.MapFrom(_ => "Alive"))
            .ForMember(dest => dest.ConsentWithdrawn, opt => opt.Ignore())
            .ForMember(dest => dest.Diagnosis, opt => opt.Ignore())
            .ForMember(dest => dest.AldStatus, opt => opt.Ignore())
            .ForMember(dest => dest.HasLungTransplant, opt => opt.Ignore())
            .ForMember(dest => dest.LastSeenInProgram, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeceased, opt => opt.Ignore())
            .ForMember(dest => dest.DeceasedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgram, opt => opt.Ignore())
            .ForMember(dest => dest.Demographics, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Aliases, opt => opt.Ignore())
            .ForMember(dest => dest.Encounters, opt => opt.Ignore());

        CreateMap<UpdatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RegistryId, opt => opt.Ignore())
            .ForMember(dest => dest.CffId, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgramId, opt => opt.Ignore())
            .ForMember(dest => dest.SsnLast4, opt => opt.Ignore())
            .ForMember(dest => dest.ConsentWithdrawn, opt => opt.Ignore())
            .ForMember(dest => dest.Diagnosis, opt => opt.Ignore())
            .ForMember(dest => dest.VitalStatus, opt => opt.Ignore())
            .ForMember(dest => dest.AldStatus, opt => opt.Ignore())
            .ForMember(dest => dest.HasLungTransplant, opt => opt.Ignore())
            .ForMember(dest => dest.LastSeenInProgram, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeceased, opt => opt.Ignore())
            .ForMember(dest => dest.DeceasedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgram, opt => opt.Ignore())
            .ForMember(dest => dest.Demographics, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Aliases, opt => opt.Ignore())
            .ForMember(dest => dest.Encounters, opt => opt.Ignore());

        // CareProgram mappings
        CreateMap<CareProgram, CareProgramDto>();

        CreateMap<CreateCareProgramDto, CareProgram>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.IsOrphanHoldingProgram, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.AccreditationDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
