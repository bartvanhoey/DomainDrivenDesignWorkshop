using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IssueTracking.Domain.Issues
{
    public class MileStoneSpecification : Specification<Issue>
    {
        public Guid MileStoneId { get; }

        public MileStoneSpecification(Guid mileStoneId)
        {
            MileStoneId = mileStoneId;
        }

        public override Expression<Func<Issue, bool>> ToExpression()
        {
            return i => i.MileStoneId == MileStoneId;
        }
    }
}