using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Domain.Issues
{
  public class UserIssueService : IUserIssueService, ITransientDependency
  {
    private readonly IIssueRepository _issueRepository;

    public UserIssueService(IIssueRepository issueRepository)
    {
      _issueRepository = issueRepository;
    }

    public async Task<int> GetOpenIssueCountAsync(Guid userId)
    {
      return (await _issueRepository.GetIssuesAsync(new OpenIssuesOfUserSpecification(userId))).Count;
    }
  }
}