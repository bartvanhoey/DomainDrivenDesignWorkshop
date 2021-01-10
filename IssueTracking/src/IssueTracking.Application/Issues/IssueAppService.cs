using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracking.Application.Contracts.Issues;
using IssueTracking.Domain.Issues;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Application.Issues
{
  public class IssueAppService : ApplicationService, IIssueAppService
  {
    private readonly IIssueRepository _issueRepository;

    public IssueAppService(IIssueRepository issueRepository)
    {
      _issueRepository = issueRepository;
    }
    public async Task<IssueDto> CreateAsync(CreateIssueDto input)
    {
      var issue = new Issue
      {
        RepositoryId = input.RepositoryId,
        Title = input.Title,
        Text = input.Text
      };

      await _issueRepository.InsertAsync(issue);
      return ObjectMapper.Map<Issue, IssueDto>(issue);
    }

    public async Task<IssueDto> GetAsync(Guid id)
    {
      var issue = await _issueRepository.GetAsync(id);
      return ObjectMapper.Map<Issue, IssueDto>(issue);
    }
    public async Task<PagedResultDto<IssueDto>> GetListAsync(GetIssueListDto input)
    {
      if (input.Sorting.IsNullOrWhiteSpace())
      {
        input.Sorting = nameof(Issue.Title);
      }

      var issues = await _issueRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, includeDetails: true);
      var totalCount = await AsyncExecuter.CountAsync(_issueRepository.WhereIf(!input.Filter.IsNullOrWhiteSpace(), issue => issue.Title.Contains(input.Filter)));

      List<IssueDto> items = ObjectMapper.Map<List<Issue>, List<IssueDto>>(issues);

      return new PagedResultDto<IssueDto>(totalCount, items);
    }

    public async Task DeleteAsync(Guid id)
    {
      await _issueRepository.DeleteAsync(id);
    }

    public async Task UpdateAsync(Guid id, UpdateIssueDto input)
    {
      var issue = await _issueRepository.GetAsync(id);

      issue.Title = input.Title;
      issue.Text = input.Text;
      issue.AssignedUserId = input.AssignedUserId;

      await _issueRepository.UpdateAsync(issue);
    }
  }
}