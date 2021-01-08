using System;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Application.Contracts.Issues
{
    public class CreateCommentDto : EntityDto<Guid>
  {
    public Guid IssueId { get; set; }
    public string Text { get; set; }
  }
}