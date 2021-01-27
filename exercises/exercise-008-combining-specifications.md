# Exercise 008: Implement Inactive Issue Filter as a Specification class

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_008)

In exercise 007 we used the InActiveIssueSpecification to filter InActive issues. In this exercise we will see how you can combine specifications.
Let's say, we want only see inactive issues and belong to a specific milestone.

## Put it into practice

### Checkout branch exercise_008

```bash
git checkout exercise_008
```

1. Add a **MileStoneSpecification.cs** file to **Issues** folder of the **Domain Project**. As you can see, this specification takes a parameter in its constructor.

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

2. Update the **GetListAsync** method in **IssueAppService** class of the **Application** project.

    ```csharp
        var issues = new List<Issue>();
        if (input.ShowInActiveIssues.HasValue && input.ShowInActiveIssues == true &&  input.MileStoneId != Guid.Empty)
        {
            issues = await AsyncExecuter.ToListAsync(_issueRepository.Where(
                new InActiveIssueSpecification()
                .And(new MileStoneSpecification(input.MileStoneId)).ToExpression()));
        }
        else if (input.ShowInActiveIssues.HasValue && input.ShowInActiveIssues == false &&  input.MileStoneId != Guid.Empty)
        {
            issues = await AsyncExecuter.ToListAsync(_issueRepository.Where(
                new MileStoneSpecification(input.MileStoneId)));
        }
        else if (input.ShowInActiveIssues.HasValue && input.ShowInActiveIssues == true)
        {
            issues = await AsyncExecuter.ToListAsync(_issueRepository.Where(
                new InActiveIssueSpecification()));
        }
        else
        {
            issues = await _issueRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, includeDetails: true);
        }
    ```

### Run application and Test the implementation of the InActiveIssueSpecification

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Goto the **Issues** list and try out the newly implemented functionality to show only issues with a specific MileStoneId.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt)

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_008)
