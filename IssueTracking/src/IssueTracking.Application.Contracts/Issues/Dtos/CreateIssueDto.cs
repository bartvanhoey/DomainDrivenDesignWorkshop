using System;
using System.ComponentModel.DataAnnotations;

namespace IssueTracking.Application.Contracts.Issues
{
  public class CreateIssueDto
  {
    public Guid RepositoryId { get; set; }
    
    [Required] 
    public string Title { get; set; }
    public string Text { get; set; }
    public Guid? AssignedUserId { get; set; }
  }
}