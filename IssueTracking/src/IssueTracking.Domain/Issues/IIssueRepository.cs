using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Domain.Issues
{
    public interface IIssueRepository :  IRepository<Issue, Guid>
    {
         Task<List<Issue>> GetInActiveIssuesAsync();
    }
}