# Exercise 009: Business logic on an entity method and **get external dependencies as parameters** of the method

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_009)

In this exercise we will see how to use an external service in an entity by passing that service as a parameter in a method. In the next exercise you will see a better approach of how to do it in the ABP Framework.

## Put it into practice

### Checkout branch exercise_009

```bash
git checkout exercise_009
```

1. 



### Run application and Test the implementation of the InActiveIssueSpecification

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Goto the **Issues** list and try out the newly implemented functionality to show only issues with a specific MileStoneId.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt)

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_009)
