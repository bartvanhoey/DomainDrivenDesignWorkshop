using Volo.Abp.Application.Dtos;

namespace IssueTracking.Application.Contracts.Issues
{
  public class GetIssueListDto  : PagedAndSortedResultRequestDto
  {
    public string Filter { get; set; }
    public bool? IsActive { get; set; }
  }
}