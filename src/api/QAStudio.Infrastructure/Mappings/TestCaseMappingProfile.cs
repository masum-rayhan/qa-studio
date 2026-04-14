using AutoMapper;
using QAStudio.Application.TestCases.DTOs;
using QAStudio.Domain.Entities;

namespace QAStudio.Infrastructure.Mappings;

public class TestCaseMappingProfile : Profile
{
    public TestCaseMappingProfile()
    {
        CreateMap<TestCase, TestCaseDto>()
            .ForMember(d => d.TargetEnvironmentName, o => o.MapFrom(s => s.TargetEnvironment != null ? s.TargetEnvironment.Name : null))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.Creator != null ? s.Creator.Name : null));

        CreateMap<CreateTestCaseDto, TestCase>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.TargetEnvironment, o => o.Ignore())
            .ForMember(d => d.TestRuns, o => o.Ignore())
            .ForMember(d => d.TestSchedules, o => o.Ignore());

        CreateMap<UpdateTestCaseDto, TestCase>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.TargetEnvironment, o => o.Ignore())
            .ForMember(d => d.TestRuns, o => o.Ignore())
            .ForMember(d => d.TestSchedules, o => o.Ignore());
    }
}
