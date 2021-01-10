using System;
using System.Threading.Tasks;
using IssueTracking.Domain.Issues;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Domain
{
  public class IssueDataSeederContributor : IssueDataSeederContributorBase, IDataSeedContributor, ITransientDependency
  {
    private readonly IRepository<Issue, Guid> _issueRepository;
    private readonly IRepository<Comment, Guid> _commentRepository;

    public IssueDataSeederContributor(IRepository<Issue, Guid> issueRepository, IRepository<Comment, Guid> commentRepository)
    {
      _issueRepository = issueRepository;
      _commentRepository = commentRepository;
    }
    public async Task SeedAsync(DataSeedContext context)
    {
      if (await _issueRepository.GetCountAsync() <= 0)
      {
        var random = new Random();
        foreach (var dataSeederIssue in DataSeederIssues)
        {
          var issue = await _issueRepository.InsertAsync(
             new Issue
             {
               Title = dataSeederIssue.Title,
               Text = dataSeederIssue.Text,
               RepositoryId = Guid.NewGuid(),
               IsClosed = dataSeederIssue.IsClosed,
               CloseReason = dataSeederIssue.CloseReason
             },
             autoSave: true
         );

          var numberOfComments = random.Next(4);
          for (int i = 1; i <= numberOfComments; i++)
          {
            await _commentRepository.InsertAsync(
                new Comment { IssueId = issue.Id, Text = $"Sample comment {i}" },
                autoSave: true);
          }
        }
      }
    }
  }
}