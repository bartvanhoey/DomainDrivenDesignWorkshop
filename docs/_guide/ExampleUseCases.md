## Example Use Cases

This section will demonstrate some example use cases and discuss alternative scenarios.

### Entity Creation

Creating an object from an Entity / Aggregate Root class is the first step of the lifecycle of that entity. The *Aggregate / Aggregate Root Rules & Best Practices* section suggests to **create a primary constructor** for the Entity class that guarantees to **create a valid entity**. So, whenever we need to create an instance of that entity, we should always **use that constructor**.

See the `Issue` Aggregate Root class below:

````csharp
public class Issue : AggregateRoot<Guid>
{
    public Guid RepositoryId { get; private set; }
    public string Title { get; private set; }
    public string Text { get; set; }
    public Guid? AssignedUserId { get; internal set; }

    public Issue(
        Guid id,
        Guid repositoryId,
        string title,
        string text = null
        ) : base(id)
    {
        RepositoryId = repositoryId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        Text = text; //Allow empty/null
    }
    
    private Issue() { /* Empty constructor is for ORMs */ }

    public void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
    }

    //...
}
````

* This class guarantees to create a valid entity by its constructor.
* If you need to change the `Title` later, you need to use the `SetTitle` method which continues to keep `Title` in a valid state.
* If you want to assign this issue to a user, you need to use `IssueManager` (it implements some business rules before the assignment - see the *Domain Services* section above to remember).
* The `Text` property has a public setter, because it also accepts null values and does not have any validation rules for this example. It is also optional in the constructor.

Let's see an Application Service method that is used to create an issue:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IssueManager _issueManager;
    private readonly IRepository<Issue, Guid> _issueRepository;
    private readonly IRepository<AppUser, Guid> _userRepository;

    public IssueAppService(
        IssueManager issueManager,
        IRepository<Issue, Guid> issueRepository,
        IRepository<AppUser, Guid> userRepository)
    {
        _issueManager = issueManager;
        _issueRepository = issueRepository;
        _userRepository = userRepository;
    }

    public async Task<IssueDto> CreateAsync(IssueCreationDto input)
    {
        // Create a valid entity
        var issue = new Issue(
            GuidGenerator.Create(),
            input.RepositoryId,
            input.Title,
            input.Text
        );

        // Apply additional domain actions
        if (input.AssignedUserId.HasValue)
        {
            var user = await _userRepository.GetAsync(input.AssignedUserId.Value);
            await _issueManager.AssignToAsync(issue, user);
        }

        // Save
        await _issueRepository.InsertAsync(issue);

        // Return a DTO represents the new Issue
        return ObjectMapper.Map<Issue, IssueDto>(issue);
    }
}
````

`CreateAsync` method;

* Uses the `Issue` **constructor** to create a valid issue. It passes the `Id` using the [IGuidGenerator](Guid-Generation.md) service. It doesn't use auto object mapping here.
* If the client wants to **assign this issue to a user** on object creation, it uses the `IssueManager` to do it by allowing the `IssueManager` to perform the necessary checks before this assignment.
* **Saves** the entity to the database.
* Finally uses the `IObjectMapper` to return an `IssueDto` that is automatically created by **mapping** from the new `Issue` entity.

#### Applying Domain Rules on Entity Creation

The example `Issue` entity has no business rule on entity creation, except some formal validations in the constructor. However, there maybe scenarios where entity creation should check some extra business rules.

For example, assume that you **don't want** to allow to create an issue if there is already an issue with **exactly the same `Title`**. Where to implement this rule? It is **not proper** to implement this rule in the **Application Service**, because it is a **core business (domain) rule** that should always be checked.

This rule should be implemented in a **Domain Service**, `IssueManager` in this case. So, we need to force the Application Layer always to use the `IssueManager` to create a new `Issue.`

First, we can make the `Issue` constructor `internal`, instead of `public`:

````csharp
public class Issue : AggregateRoot<Guid>
{
    //...

    internal Issue(
        Guid id,
        Guid repositoryId,
        string title,
        string text = null
        ) : base(id)
    {
        RepositoryId = repositoryId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        Text = text; //Allow empty/null
    }
        
    //...
}
````

This prevents Application Services to directly use the constructor, so they will use the `IssueManager`. Then we can add a `CreateAsync` method to the `IssueManager`:

````csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IssueTracking.Issues
{
    public class IssueManager : DomainService
    {
        private readonly IRepository<Issue, Guid> _issueRepository;

        public IssueManager(IRepository<Issue, Guid> issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<Issue> CreateAsync(
            Guid repositoryId,
            string title,
            string text = null)
        {
            if (await _issueRepository.AnyAsync(i => i.Title == title))
            {
                throw new BusinessException("IssueTracking:IssueWithSameTitleExists");
            }

            return new Issue(
                GuidGenerator.Create(),
                repositoryId,
                title,
                text
            );
        }
    }
}
````

* `CreateAsync` method checks if there is already an issue with the same title and throws a business exception in this case.
* If there is no duplication, it creates and returns a new `Issue`.

The `IssueAppService` is changed as shown below in order to use the `IssueManager`'s `CreateAsync` method:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IssueManager _issueManager;
    private readonly IRepository<Issue, Guid> _issueRepository;
    private readonly IRepository<AppUser, Guid> _userRepository;

    public IssueAppService(
        IssueManager issueManager,
        IRepository<Issue, Guid> issueRepository,
        IRepository<AppUser, Guid> userRepository)
    {
        _issueManager = issueManager;
        _issueRepository = issueRepository;
        _userRepository = userRepository;
    }

    public async Task<IssueDto> CreateAsync(IssueCreationDto input)
    {
        // Create a valid entity using the IssueManager
        var issue = await _issueManager.CreateAsync(
            input.RepositoryId,
            input.Title,
            input.Text
        );

        // Apply additional domain actions
        if (input.AssignedUserId.HasValue)
        {
            var user = await _userRepository.GetAsync(input.AssignedUserId.Value);
            await _issueManager.AssignToAsync(issue, user);
        }

        // Save
        await _issueRepository.InsertAsync(issue);

        // Return a DTO represents the new Issue
        return ObjectMapper.Map<Issue, IssueDto>(issue);
    }    
}

// *** IssueCreationDto class ***
public class IssueCreationDto
{
    public Guid RepositoryId { get; set; }
    [Required]
    public string Title { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string Text { get; set; }
}
````

##### Discussion: Why is the Issue not saved to the database in `IssueManager`?

You may ask "**Why didn't `IssueManager` save the `Issue` to the database?**". We think it is the responsibility of the Application Service.

Because, the Application Service may require additional changes/operations on the `Issue` object before saving it. If Domain Service saves it, then the *Save* operation is duplicated;

* It causes performance lost because of double database round trip.
* It requires explicit database transaction that covers both operations.
* If additional actions cancel the entity creation because of a business rule, the transaction should be rolled back in the database.

When you check the `IssueAppService`, you will see the advantage of **not saving** `Issue` to the database in the `IssueManager.CreateAsync`. Otherwise, we would need to perform one *Insert* (in the `IssueManager`) and one *Update* (after the Assignment).

##### Discussion: Why is the duplicate Title check not implemented in the Application Service?

We could simple say "Because it is a **core domain logic** and should be implemented in the Domain Layer". However, it brings a new question "**How did you decide** that it is a core domain logic, but not an application logic?" (we will discuss the difference later with more details).

For this example, a simple question can help us to make the decision: "If we have another way (use case) of creating an issue, should we still apply the same rule? Is that rule should *always* be implemented". You may think "Why do we have a second way of creating an issue?". However, in real life, you have;

* **End users** of the application may create issues in your application's standard UI.
* You may have a second **back office** application that is used by your own employees and you may want to provide a way of creating issues (probably with different authorization rules in this case).
* You may have an HTTP API that is open to **3rd-party clients** and they create issues.
* You may have a **background worker** service that do something and creates issues if it detects some problems. In this way, it will create an issue without any user interaction (and probably without any standard authorization check).
* You may have a button on the UI that **converts** something (for example, a discussion) to an issue.

We can give more examples. All of these are should be implemented by **different Application Service methods** (see the *Multiple Application Layers* section below), but they **always** follow the rule: Title of the new issue can not be same of any existing issue! That's why this logic is a **core domain logic**, should be located in the Domain Layer and **should not be duplicated** in all these application service methods.

### Updating / Manipulating An Entity

Once an entity is created, it is updated/manipulated by the use cases until it is deleted from the system. There can be different types of the use cases directly or indirectly changes an entity.

In this section, we will discuss a typical update operation that changes multiple properties of an `Issue`.

This time, beginning from the *Update* DTO:

````csharp
public class UpdateIssueDto
{
    [Required]
    public string Title { get; set; }
    public string Text { get; set; }
    public Guid? AssignedUserId { get; set; }
}
````

By comparing to `IssueCreationDto`, you see no `RepositoryId`. Because, our system doesn't allow to move issues across repositories (think as GitHub repositories). Only `Title` is required and the other properties are optional.

Let's see the *Update* implementation in the `IssueAppService`:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IssueManager _issueManager;
    private readonly IRepository<Issue, Guid> _issueRepository;
    private readonly IRepository<AppUser, Guid> _userRepository;

    public IssueAppService(
        IssueManager issueManager,
        IRepository<Issue, Guid> issueRepository,
        IRepository<AppUser, Guid> userRepository)
    {
        _issueManager = issueManager;
        _issueRepository = issueRepository;
        _userRepository = userRepository;
    }

    public async Task<IssueDto> UpdateAsync(Guid id, UpdateIssueDto input)
    {
        // Get entity from database
        var issue = await _issueRepository.GetAsync(id);

        // Change Title
        await _issueManager.ChangeTitleAsync(issue, input.Title);

        // Change Assigned User
        if (input.AssignedUserId.HasValue)
        {
            var user = await _userRepository.GetAsync(input.AssignedUserId.Value);
            await _issueManager.AssignToAsync(issue, user);
        }

        // Change Text (no business rule, all values accepted)
        issue.Text = input.Text;

        // Update entity in the database
        await _issueRepository.UpdateAsync(issue);

        // Return a DTO represents the new Issue
        return ObjectMapper.Map<Issue, IssueDto>(issue);
    }
}
````

* `UpdateAsync` method gets `id` as a separate parameter. It is not included in the `UpdateIssueDto`. This is a design decision that helps ABP to properly define HTTP routes when you [auto expose](API/Auto-API-Controllers.md) this service as an HTTP API endpoint. So, that's not related to DDD.
* It starts by **getting** the `Issue` entity **from the database**.
* Uses `IssueManager`'s `ChangeTitleAsync` instead of directly calling `Issue.SetTitle(...)`. Because we need to implement the **duplicate Title check** as just done in the *Entity Creation*. This requires some changes in the `Issue` and `IssueManager` classes (will be explained below).
* Uses `IssueManager`'s `AssignToAsync` method if the **assigned user** is being changed with this request.
* Directly sets the `Issue.Text` since there is **no business rule** for that. If we need later, we can always refactor.
* **Saves changes** to the database. Again, saving changed entities is a responsibility of the Application Service method that coordinates the business objects and the transaction. If the `IssueManager` had saved internally in `ChangeTitleAsync` and `AssignToAsync` method, there would be double database operation (see the *Discussion: Why is the Issue not saved to the database in `IssueManager`?* above).
* Finally uses the `IObjectMapper` to return an `IssueDto` that is automatically created by **mapping** from the updated `Issue` entity.

As said, we need some changes in the `Issue` and `IssueManager` classes.

First, made `SetTitle` internal in the `Issue` class:

````csharp
internal void SetTitle(string title)
{
    Title = Check.NotNullOrWhiteSpace(title, nameof(title));
}
````

Then added a new method to the `IssueManager` to change the Title:

````csharp
public async Task ChangeTitleAsync(Issue issue, string title)
{
    if (issue.Title == title)
    {
        return;
    }

    if (await _issueRepository.AnyAsync(i => i.Title == title))
    {
        throw new BusinessException("IssueTracking:IssueWithSameTitleExists");
    }

    issue.SetTitle(title);
}
````

[Introduction](Introduction.md) | [Next part: Domain and Application Logic](DomainAndApplicationLogic.md)
