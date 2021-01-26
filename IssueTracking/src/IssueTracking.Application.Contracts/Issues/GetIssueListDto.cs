using Volo.Abp.Application.Dtos;
using System;

namespace IssueTracking.Application.Contracts.Issues
{
  public class GetIssueListDto  : PagedAndSortedResultRequestDto
  {
    public string Filter { get; set; }
    public bool? ShowInActiveIssues { get; set; }
    public Guid MileStoneId{ get; set; }
  }
}