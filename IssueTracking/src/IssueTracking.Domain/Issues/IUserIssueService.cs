using System;
using System.Threading.Tasks;

namespace IssueTracking.Domain.Issues
{
    public interface IUserIssueService
    {
          Task<int> GetOpenIssueCountAsync(Guid userId);
    }
    

    
}