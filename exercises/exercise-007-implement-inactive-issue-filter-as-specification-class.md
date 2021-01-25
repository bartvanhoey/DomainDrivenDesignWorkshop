# Exercise 007: Implement Inactive Issue Filter as a Specification class

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_007)

In exercise 006 did you see how easy it is to leak business rules into repositories and classes. In this exercise we will see how to solve this problem by using a **Specification**

## Put it into practice

### Checkout branch exercise_007

```bash
git checkout exercise_007
```

1. Add a **InActiveIssueSpecification.cs** file to **Issues** folder of the **Domain Project**.

    ```csharp
    using System;
    using System.Linq.Expressions;
    using Volo.Abp.Specifications;

    namespace IssueTracking.Issues
    {
        public class InActiveIssueSpecification : Specification<Issue>
        {
            public override Expression<Func<Issue, bool>> ToExpression()
            {
                var daysAgo30 = DateTime.Now.Subtract(TimeSpan.FromDays(30));
                return i =>

                    //Open
                    !i.IsClosed &&

                    //Assigned to nobody
                    i.AssignedUserId == null &&

                    //Created 30+ days ago
                    i.CreationTime < daysAgo30 &&

                    //No comment or the last comment was 30+ days ago
                    (i.LastCommentTime == null || i.LastCommentTime < daysAgo30);
            }
        }
    }

    ```

2. Update the **IsInActive** method of the **Issue** class and use the **InActiveIssueSpecification** specification.

    ```csharp
    public class Issue : AggregateRoot<Guid>, IHasCreationTime
    {
        public bool IsClosed { get; private set; }
        public Guid? AssignedUserId { get; private set; }
        public DateTime CreationTime { get; private set; }
        public DateTime? LastCommentTime { get; private set; }
        //...

        public bool IsInActive()
        {
            return new InActiveIssueSpecification().IsSatisfiedBy(this);
        }
    }
    ```

3. Rename method **GetInActiveIssuesAsync** to **GetIssuesAsync** and pass in a **ISpecification** parameter.

    ```csharp
    public interface IIssueRepository : IRepository<Issue, Guid>
    {
        Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec);
    }
    ```

4. Implement the **GetIssueAsync** method in the **IssueRepository** class

    ```csharp
    public async Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec)
    {
      return await DbSet.Where(spec.ToExpression()).ToListAsync();
    }
    ```

5. Uncomment the line below in **IssueAppService** in the **Application** project

    ```csharp
    // issues = await _issueRepository.GetIssuesAsync(new InActiveIssueSpecification());
    
    ```

### Run application and Test the implementation of the InActiveIssueSpecification

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Login with username `admin` and password `1q2w3E*`.

* Goto the **Issues** list and check the **No** checkbox to see the inactive issues only.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt)

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_007)
