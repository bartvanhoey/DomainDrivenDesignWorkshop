using System;
using System.ComponentModel.DataAnnotations;

namespace IssueTracking.Application.Contracts.Issues
{
  public class UpdateIssueDto
  {

    [Required] 
    public string Title { get; set; }

    public string Text { get; set; }

    public Guid? AssignedUserId { get; set; }
  }
}