using System;
using System.Threading.Tasks;
using GenFu;
using IssueTracking.Domain.Issues;
using IssueTracking.Users;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace IssueTracking.Domain
{
  public class IssueDataSeederContributor : IssueDataSeederContributorBase, IDataSeedContributor, ITransientDependency
  {
    private readonly IRepository<Issue, Guid> _issueRepository;
    private readonly IRepository<Comment, Guid> _commentRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public IssueDataSeederContributor(IRepository<Issue, Guid> issueRepository, IRepository<Comment, Guid> commentRepository, IdentityUserManager identityUserManager, IGuidGenerator guidGenerator, IRepository<IdentityUser, Guid> userRepository)
    {
      _issueRepository = issueRepository;
      _commentRepository = commentRepository;
      _identityUserManager = identityUserManager;
      _guidGenerator = guidGenerator;
      _userRepository = userRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
      var mileStoneId1 = Guid.NewGuid();
      var mileStoneId2 = Guid.NewGuid();


      if (await _userRepository.GetCountAsync() <= 1)
      {
        var users = A.ListOf<User>();
        foreach (var user in users)
        {
          var userName = (user.FirstName + user.LastName).Trim().Replace(" ", "");
          var identityUser = new IdentityUser(_guidGenerator.Create(), userName, user.EmailAddress);
          var result = await _identityUserManager.CreateAsync(identityUser, "Server2008!");
        }
      }

      if (await _issueRepository.GetCountAsync() <= 0)
      {
        var random = new Random();
        int counter = 0;
        foreach (var dataSeederIssue in DataSeederIssues)
        {
          var creationDate = DateTime.Now.AddDays(-random.Next(60));
          var issueToInsert = new Issue(Guid.NewGuid(), Guid.NewGuid(), dataSeederIssue.Title, dataSeederIssue.Text, creationTime: creationDate);
          if (dataSeederIssue.IsClosed == true && dataSeederIssue.CloseReason.HasValue)
            issueToInsert.Close(dataSeederIssue.CloseReason.Value);

          if (counter % 4 == 0)
          {
            // issueToInsert.SetAssignedUserId(Guid.NewGuid());
            if (issueToInsert.IsClosed) issueToInsert.Lock();
          }
          else if (counter % 2 == 0)
          {
            issueToInsert.SetMileStoneId(mileStoneId2);
          }
          else
          {
            issueToInsert.SetMileStoneId(mileStoneId1);
          }

          var insertedIssue = await _issueRepository.InsertAsync(issueToInsert, autoSave: true);
          var numberOfComments = random.Next(4);
          for (int i = 1; i <= numberOfComments; i++)
          {
            await _commentRepository.InsertAsync(
                new Comment { IssueId = insertedIssue.Id, Text = $"Sample comment {i}" },
                autoSave: true);
          }
          counter++;
        }
      }
    }
  }

  public class User
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EmailAddress { get; set; }

    public string PhoneNumber { get; set; }
  }
}