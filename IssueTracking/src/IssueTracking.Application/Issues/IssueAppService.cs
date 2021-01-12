using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracking.Application.Contracts.Issues;
using IssueTracking.Domain.Issues;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace IssueTracking.Application.Issues
{
  public class IssueAppService : ApplicationService, IIssueAppService
  {
    private readonly IIssueRepository _issueRepository;
    private readonly IGuidGenerator _guidGenerator;

    public IssueAppService(IIssueRepository issueRepository, IGuidGenerator guidGenerator)
    {
      _guidGenerator = guidGenerator;
      _issueRepository = issueRepository;
    }
    private readonly IGuidGenerator _;
    public async Task<IssueDto> CreateAsync(CreateIssueDto input)
    {
      var issue = new Issue(_guidGenerator.Create(), input.RepositoryId, input.Title, input.Text);

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

    [Authorize]
    public async Task CreateCommentAsync(CreateCommentDto input)
    {
      var issue = await _issueRepository.GetAsync(input.IssueId);
      issue.AddComment(CurrentUser.GetId(), input.Text);
      await _issueRepository.UpdateAsync(issue);
    }
  }
}