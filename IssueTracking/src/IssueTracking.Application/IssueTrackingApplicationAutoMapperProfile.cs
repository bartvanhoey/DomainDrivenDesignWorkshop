using AutoMapper;
using IssueTracking.Application.Contracts.Issues;
using IssueTracking.Domain.Issues;

namespace IssueTracking
{
  public class IssueTrackingApplicationAutoMapperProfile : Profile
  {
    public IssueTrackingApplicationAutoMapperProfile()
    {
      /* You can configure your AutoMapper mapping configuration here.
       * Alternatively, you can split your mapping configurations
       * into multiple profile classes for a better organization. */

      CreateMap<Issue, IssueDto>()
        .ForMember(x => x.IsActive, x => x.MapFrom(src => !src.IsInActive()));
      
      CreateMap<Comment, CommentDto>();
    }
  }
}
