using AutoMapper;
using QAStudio.Application.Environments.DTOs;
using QAStudio.Domain.Entities;

namespace QAStudio.Infrastructure.Mappings;

public class EnvironmentMappingProfile : Profile
{
    public EnvironmentMappingProfile()
    {
        CreateMap<TargetEnvironment, EnvironmentDto>()
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.Creator != null ? s.Creator.Name : null));

        CreateMap<CreateEnvironmentDto, TargetEnvironment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.TestCases, o => o.Ignore())
            .ForMember(d => d.TestRuns, o => o.Ignore())
            .ForMember(d => d.TestSchedules, o => o.Ignore());

        CreateMap<UpdateEnvironmentDto, TargetEnvironment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.TestCases, o => o.Ignore())
            .ForMember(d => d.TestRuns, o => o.Ignore())
            .ForMember(d => d.TestSchedules, o => o.Ignore());
    }
}
