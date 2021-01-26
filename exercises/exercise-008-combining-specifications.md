# Exercise 008: Implement Inactive Issue Filter as a Specification class

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_008)

In exercise 006 did you see how easy it is to leak business rules into repositories and classes. In this exercise we will see how to solve this problem by using a **Specification**

## Put it into practice

### Checkout branch exercise_008

```bash
git checkout exercise_008
```

1. Add a **MileStoneSpecification.cs** file to **Issues** folder of the **Domain Project**. This specification takes a parameter in its constructor.

    ```csharp
    using System;
    using System.Linq.Expressions;
    using Volo.Abp.Specifications;

    namespace IssueTracking.Domain.Issues
    {
        public class MileStoneSpecification : Specification<Issue>
        {
            public Guid MileStoneId { get; }

            public MileStoneSpecification(Guid mileStoneId)
            {
            MileStoneId = mileStoneId;
            }

            public override Expression<Func<Issue, bool>> ToExpression()
            {
            return i => i.MileStoneId == MileStoneId;
            }
        }
    }
    ```



2. Uncomment the line below in **IssueAppService** in the **Application** project

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

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_008)
