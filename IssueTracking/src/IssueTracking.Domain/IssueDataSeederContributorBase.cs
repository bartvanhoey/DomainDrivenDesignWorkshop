using System;
using System.Collections.Generic;
using IssueTracking.Domain.Issues;
using IssueTracking.Domain.Shared.Issues;

namespace IssueTracking.Domain
{
  public class IssueDataSeederContributorBase
  {
    public readonly List<SeederHelperIssue> DataSeederIssues = new List<SeederHelperIssue>();

    public IssueDataSeederContributorBase()
    {
      AddIssues();
    }

    private void AddIssues()
    {
      DataSeederIssues.AddRange(new[]
       {
                new SeederHelperIssue {Title = "Issue1", Text = "CMS Kit - Tag System Configuration Changes abp-module-cms-kit effort-3 in-progress", IsClosed = true, CloseReason = IssueCloseReason.DueDatePassed},
                new SeederHelperIssue {Title = "Issue2", Text = "Intellisense for Bootstrap Components is not working in the Module Project", IsClosed = false},
                new SeederHelperIssue {Title = "Issue3", Text = "How to add a identityUser foreign key to an entity", IsClosed = false},
                new SeederHelperIssue {Title = "Issue4", Text = "sometimes ,throw exception:The connection is closed", IsClosed = true, CloseReason = IssueCloseReason.NoBandWith},
                new SeederHelperIssue {Title = "Issue5", Text = "EmailSend Exception", IsClosed = false},
                new SeederHelperIssue {Title = "Issue6", Text = "Documentation of CLI public web site option abp-cli documentation effort-0.5", IsClosed = true, CloseReason = IssueCloseReason.DueDatePassed},
                new SeederHelperIssue {Title = "Issue7", Text = "Sometimes startup is too slow", IsClosed = false},
                new SeederHelperIssue {Title = "Issue8", Text = "Volo.Abp.Cli add-module --new can not create angular module properly abp-cli effort-2", IsClosed = false},
                new SeederHelperIssue {Title = "Issue9", Text = "Local Events not firing after upgrading to version 4.0", IsClosed = false},
                new SeederHelperIssue {Title = "Issue10", Text = "RepositoryAsyncExtensions should use Repository.GetQueryableAsync() abp-framework", IsClosed = true, CloseReason = IssueCloseReason.NoBandWith},
                new SeederHelperIssue {Title = "Issue11", Text = "Create an Excel import and export sample.", IsClosed = false},
                new SeederHelperIssue {Title = "Issue12", Text = "Article Request: DevExpress Reporting and Dashboard Integrations into Abp.io feature", IsClosed = true, CloseReason = IssueCloseReason.OutOfScope},
                new SeederHelperIssue {Title = "Issue13", Text = "IdentityServer tests have duplicate data builders abp-module-identityserver effort-1", IsClosed = false},
                new SeederHelperIssue {Title = "Issue14", Text = "Clean build warnings effort-5", IsClosed = true, CloseReason = IssueCloseReason.Irrelevant},
                new SeederHelperIssue {Title = "Issue15", Text = "[Angular] Domain Tenant Resolver is not working properly bug effort-3 in-progress", IsClosed = false, },
                new SeederHelperIssue {Title = "Issue16", Text = "How to create a project using the preview 4.2 with cms-kit included ?", IsClosed = true, CloseReason = IssueCloseReason.Solved}

            });
    }
  }

  public class SeederHelperIssue
  {
    public string Title { get; set; }
    public string Text { get; set; }
    public bool IsClosed { get; set; }
    public IssueCloseReason? CloseReason { get; set; }
  }
}