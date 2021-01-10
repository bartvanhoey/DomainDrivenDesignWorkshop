using System;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Application.Contracts.Issues
{
  public class CommentDto : EntityDto<Guid>
  {
    public string Text { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid IssueId { get; set; }
    public Guid UserId { get; set; }
  }
}