using System;
using System.Collections.Generic;
using IssueTracking.Domain.Shared.Issues;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Application.Contracts.Issues
{
  public class IssueDto  : EntityDto<Guid>
  {
    public Guid RepositoryId { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool IsClosed { get; set; }
    public Guid? AssignedUserId { get; set; }
    public bool IsLocked { get; set; }
    public IssueCloseReason? CloseReason { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastCommentTime { get; set; }
    public Guid MileStoneId { get; set; }
  }
}