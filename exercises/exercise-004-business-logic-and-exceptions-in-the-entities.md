# Exercise 004: Business Logic & Exceptions in the Entities

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_004)

## Domain Specific Exceptions

When you implement validation and business logic in the entities, you frequently need to manage the exceptional cases. In these cases;

* Create **domain specific exceptions**.
* **Throw these exceptions** in the entity methods when necessary.

## Put it into practice

### Checkout branch exercise_004

```bash
git checkout exercise_004
```

### Let's make it work by adding some code

1. Update class **Issue** in the **Issues** folder of the **Domain** project.

    ```csharp
        public class Issue : AggregateRoot<Guid>
        {
            //...
            
            public bool IsLocked { get; private set; }
            public bool IsClosed { get; private set; }
            public IssueCloseReason? CloseReason { get; private set; }

            public void Close(IssueCloseReason reason)
            {
                IsClosed = true;
                CloseReason = reason;
            }

            public void ReOpen()
            {
                if (IsLocked)
                {
                    // business rule 1: A locked issue can not be re-opened.
                    throw new IssueStateException(
                        "Can not open a locked issue! Unlock it first."
                    );
                }

                IsClosed = false;
                CloseReason = null;
            }

            public void Lock()
            {
                if (!IsClosed)
                {
                    // business rule 2: You can not lock an open issue.
                    throw new IssueStateException(
                        "Can not lock an open issue! Close it first."
                    );
                }

                IsLocked = true;
            }

            public void Unlock()
            {
                IsLocked = false;
            }
        }
    ```

2. Add a  **IssueStateException** class in the **Issues** folder of the  **Domain** project with the code below.

    ```csharp
    using System;

    namespace IssueTracking.Domain.Issues
    {
        public class IssueStateException : Exception
        {
            public IssueStateException(string message)
                : base(message)
            {
                
            }
        }
    }
    ```

3. Add extra methods in the **IIssueAppService** interface in folder **Issues** of the **Application.Contracts** project.

    ```csharp
        // import usings
        //  using IssueTracking.Domain.Shared.Issues;

        //...
        Task CloseAsync(Guid id, CloseIssueDto input);
        
        Task ReOpenAsync(Guid id);

        Task LockAsync(Guid id);

        Task UnlockAsync(Guid id);
    ```

4. Implement these extra methods in **IssueAppService** class in folder **Issues** of the **Application** project.

    ```csharp
        // import usings
        //  using IssueTracking.Domain.Shared.Issues;

        //...
        public async Task CloseAsync(Guid id, CloseIssueDto input)
        {
        var issue = await _issueRepository.GetAsync(id);
        issue.Close(input.CloseReason);
        }
        
        public async Task ReOpenAsync(Guid id)
        {
        var issue = await _issueRepository.GetAsync(id);
        issue.ReOpen();      
        }

        public async Task LockAsync(Guid id)
        {
        var issue = await _issueRepository.GetAsync(id);
        issue.Lock();
        
        }

        public async Task UnlockAsync(Guid id)
        {
        var issue = await _issueRepository.GetAsync(id);
        issue.Unlock();
        }
    ```

5. Open file **Issues.razor.cs** in folder **Issues** of the **Blazor** project and update the methods below.

   ```csharp
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
   ```

### Run application and Test the Close/Reopen and Lock/Unlock methods to see if Business Rules are applied

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Login with username `admin` and password `1q2w3E*`. 

* Goto the **Issues** list and click on the **Actions** dropdown to test the different methods.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt).

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_004)
