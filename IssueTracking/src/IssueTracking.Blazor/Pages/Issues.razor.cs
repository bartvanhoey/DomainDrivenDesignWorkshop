using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using IssueTracking.Application.Contracts.Issues;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Blazor.Pages
{
  public partial class Issues
  {
    protected IReadOnlyList<IssueDto> IssueList { get; set; }
    protected int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
    protected int CurrentPage { get; set; }
    protected string CurrentSorting { get; set; }
    protected int TotalCount { get; set; }
    protected bool CanCreateIssue = true;
    protected bool CanUpdateIssue = true;
    protected bool CanDeleteIssue = true;
    protected IssueDto selectedIssueDto;
    protected bool ShowComments = true;

    protected CreateIssueDto NewEntity { get; set; } = new CreateIssueDto();
    protected UpdateIssueDto EditingEntity { get; set; } = new UpdateIssueDto();
    protected CreateCommentDto CreateCommentEntity { get; set; } = new CreateCommentDto();

    protected Guid EditingIssueId { get; set; }
    protected Guid CreateCommentIssueId { get; set; }

    protected Modal CreateModal { get; set; }
    protected Modal EditModal { get; set; }
    protected Modal AddCommentModal { get; set; }

    protected override async Task OnInitializedAsync()
    {
      await SetPermissionsAsync();
      await GetIssuesAsync();

    }

    protected Task SetPermissionsAsync()
    {
      // CanCreateIssue = await AuthorizationService.IsGrantedAsync(IssueTrackingPermissions.Issue.Create);
      // CanUpdateIssue = await AuthorizationService.IsGrantedAsync(IssueTrackingPermissions.Issue.Update);
      // CanDeleteIssue = await AuthorizationService.IsGrantedAsync(IssueTrackingPermissions.Issue.Delete);

      return Task.CompletedTask;

    }

    protected void OpenCreateModal()
    {
      NewEntity = new CreateIssueDto();
      CreateModal.Show();
    }

    protected void CloseCreateModalAsync()
    {
      CreateModal.Hide();
    }

    protected void OpenEditModal(IssueDto issue)
    {
      EditingIssueId = issue.Id;
      EditingEntity = ObjectMapper.Map<IssueDto, UpdateIssueDto>(issue);
      EditModal.Show();
    }

    protected async Task DeleteIssueAsync(IssueDto issue)
    {
      var confirmMessage = L["IssueDeletionConfirmationMessage", issue.Title];
      if (!await Message.Confirm(confirmMessage))
      {
        return;
      }

      await IssueAppService.DeleteAsync(issue.Id);
      await GetIssuesAsync();
    }

    protected async Task AddCommentAsync()
    {
      await Task.CompletedTask;
    }

    protected void OpenAddCommentModalAsync(IssueDto issue)
    {
      CreateCommentIssueId = issue.Id;
      CreateCommentEntity = ObjectMapper.Map<IssueDto, CreateCommentDto>(issue);
      AddCommentModal.Show();
    }

    protected async Task GetIssuesAsync()
    {
      var result = await IssueAppService.GetListAsync(
          new GetIssueListDto
          {
            MaxResultCount = PageSize,
            SkipCount = CurrentPage * PageSize,
            Sorting = CurrentSorting
          }
          );

      IssueList = result.Items;
      TotalCount = (int)result.TotalCount;
    }

    protected async Task OnDataGridReadAsync(DataGridReadDataEventArgs<IssueDto> e)
    {
      CurrentSorting = e.Columns
          .Where(c => c.Direction != SortDirection.None)
          .Select(c => c.Field + (c.Direction == SortDirection.Descending ? "DESC" : ""))
          .JoinAsString(",");
      CurrentPage = e.Page - 1;

      await GetIssuesAsync();

      StateHasChanged();
    }

    protected void CloseEditModalAsync()
    {
      EditModal.Hide();
    }

    protected void CloseAddCommentModalAsync()
    {
      AddCommentModal.Hide();
    }

    protected async Task CreateEntityAsync()
    {
      await IssueAppService.CreateAsync(NewEntity);
      await GetIssuesAsync();
      CreateModal.Hide();
    }

    protected async Task UpdateEntityAsync()
    {
      await IssueAppService.UpdateAsync(EditingIssueId, EditingEntity);
      await GetIssuesAsync();
      EditModal.Hide();
    }

    protected void ToggleSelectedIssueDto(IssueDto issueDto)
    {
      if (selectedIssueDto == issueDto && ShowComments == true)
        ShowComments = false;
      else
        ShowComments = true;
    }


  }
}