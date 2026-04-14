using AutoMapper;
using QAStudio.Application.Recording.DTOs;
using QAStudio.Domain.Entities;

namespace QAStudio.Infrastructure.Mappings;

public class RecordingMappingProfile : Profile
{
    public RecordingMappingProfile()
    {
        CreateMap<RecordingSession, RecordingSessionDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.Creator != null ? s.Creator.Name : null))
            .ForMember(d => d.TargetEnvironmentName, o => o.MapFrom(s => s.TargetEnvironment != null ? s.TargetEnvironment.Name : null));
    }
}
