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

          //Open
          !i.IsClosed &&

          //Assigned to nobody
          i.AssignedUserId == null &&

          //Created 30+ days ago
          i.CreationTime < daysAgo30 &&

          //No comment or the last comment was 30+ days ago
          (i.LastCommentTime == null || i.LastCommentTime < daysAgo30);

    }
  }
}