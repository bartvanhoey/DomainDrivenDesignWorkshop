using System;
using System.Threading.Tasks;
using IssueTracking.Domain.Issues;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Domain
{
    public class IssueDataSeederContributor : IDataSeedContributor, ITransientDependency
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
                await _issueRepository.InsertAsync(
                    new Issue
                    {
                        Title = "Issue1 Title",
                        Text = "Issue1 Text",
                        RepositoryId = Guid.NewGuid()
                    },
                    autoSave: true
                );

                await _issueRepository.InsertAsync(
                     new Issue
                     {
                         Title = "Issue2 Title",
                         Text = "Issue2 Text",
                         RepositoryId = Guid.NewGuid()
                     },
                     autoSave: true
                 );
            }
        }
    }
}