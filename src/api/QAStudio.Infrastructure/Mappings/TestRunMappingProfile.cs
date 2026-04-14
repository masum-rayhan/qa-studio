using AutoMapper;
using QAStudio.Application.TestRuns.DTOs;
using QAStudio.Domain.Entities;

namespace QAStudio.Infrastructure.Mappings;

public class TestRunMappingProfile : Profile
{
    public TestRunMappingProfile()
    {
        CreateMap<TestRun, TestRunDto>()
            .ForMember(d => d.TestCaseName, o => o.MapFrom(s => s.TestCase != null ? s.TestCase.Name : null))
            .ForMember(d => d.EnvironmentName, o => o.MapFrom(s => s.Environment != null ? s.Environment.Name : null))
            .ForMember(d => d.TriggeredByName, o => o.MapFrom(s => s.TriggeredByUser != null ? s.TriggeredByUser.Name : null));

        CreateMap<TestResult, TestResultDto>();

        CreateMap<CreateTestRunDto, TestRun>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.StartedAt, o => o.Ignore())
            .ForMember(d => d.CompletedAt, o => o.Ignore())
            .ForMember(d => d.TotalDurationMs, o => o.Ignore())
            .ForMember(d => d.VideoPath, o => o.Ignore())
            .ForMember(d => d.TracePath, o => o.Ignore())
            .ForMember(d => d.TestCase, o => o.Ignore())
            .ForMember(d => d.Environment, o => o.Ignore())
            .ForMember(d => d.TriggeredByUser, o => o.Ignore())
            .ForMember(d => d.Results, o => o.Ignore());
    }
}
