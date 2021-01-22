# Exercise 001: Add a comment to an issue

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_001)

Entities are responsible to implement the business rules related to the properties of their own. The *Aggregate Root Entities* are also responsible for their sub-collection entities.

An aggregate should maintain its self **integrity** and **validity** by implementing domain rules and constraints. That means, unlike the DTOs, Entities have **methods to implement some business logic**. Actually, we should try to implement business rules in the entities wherever possible.

To achieve this, add a **AddComment** method to the **Issue** class. This method will be responsible for adding a comment to an issue.

## Put it into practice

### Checkout branch exercise_001

```bash
git checkout exercise_001
```

### Let's make it work by adding some code

1. Add a **AddComment** method to class **Issue** in the **Issues** folder of the **Domain** project.

    ```csharp
    public void AddComment(Guid userId, string text)
    {
        Comments ??= new Collection<Comment>();
        Comments.Add(new Comment { IssueId = this.Id, Text = text, UserId = userId });
    } 
    ```

2. Add a **CreateCommentAsync** method to interface **IIssueAppService** in the **Issues** folder of the  **Application.Contracts** project

    ```csharp
    public interface IIssueAppService : IApplicationService
    {
      ///.....
      Task CreateCommentAsync(CreateCommentDto input);
    }
    ```

3. Implement interface **IIssueAppService** by adding the **CreateCommentAsync** to the **IssueAppService** in the **Issues** folder of the  **Application** project

    ```csharp
    // import usings
    // using Microsoft.AspNetCore.Authorization;
    // using Volo.Abp.Users;

    public class IssueAppService : ApplicationService, IIssueAppService
    {
        private readonly IssueRepository _issueRepository;

        public IssueAppService(IssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        ///.....
    
        [Authorize]
        public async Task CreateCommentAsync(CreateCommentDto input)
        {
            var issue = await _issueRepository.GetAsync(input.IssueId);
            issue.AddComment(CurrentUser.GetId(), input.Text);
            await _issueRepository.UpdateAsync(issue);
        }
    }
    ```

4. In **Issues.razor.cs** update method **AddCommentAsync** in the **Pages** folder of the **Blazor** project

    ```csharp
    protected async Task AddCommentAsync()
    {
      CreateCommentEntity.IssueId = CreateCommentIssueId;
      await IssueAppService.CreateCommentAsync(CreateCommentEntity);
      await GetIssuesAsync();
      AddCommentModal.Hide();
    }
    ```

### Run application and Test the AddComment method

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Start the **IssueTracking.HttpApi.Host** by pressing `F5`.

* Open a command prompt in the **Blazor** project and enter `dotnet watch run`.

* Log in and navigate to the **Issues** list and double-click on an issue to have its comments displayed and click on the **AddComment** button in the **Actions** dropdown.

* Enter a comment and check if it gets added to the issue.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt).

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_001)
