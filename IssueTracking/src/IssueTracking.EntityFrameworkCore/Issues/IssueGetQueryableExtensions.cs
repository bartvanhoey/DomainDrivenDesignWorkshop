using System.Linq;
using IssueTracking.Domain.Issues;
using Microsoft.EntityFrameworkCore;

namespace IssueTracking.EntityFrameworkCore.Issues
{
    public static class IssueGetQueryableExtensions
    {
        public static IQueryable<Issue> IncludeDetails(this IQueryable<Issue> queryable, bool include = true)
        {
            if (!include) return queryable;
            return queryable.Include(x => x.Comments);
        }
    }
}