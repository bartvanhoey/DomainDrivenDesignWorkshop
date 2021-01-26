using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Specifications;

namespace IssueTracking.Domain.Issues
{
  public interface IIssueRepository : IRepository<Issue, Guid>
  {
    Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec);
  }
}