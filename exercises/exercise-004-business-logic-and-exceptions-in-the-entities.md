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
                        "Can not lock a closed issue! Open it first."
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

### Run application and Test the AddComment method

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Register as a new user and make sure you are logged in. Goto the **Issues** list and double-click on an issue to have its comments displayed and click on the **AddComment** button in the **Actions** dropdown.

* Enter a comment and check if it gets added to the issue.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt).

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_004)
