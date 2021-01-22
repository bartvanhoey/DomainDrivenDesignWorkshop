using AutoMapper;
using IssueTracking.Application.Contracts.Issues;

namespace IssueTracking.Blazor
{
  public class IssueTrackingBlazorAutoMapperProfile : Profile
  {
    public IssueTrackingBlazorAutoMapperProfile()
    {
      CreateMap<IssueDto, UpdateIssueDto>();

      CreateMap<IssueDto, CreateCommentDto>()
      .ForMember(x => x.IssueId, x => x.MapFrom(src => src.Id))
      .ForAllMembers(opt => opt.Ignore());
    }
  }
}
