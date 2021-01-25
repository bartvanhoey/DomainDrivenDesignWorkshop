using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracking.Domain.Issues;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Specifications;

namespace IssueTracking.EntityFrameworkCore.Issues
{

  public class IssueRepository : EfCoreRepository<IssueTrackingDbContext, Issue, Guid>, IIssueRepository
  {
    public IssueRepository(IDbContextProvider<IssueTrackingDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec)
    {
      return await DbSet.Where(spec.ToExpression()).ToListAsync();
    }

    public override IQueryable<Issue> WithDetails()
    {
      return GetQueryable().IncludeDetails();
    }
  }
}