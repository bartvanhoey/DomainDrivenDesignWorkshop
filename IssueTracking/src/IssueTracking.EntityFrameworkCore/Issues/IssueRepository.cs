using System;
using System.Linq;
using IssueTracking.Domain.Issues;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IssueTracking.EntityFrameworkCore.Issues
{
  public class EfCoreIssueRepository : EfCoreRepository<IssueTrackingDbContext, Issue, Guid>, IIssueRepository
  {
    public EfCoreIssueRepository(IDbContextProvider<IssueTrackingDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
    
    public override IQueryable<Issue> WithDetails()
    {
      return GetQueryable().IncludeDetails(); // Uses the extension method defined above
    }

  }
}