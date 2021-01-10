using System;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Domain.Issues
{
    public interface IIssueRepository :  IRepository<Issue, Guid>
    {
         
    }
}