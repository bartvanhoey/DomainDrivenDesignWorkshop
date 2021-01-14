# Exercise 006: Do Not Include Domain Logic in Repositories

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_006)

While this rule seems obvious at the beginning, it is easy to leak business logic into repositories.

In this exercise you will see how easy it is to leak business rules into repositories. We will fix this problem in exercise 007.

## Put it into practice - The Wrong Way

### Checkout branch exercise_006

```bash
git checkout exercise_006
```

### Let's do it wrong first. We will do it right in the exercise 007

1. Add a **GetInActiveIssueAsync** method to the **IIssueRepository** interface in the **Issues** folder of the **Domain** project.

    ```csharp
    public interface IIssueRepository : IRepository<Issue, Guid>
    {
      Task<List<Issue>> GetInActiveIssuesAsync();
    }
    ```

2. Change the **ReOpen** method in the **Issues** folder of the **Domain** project.

    ```csharp
    public void ReOpen()
    {
      if (IsLocked)
      {
        // business rule 1: A locked issue can not be re-opened.
        throw new IssueStateException("IssueTrackingDomainErrorCodes.YouCannotReOpenALockedIssue");
      }
      IsClosed = false;
      CloseReason = null;
    }
    ```

3. Implement IIssueRepository in that specifies a **unique error code** in **IssueTrackingDomainErrorCodes** class in the **Domain.Shared** project.

    ```csharp
    public static class IssueTrackingDomainErrorCodes
    {
      public const string YouCannotReOpenALockedIssue = "IssueTracking:00001";
    }
    ````

4. Add an entry in file **en.json** file in the **Localization/IssueTracking** folder of the **Domain.Shared** project.

   ```json
    "IssueTracking:00001": "Can not open a locked issue! Unlock it first."
   ```

### Run application and Test the Reopen method for an Issue that has been locked

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Register as a new user and make sure you are logged in. Goto the **Issues** list. Reopening a locked issue should throw a **localized business exception**.

![Localized error message](images/error_message_localized_business_exception.png "Localized error message thrown by the system")

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt)

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_006)
