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

2. Use **InActiveIssueSpecification** in the **IsInActive** method of the **Issue** class.

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

3. Add a method to the **IIssueRepository** interface in the **Application.Contracts** project.

    ```csharp
    public interface IIssueRepository : IRepository<Issue, Guid>
    {
        Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec);
    }
    ```

4. Implement the GetIssueAsync method in the EfCoreIssueRepository class

    ```csharp
    public async Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec)
    {
      return await DbSet.Where(spec.ToExpression()).ToListAsync();
    }
    ```

