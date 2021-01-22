using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using IssueTracking.Application.Contracts.Issues;
using IssueTracking.Domain.Shared.Issues;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Blazor.Pages
{
  public partial class Issues
  {
    protected IReadOnlyList<IssueDto> IssueList { get; set; }
    protected int PageSize { get; } = 20;
    protected int CurrentPage { get; set; }
    protected string CurrentSorting { get; set; }
    protected int TotalCount { get; set; }
    protected bool CanCreateIssue = true;
    protected bool CanUpdateIssue = true;
    protected bool CanDeleteIssue = true;
    protected IssueDto selectedIssueDto;
    protected bool ShowComments = true;
    protected bool ShowInActiveIssues = false;

    protected CreateIssueDto NewEntity { get; set; } = new CreateIssueDto();
    protected UpdateIssueDto EditingEntity { get; set; } = new UpdateIssueDto();
    protected CreateCommentDto CreateCommentEntity { get; set; } = new CreateCommentDto();
    protected CloseIssueDto CloseIssueEntity { get; set; } = new CloseIssueDto();

    protected Guid EditingIssueId { get; set; }
    protected Guid CreateCommentIssueId { get; set; }
    protected Guid CloseIssueId { get; set; }

    protected Modal CreateModal { get; set; }
    protected Modal EditModal { get; set; }
    protected Modal AddCommentModal { get; set; }
    protected Modal CloseIssueModal { get; set; }

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
      CreateCommentEntity.IssueId = CreateCommentIssueId;
      await IssueAppService.CreateCommentAsync(CreateCommentEntity);
      await GetIssuesAsync();
      AddCommentModal.Hide();
    }

    protected void OpenAddCommentModal(IssueDto issue)
    {
      CreateCommentIssueId = issue.Id;
      CreateCommentEntity = ObjectMapper.Map<IssueDto, CreateCommentDto>(issue);
      AddCommentModal.Show();
    }

    protected void OpenCloseIssueModal(IssueDto issue)
    {
      CloseIssueId = issue.Id;
      CreateCommentEntity = ObjectMapper.Map<IssueDto, CreateCommentDto>(issue);
      CloseIssueModal.Show();
    }


    protected async Task GetIssuesAsync(bool? showNotActiveIssues = null)
    {
      var result = await IssueAppService.GetListAsync(
          new GetIssueListDto
          {
            MaxResultCount = PageSize,
            SkipCount = CurrentPage * PageSize,
            Sorting = CurrentSorting,
            ShowNotActiveIssues = showNotActiveIssues
          });

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

    protected void CloseCloseIssueModalAsync()
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

    protected async Task LockIssueAsync(IssueDto issue)
    {
      var confirmMessage = L["IssueLockConfirmationMessage", issue.Title];
      if (!await Message.Confirm(confirmMessage)) return;
      await IssueAppService.LockAsync(issue.Id);
      await GetIssuesAsync();
    }

    protected async Task UnlockIssueAsync(IssueDto issue)
    {
      var confirmMessage = L["IssueUnlockConfirmationMessage", issue.Title];
      if (!await Message.Confirm(confirmMessage)) return;
      await IssueAppService.UnlockAsync(issue.Id);
      await GetIssuesAsync();
    }

    protected async Task CloseIssueAsync()
    {
      await IssueAppService.CloseAsync(CloseIssueId, CloseIssueEntity);
      await GetIssuesAsync();
      CloseIssueModal.Hide();
    }

    protected async Task ReOpenIssueAsync(IssueDto issue)
    {
      var confirmMessage = L["IssueReopenConfirmationMessage", issue.Title];
      if (!await Message.Confirm(confirmMessage)) return;
      await IssueAppService.ReOpenAsync(issue.Id);
      await GetIssuesAsync();
    }

    protected async Task OnIsActiveChangedAsync()
    {
        ShowInActiveIssues  =! ShowInActiveIssues;
        await GetIssuesAsync(ShowInActiveIssues);
    }

  }
}