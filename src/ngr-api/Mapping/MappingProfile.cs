using AutoMapper;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Mapping;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.CareProgramName, opt => opt.MapFrom(src => src.CareProgram.Name));

        CreateMap<CreatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RegistryId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeceased, opt => opt.Ignore())
            .ForMember(dest => dest.DeceasedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgram, opt => opt.Ignore())
            .ForMember(dest => dest.Demographics, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Encounters, opt => opt.Ignore());

        CreateMap<UpdatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RegistryId, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgramId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeceased, opt => opt.Ignore())
            .ForMember(dest => dest.DeceasedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CareProgram, opt => opt.Ignore())
            .ForMember(dest => dest.Demographics, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Encounters, opt => opt.Ignore());
    }
}
