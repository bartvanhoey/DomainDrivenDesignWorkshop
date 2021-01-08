using System;

namespace IssueTracking.Domain.Issues
{
  public class IssueLabel
  {
    public Guid IssueId { get; set; }
    public Guid LabelId { get; set; }
  }
}