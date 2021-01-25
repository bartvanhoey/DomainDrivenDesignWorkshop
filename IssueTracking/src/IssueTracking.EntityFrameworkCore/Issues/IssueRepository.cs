using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracking.Domain.Issues;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IssueTracking.EntityFrameworkCore.Issues
{

  public class IssueRepository : EfCoreRepository<IssueTrackingDbContext, Issue, Guid>, IIssueRepository
  {
    public IssueRepository(IDbContextProvider<IssueTrackingDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<Issue>> GetInActiveIssuesAsync()
    {
      var daysAgo30 = DateTime.Now.Subtract(TimeSpan.FromDays(30));

      return await DbSet.Where(i =>

          //Open
          !i.IsClosed &&

          //Assigned to nobody
          i.AssignedUserId == null &&

          //Created 30+ days ago
          i.CreationTime < daysAgo30 &&

          //No comment or the last comment was 30+ days ago
          (i.LastCommentTime == null || i.LastCommentTime < daysAgo30)

      ).ToListAsync();
    }

    public override IQueryable<Issue> WithDetails()
    {
      return GetQueryable().IncludeDetails();
    }
  }
}