using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IssueTracking.Application.Contracts.Issues
{
  public interface IIssueAppService : IApplicationService
  {
    Task<IssueDto> GetAsync(Guid id);

    Task<PagedResultDto<IssueDto>> GetListAsync(GetIssueListDto input);

    Task<IssueDto> CreateAsync(CreateIssueDto input);

    Task UpdateAsync(Guid id, UpdateIssueDto input);

    Task DeleteAsync(Guid id);

    Task CreateCommentAsync(CreateCommentDto input);
  }
}