using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IssueTracking.Domain.Issues
{
  public class InActiveIssueSpecification : Specification<Issue>
  {
    public override Expression<Func<Issue, bool>> ToExpression()
    {
       var daysAgo30 = DateTime.Now.Subtract(TimeSpan.FromDays(30));

       return i => 
            
            !i.IsClosed &&

            i.AssignedUserId == null &&

            i.CreationTime < daysAgo30 &&

            (i.LastCommentTime == null || i.LastCommentTime < daysAgo30);
    }
  }
}