using System;
using Volo.Abp.Domain.Entities;

namespace IssueTracking.Domain.Issues
{
    public class Comment  : Entity<Guid>
    {
        public string Text { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid IssueId { get; set; }
        public Guid UserId { get; set; }
    }
}