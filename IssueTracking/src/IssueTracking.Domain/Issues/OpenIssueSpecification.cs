using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IssueTracking.Domain.Issues
{
  public class OpenIssuesOfUserSpecification : Specification<Issue>
  {
    public  Guid UserId { get; }

    public OpenIssuesOfUserSpecification(Guid userId)
    {
      UserId = userId;
    }

    public override Expression<Func<Issue, bool>> ToExpression()
    {
      return i => !i.IsClosed && i.AssignedUserId == UserId;
    }
  }
}