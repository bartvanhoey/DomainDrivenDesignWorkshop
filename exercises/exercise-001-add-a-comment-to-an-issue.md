# Exercise 001: Add a comment to an issue

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#Aggregate-/-Aggregate-Root-Principles)

Entities are responsible to implement the business rules related to the properties of their own. The *Aggregate Root Entities* are also responsible for their sub-collection entities.

An aggregate should maintain its self **integrity** and **validity** by implementing domain rules and constraints. That means, unlike the DTOs, Entities have **methods to implement some business logic**. Actually, we should try to implement business rules in the entities wherever possible.

To achieve this, add a AddCommentAsync method to the Issue class.

1. Add a **CreateCommentAsync** method to interface **IIssueAppService** in the **Issues** folder of the  **Application.Contracts** project

    ```csharp
    public interface IIssueAppService : IApplicationService
    {
      /// Other methods here ...
      Task CreateCommentAsync(CreateCommentDto input);
    }
    ```

2. Implement interface **IIssueAppService** by adding the **CreateCommentAsync** to the **IssueAppService** in the **Issues** folder of the  **Application** project

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

        /// Other methods here ...
    
        [Authorize]
        public async Task CreateCommentAsync(CreateCommentDto input)
        {
            var issue = await _issueRepository.GetAsync(input.IssueId);
            issue.AddComment(CurrentUser.GetId(), input.Text);
            await _issueRepository.UpdateAsync(issue);
        }
    }
    ```

3. Add a **AddComment** method to class **Issue** in the **Issues** folder of the **Domain** project

    ```csharp
    public void AddComment(Guid userId, string text)
    {
        Comments.Add(new Comment { IssueId = this.Id, Text = text, UserId = userId });
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
