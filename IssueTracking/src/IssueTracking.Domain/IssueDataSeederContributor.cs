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

        public IssueDataSeederContributor(IRepository<Issue, Guid> issueRepository)
        {
            _issueRepository = issueRepository;
        }
        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _issueRepository.GetCountAsync() <= 0)
            {
                foreach (var issue in DataSeederIssues)
                {
                     await _issueRepository.InsertAsync(
                     new Issue
                     {
                         Title = issue.Title,
                         Text = issue.Text,
                         RepositoryId = Guid.NewGuid(),
                         IsClosed = issue.IsClosed,
                         CloseReason = issue.CloseReason
                     },
                     autoSave: true
                 ); 
                }
            }
        }
    }
}