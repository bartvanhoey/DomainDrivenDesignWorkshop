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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Volo.Abp.Domain.Repositories;

    namespace IssueTracking.Domain.Issues
    {
        public interface IIssueRepository :  IRepository<Issue, Guid>
        {
            Task<List<Issue>> GetInActiveIssuesAsync();
        }
    }
    ```

    `IIssueRepository` extends the standard `IRepository<...>` interface by adding a `GetInActiveIssuesAsync` method. This repository works with such an `Issue` class:

2. Implement interface **IIssueRepository** in class **EfCoreIssueRepository** in the **EntityFrameworkCore** project.

    The rule says the repository shouldn't know the business rules. The question here is "**What is an inactive issue**? Is it a business rule definition?"

    Let's see the implementation to understand it:

    ```csharp
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IssueTracking.Domain.Issues;
    using Microsoft.EntityFrameworkCore;
    using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
    using Volo.Abp.EntityFrameworkCore;

    namespace IssueTracking.EntityFrameworkCore.Issues
    {
      public class EfCoreIssueRepository : EfCoreRepository<IssueTrackingDbContext, Issue, Guid>, IIssueRepository
      {
        public EfCoreIssueRepository(IDbContextProvider<IssueTrackingDbContext> dbContextProvider) 
        : base(dbContextProvider)
        {
        }

        public async Task<List<Issue>> GetInActiveIssuesAsync()
        {

          var daysAgo30 = DateTime.Now.Subtract(TimeSpan.FromDays(30));

          return await DbSet.Where(i =>

              //Open
              !i.IsClosed &&

              //Assigned to nobody
              i.AssignedUserId == null &&

              //Created 30+ days ago
              i.CreationTime < daysAgo30 &&

              //No comment or the last comment was 30+ days ago
              (i.LastCommentTime == null || i.LastCommentTime < daysAgo30)

          ).ToListAsync();
        }

        //...

    }
    ```

    When we check the `GetInActiveIssuesAsync` implementation, we see a **business rule that defines an in-active issue**: The issue should be **open**, **assigned to nobody**, **created 30+ days ago** and has **no comment in the last 30 days**.

    This is an implicit definition of a business rule that is hidden inside a repository method. The problem occurs when we need to reuse this business logic.

    For example, let's say that we want to add an `bool IsInActive()` method on the `Issue` entity. In this way, we can check activeness when we have an issue entity.

3. Add an **IsInActive** method in the **Issue** class in the  **issues** folder of the **Domain** project.

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
            var daysAgo30 = DateTime.Now.Subtract(TimeSpan.FromDays(30));
            return
                //Open
                !IsClosed &&

                //Assigned to nobody
                AssignedUserId == null &&

                //Created 30+ days ago
                CreationTime < daysAgo30 &&

                //No comment or the last comment was 30+ days ago
                (LastCommentTime == null || LastCommentTime < daysAgo30);
        }
    }
   ```

### Do you see the problem?

We had to copy/paste/modify the code. What if the definition of the activeness changes? We should not forget to update both places. This is a duplication of a business logic, which is pretty dangerous.

A good solution to this problem is the *Specification Pattern*! We will see it in action in exercise 007.

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_006)
