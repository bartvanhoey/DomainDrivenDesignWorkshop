## Implementation: The Building Blocks

This is the essential part of this guide. We will introduce and explain some **explicit rules** with examples. You can follow these rules and apply in your solution while implementing the Domain Driven Design.

### The Example Domain

The examples will use some concepts those are used by GitHub, like `Issue`, `Repository`, `Label` and `User`, you are already familiar with. The figure below shows some of the aggregates, aggregate roots, entities, value object and the relations between them:

![domain driven design example schema](images/domain-driven-design-example-domain-schema.png)

**Issue Aggregate** consists of an `Issue` Aggregate Root that contains `Comment` and `IssueLabel` collections. Other aggregates are shown as simple since we will focus on the Issue Aggregate:

![domain-driven-design-issue-aggregate-diagram](images/domain-driven-design-issue-aggregate-diagram.png)

### Aggregates

As said before, an [Aggregate](https://docs.abp.io/en/abp/latest/Entities) is a cluster of objects (entities and value objects) bound together by an Aggregate Root object. This section will introduce the principles and rules related to the Aggregates.

> We refer the term *Entity* both for *Aggregate Root* and *sub-collection entities* unless we explicitly write *Aggregate Root* or *sub-collection entity*.

#### <a name="theory_exercise_001"></a> Aggregate / Aggregate Root Principles

##### Business Rules

Entities are responsible to implement the business rules related to the properties of their own. The *Aggregate Root Entities* are also responsible for their sub-collection entities.

An aggregate should maintain its self **integrity** and **validity** by implementing domain rules and constraints. That means, unlike the DTOs, Entities have **methods to implement some business logic**. Actually, we should try to implement business rules in the entities wherever possible.

##### Single Unit

An aggregate is **retrieved and saved as a single unit**, with all the sub-collections and properties. For example, if you want to add a `Comment` to an `Issue`, you need to;

* Get the `Issue` from database with including all the sub-collections (`Comment`s and `IssueLabel`s).
* Use methods on the `Issue` class to add a new comment, like `Issue.AddComment(...);`.
* Save the `Issue` (with all sub-collections) to the database as a single database operation (update).

That may seem strange to the developers used to work with **EF Core & Relational Databases** before. Getting the `Issue` with all details seems **unnecessary and inefficient**. Why don't we just execute an SQL `Insert` command to database without querying any data?

The answer is that we should **implement the business** rules and preserve the data **consistency** and **integrity** in the **code**. If we have a business rule like "*Users can not comment on the locked issues*", how can we check the `Issue`'s lock state without retrieving it from the database? So, we can execute the business rules only if the related objects available in the application code.

On the other hand, **MongoDB** developers will find this rule very natural. In MongoDB, an aggregate object (with sub-collections) is saved in a **single collection** in the database (while it is distributed into several tables in a relational database). So, when you get an aggregate, all the sub-collections are already retrieved as a part of the query, without any additional configuration.

ABP Framework helps to implement this principle in your applications.

**Example: Add a comment to an issue**
[Exercise 001](../../exercises/exercise-001-add-a-comment-to-an-issue.md)

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IRepository<Issue, Guid> _issueRepository;

    public IssueAppService(IRepository<Issue, Guid> issueRepository)
    {
        _issueRepository = issueRepository;
    }

    [Authorize]
    public async Task CreateCommentAsync(CreateCommentDto input)
    {
        var issue = await _issueRepository.GetAsync(input.IssueId);
        issue.AddComment(CurrentUser.GetId(), input.Text);
        await _issueRepository.UpdateAsync(issue);
    }
}
````

`_issueRepository.GetAsync` method retrieves the `Issue` with all details (sub-collections) as a single unit by default. While this works out of the box for MongoDB, you need to configure your aggregate details for the EF Core. But, once you configure, repositories automatically handle it. `_issueRepository.GetAsync` method gets an optional parameter, `includeDetails`, that you can pass `false` to disable this behavior when you need it.

> See the *Loading Related Entities* section of the [EF Core document](https://docs.abp.io/en/abp/latest/Entity-Framework-Core) for the configuration and alternative scenarios.

`Issue.AddComment` gets a `userId` and comment `text`, implements the necessary business rules and adds the comment to the Comments collection of the `Issue`.

Finally, we use `_issueRepository.UpdateAsync` to save changes to the database.

> EF Core has a **change tracking** feature. So, you actually don't need to call `_issueRepository.UpdateAsync`. It will be automatically saved thanks to ABP's Unit Of Work system that automatically calls `DbContext.SaveChanges()` at the end of the method. However, for MongoDB, you need to explicitly update the changed entity.
>
> So, if you want to write your code Database Provider independent, you should always call the `UpdateAsync` method for the changed entities.

##### Transaction Boundary

An aggregate is generally considered as a transaction boundary. If a use case works with a single aggregate, reads and saves it as a single unit, all the changes made to the aggregate objects are saved together as an atomic operation and you don't need to an explicit database transaction.

However, in real life, you may need to change **more than one aggregate instances** in a single use case and you need to use database transactions to ensure **atomic update** and **data consistency**. Because of that, ABP Framework uses an explicit database transaction for a use case (an application service method boundary). See the [Unit Of Work](https://docs.abp.io/en/abp/latest/Unit-Of-Work) documentation for more info.

##### Serializability

An aggregate (with the root entity and sub-collections) should be serializable and transferrable on the wire as a single unit. For example, MongoDB serializes the aggregate to JSON document while saving to the database and deserializes from JSON while reading from the database.

> This requirement is not necessary when you use relational databases and ORMs. However, it is an important practice of Domain Driven Design.

The following rules will already bring the serializability.

#### Aggregate / Aggregate Root Rules & Best Practices

The following rules ensures implementing the principles introduced above.

##### Reference Other Aggregates Only By Id

The first rule says an Aggregate should reference to other aggregates only by their Id. That means you can not add navigation properties to other aggregates.

* This rule makes it possible to implement the serializability principle.
* It also prevents different aggregates manipulate each other and leaking business logic of an aggregate to one another.

You see two aggregate roots, `GitRepository` and `Issue` in the example below;

![domain-driven-design-reference-by-id-sample](images/domain-driven-design-reference-by-id-sample.png)

* `GitRepository` should not have a collection of the `Issue`s since they are different aggregates.
* `Issue` should not have a navigation property for the related `GitRepository` since it is a different aggregate.
* `Issue` can have `RepositoryId` (as a `Guid`).

So, when you have an `Issue` and need to have `GitRepository` related to this issue, you need to explicitly query it from database by the `RepositoryId`.

###### For EF Core & Relational Databases

In MongoDB, it is naturally not suitable to have such navigation properties/collections. If you do that, you find a copy of the destination aggregate object in the database collection of the source aggregate since it is being serialized to JSON on save.

However, EF Core & relational database developers may find this restrictive rule unnecessary since EF Core can handle it on database read and write. We see this an important rule that helps to **reduce the complexity** of the domain prevents potential problems and we strongly suggest to implement this rule. However, if you think it is practical to ignore this rule, see the *Discussion About the Database Independence Principle* section above.

##### Keep Aggregates Small

One good practice is to keep an aggregate simple and small. This is because an aggregate will be loaded and saved as a single unit and reading/writing a big object has performance problems. See the example below:

![domain-driven-design-aggregate-keep-small](images/domain-driven-design-aggregate-keep-small.png)

Role aggregate has a collection of `UserRole` value objects to track the users assigned for this role. Notice that `UserRole` is not another aggregate and it is not a problem for the rule *Reference Other Aggregates Only By Id*. However, it is a problem in practical. A role may be assigned to thousands (even millions) of users in a real life scenario and it is a significant performance problem to load thousands of items whenever you query a `Role` from database (remember: Aggregates are loaded by their sub-collections as a single unit).

On the other hand, `User` may have such a `Roles` collection since a user doesn't have much roles in practical and it can be useful to have a list of roles while you are working with a User Aggregate.

If you think carefully, there is one more problem when Role and User both have the list of relation when use a **non-relational database, like MongoDB**. In this case, the same information is duplicated in different collections and it will be hard to maintain data consistency (whenever you add an item to `User.Roles`, you need to add it to `Role.Users` too).

So, determine your aggregate boundaries and size based on the following considerations;

* Objects used together.
* Query (load/save) performance and memory consumption.
* Data integrity, validity and consistency.

In practical;

* Most of the aggregate roots will **not have sub-collections**.
* A sub-collection should not have more than **100-150 items** inside it at the most case. If you think a collection potentially can have more items, don't define the collection as a part of the aggregate and consider to extract another aggregate root for the entity inside the collection.

##### Primary Keys of the Aggregate Roots / Entities

* An aggregate root typically has a single `Id` property for its identifier (Primary Key: PK). We prefer `Guid` as the PK of an aggregate root entity (see the [Guid Generation document](https://docs.abp.io/en/abp/latest/Guid-Generation) to learn why).
* An entity (that's not the aggregate root) in an aggregate can use a composite primary key.

For example, see the Aggregate root and the Entity below:

![domain-driven-design-entity-primary-keys](images/domain-driven-design-entity-primary-keys.png)

* `Organization` has a `Guid` identifier (`Id`).
* `OrganizationUser` is a sub-collection of an `Organization` and has a composite primary key consists of the `OrganizationId` and `UserId`.

That doesn't mean sub-collection entities should always have composite PKs. They may have single `Id` properties when it's needed.

> Composite PKs are actually a concept of relational databases since the sub-collection entities have their own tables and needs to a PK. On the other hand, for example, in MongoDB you don't need to define PK for the sub-collection entities at all since they are stored as a part of the aggregate root.

##### <a name="theory_exercise_002"></a> Constructors of the Aggregate Roots / Entities

The constructor is located where the lifecycle of an entity begins. There are a some responsibilities of a well designed constructor:

* Gets the **required entity properties** as parameters to **create a valid entity**. Should force to pass only for the required parameters and may get non-required properties as optional parameters.
* **Checks validity** of the parameters.
* Initializes **sub-collections**.

**Example: `Issue` (Aggregate Root) constructor**
[Exercise 002](../../exercises/exercise-002-responsibilities-of-a-well-designed-constructor.md)

````csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace IssueTracking.Issues
{
    public class Issue : AggregateRoot<Guid>
    {
        public Guid RepositoryId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Guid? AssignedUserId { get; set; }
        public bool IsClosed { get; set; }
        public IssueCloseReason? CloseReason { get; set; } //enum

        public ICollection<IssueLabel> Labels { get; set; }

        public Issue(
            Guid id,
            Guid repositoryId,
            string title,
            string text = null,
            Guid? assignedUserId = null
            ) : base(id)
        {
            RepositoryId = repositoryId;
            Title = Check.NotNullOrWhiteSpace(title, nameof(title));
            
            Text = text;
            AssignedUserId = assignedUserId;
            
            Labels = new Collection<IssueLabel>();
        }

        private Issue() { /* for deserialization & ORMs */ }
    }
}
````

* `Issue` class properly **forces to create a valid entity** by getting minimum required properties in its constructor as parameters.
* The constructor **validates** the inputs (`Check.NotNullOrWhiteSpace(...)` throws `ArgumentException` if the given value is empty).
* It **initializes the sub-collections**, so you don't get a null reference exception when you try to use the `Labels` collection after creating the `Issue`.
* The constructor also **takes the `id`** and passes to the `base` class. We don't generate `Guid`s inside the constructor to be able to delegate this responsibility to another service (see [Guid Generation](https://docs.abp.io/en/abp/latest/Guid-Generation)).
* Private **empty constructor** is necessary for ORMs. We made it `private` to prevent accidentally using it in our own code.

> See the [Entities](https://docs.abp.io/en/abp/latest/Entities) document to learn more about creating entities with the ABP Framework.

##### Entity Property Accessors & Methods

The example above may seem strange to you! For example, we force to pass a non-null `Title` in the constructor. However, the developer may then set the `Title` property to `null` without any control. This is because the example code above just focuses on the constructor.

If we declare all the properties with **public setters** (like the example `Issue` class above), we can't force **validity** and **integrity** of the entity in its lifecycle. So;

* Use **private setter** for a property when you need to perform any **logic** while setting that property.
* Define public methods to manipulate such properties.

**Example: Methods to change the properties in a controlled way**

````csharp
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace IssueTracking.Issues
{
    public class Issue : AggregateRoot<Guid>
    {
        public Guid RepositoryId { get; private set; } //Never changes
        public string Title { get; private set; } //Needs validation
        public string Text { get; set; } //No validation
        public Guid? AssignedUserId { get; set; } //No validation
        public bool IsClosed { get; private set; } //Should be changed with CloseReason
        public IssueCloseReason? CloseReason { get; private set; } //Should be changed with IsClosed

        //...

        public void SetTitle(string title)
        {
            Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        }

        public void Close(IssueCloseReason reason)
        {
            IsClosed = true;
            CloseReason = reason;
        }

        public void ReOpen()
        {
            IsClosed = false;
            CloseReason = null;
        }
    }
}
````

* `RepositoryId` setter made private and there is no way to change it after creating an `Issue` because this is what we want in this domain: An issue can't be moved to another repository.
* `Title` setter made private and `SetTitle` method has been created if you want to change it later in a controlled way.
* `Text` and `AssignedUserId` has public setters since there is no restriction on them. They can be null or any other value. We think it is unnecessary to define separate methods to set them. If we need later, we can add methods and make the setters private. Breaking changes are not problem in the domain layer since the domain layer is an internal project, it is not exposed to clients.
* `IsClosed` and `IssueCloseReason` are pair properties. Defined `Close` and `ReOpen` methods to change them together. In this way, we prevent to close an issue without any reason.

##### Business Logic & Exceptions in the Entities

When you implement validation and business logic in the entities, you frequently need to manage the exceptional cases. In these cases;

* Create **domain specific exceptions**.
* **Throw these exceptions** in the entity methods when necessary.

**Example**

````csharp
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
            throw new IssueStateException(
                "Can not open a locked issue! Unlock it first."
            );
        }

        IsLocked = true;
    }

    public void Unlock()
    {
        IsLocked = false;
    }
}
````

There are two business rules here;

* A locked issue can not be re-opened.
* You can not lock an open issue.

`Issue` class throws an `IssueStateException` in these cases to force the business rules:

````csharp
using System;

namespace IssueTracking.Issues
{
    public class IssueStateException : Exception
    {
        public IssueStateException(string message)
            : base(message)
        {
            
        }
    }
}
````

There are two potential problems of throwing such exceptions;

1. In case of such an exception, should the **end user** see the exception (error) message? If so, how do you **localize** the exception message? You can not use the [localization](https://docs.abp.io/en/abp/latest/Localization) system, because you can't inject and use `IStringLocalizer` in the entities.
2. For a web application or HTTP API, what **HTTP Status Code** should return to the client?

ABP's [Exception Handling](https://docs.abp.io/en/abp/latest/Exception-Handling) system solves these and similar problems.

**Example: Throwing a business exception with code**

````csharp
using Volo.Abp;

namespace IssueTracking.Issues
{
    public class IssueStateException : BusinessException
    {
        public IssueStateException(string code)
            : base(code)
        {
            
        }
    }
}
````

* `IssueStateException` class inherits the `BusinessException` class. ABP returns 403 (forbidden) HTTP Status code by default (instead of 500 - Internal Server Error) for the exceptions derived from the `BusinessException`.
* The `code` is used as a key in the localization resource file to find the localized message.

Now, we can change the `ReOpen` method as shown below:

````csharp
public void ReOpen()
{
    if (IsLocked)
    {
        throw new IssueStateException("IssueTracking:CanNotOpenLockedIssue");
    }

    IsClosed = false;
    CloseReason = null;
}
````

> Use constants instead of magic strings.

And add an entry to the localization resource like below:

````json
"IssueTracking:CanNotOpenLockedIssue": "Can not open a locked issue! Unlock it first."
````

* When you throw the exception, ABP automatically uses this localized message (based on the current language) to show to the end user.
* The exception code (`IssueTracking:CanNotOpenLockedIssue` here) is also sent to the client, so it may handle the error case programmatically.

> For this example, you could directly throw `BusinessException` instead of defining a specialized `IssueStateException`. The result will be same. See the [exception handling document](https://docs.abp.io/en/abp/latest/Exception-Handling) for all the details.

##### Business Logic in Entities Requiring External Services

It is simple to implement a business rule in an entity method when the business logic only uses the properties of that entity. What if the business logic requires to **query database** or **use any external services** that should be resolved from the [dependency injection](https://docs.abp.io/en/abp/latest/Dependency-Injection) system. Remember; **Entities can not inject services!**

There are two common ways of implementing such a business logic:

* Implement the business logic on an entity method and **get external dependencies as parameters** of the method.
* Create a **Domain Service**.

Domain Services will be explained later. But, now let's see how it can be implemented in the entity class.

**Example: Business Rule: Can not assign more than 3 open issues to a user concurrently**

````csharp
public class Issue : AggregateRoot<Guid>
{
    //...
    public Guid? AssignedUserId { get; private set; }

    public async Task AssignToAsync(AppUser user, IUserIssueService userIssueService)
    {
        var openIssueCount = await userIssueService.GetOpenIssueCountAsync(user.Id);

        if (openIssueCount >= 3)
        {
            throw new BusinessException("IssueTracking:ConcurrentOpenIssueLimit");
        }

        AssignedUserId = user.Id;
    }

    public void CleanAssignment()
    {
        AssignedUserId = null;
    }
}
````

* `AssignedUserId` property setter made private. So, the only way to change it to use the `AssignToAsync` and `CleanAssignment` methods.
* `AssignToAsync` gets an `AppUser` entity. Actually, it only uses the `user.Id`, so you could get a `Guid` value, like `userId`. However, this way ensures that the `Guid` value is `Id` of an existing user and not a random `Guid` value.
* `IUserIssueService` is an arbitrary service that is used to get open issue count for a user. It's the responsibility of the code part (that calls the `AssignToAsync`) to resolve the `IUserIssueService` and pass here.
* `AssignToAsync` throws exception if the business rule doesn't meet.
* Finally, if everything is correct, `AssignedUserId` property is set.

This method perfectly guarantees to apply the business logic when you want to assign an issue to a user. However, it has some problems;

* It makes the entity class **depending on an external service** which makes the entity **complicated**.
* It makes **hard to use** the entity. The code that uses the entity now needs to inject `IUserIssueService` and pass to the `AssignToAsync` method.

An alternative way of implementing this business logic is to introduce a **Domain Service**, which will be explained later.

### Repositories

A [Repository](https://docs.abp.io/en/abp/latest/Repositories) is a collection-like interface that is used by the Domain and Application Layers to access to the data persistence system (the database) to read and write the Business Objects, generally the Aggregates.

Common Repository principles are;

* Define a repository **interface in the Domain Layer** (because it is used in the Domain and Application Layers), **implement in the Infrastructure Layer** (*EntityFrameworkCore* project in the startup template).
* **Do not include business logic** inside the repositories.
* Repository interface should be **database provider / ORM independent**. For example, do not return a `DbSet` from a repository method. `DbSet` is an object provided by the EF Core.
* **Create repositories for aggregate roots**, not for all entities. Because, sub-collection entities (of an aggregate) should be accessed over the aggregate root.

#### Do Not Include Domain Logic in Repositories

While this rule seems obvious at the beginning, it is easy to leak business logic into repositories.

**Example: Get inactive issues from a repository**

````csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Issues
{
    public interface IIssueRepository : IRepository<Issue, Guid>
    {
        Task<List<Issue>> GetInActiveIssuesAsync();
    }
}
````

`IIssueRepository` extends the standard `IRepository<...>` interface by adding a `GetInActiveIssuesAsync` method. This repository works with such an `Issue` class:

````csharp
public class Issue : AggregateRoot<Guid>, IHasCreationTime
{
    public bool IsClosed { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? LastCommentTime { get; private set; }
    //...
}
````

(the code shows only the properties we need for this example)

The rule says the repository shouldn't know the business rules. The question here is "**What is an inactive issue**? Is it a business rule definition?"

Let's see the implementation to understand it:

````csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracking.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IssueTracking.Issues
{
    public class EfCoreIssueRepository : 
        EfCoreRepository<IssueTrackingDbContext, Issue, Guid>,
        IIssueRepository
    {
        public EfCoreIssueRepository(
            IDbContextProvider<IssueTrackingDbContext> dbContextProvider) 
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
    }
}
````

(Used EF Core for the implementation. See the [EF Core integration document](https://docs.abp.io/en/abp/latest/Entity-Framework-Core) to learn how to create custom repositories with the EF Core.)

When we check the `GetInActiveIssuesAsync` implementation, we see a **business rule that defines an in-active issue**: The issue should be **open**, **assigned to nobody**, **created 30+ days ago** and has **no comment in the last 30 days**.

This is an implicit definition of a business rule that is hidden inside a repository method. The problem occurs when we need to reuse this business logic.

For example, let's say that we want to add an `bool IsInActive()` method on the `Issue` entity. In this way, we can check activeness when we have an issue entity.

Let's see the implementation:

````csharp
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
````

We had to copy/paste/modify the code. What if the definition of the activeness changes? We should not forget to update both places. This is a duplication of a business logic, which is pretty dangerous.

A good solution to this problem is the *Specification Pattern*!

### Specifications

A [specification](https://docs.abp.io/en/abp/latest/Specifications) is a **named**, **reusable**, **combinable** and **testable** class to filter the Domain Objects based on the business rules.

ABP Framework provides necessary infrastructure to easily create specification classes and use them inside your application code. Let's implement the in-active issue filter as a specification class:

````csharp
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
````

`Specification<T>` base class simplifies to create a specification class by defining an expression. Just moved the expression here, from the repository.

Now, we can re-use the `InActiveIssueSpecification` in the `Issue` entity and `EfCoreIssueRepository` classes.

#### Using within the Entity

`Specification` class provides an `IsSatisfiedBy` method that returns `true` if the given object (entity) satisfies the specification. We can re-write the `Issue.IsInActive` method as shown below:

````csharp
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
````

Just created a new instance of the `InActiveIssueSpecification` and used its `IsSatisfiedBy` method to re-use the expression defined by the specification.

#### Using with the Repositories

First, starting from the repository interface:

````csharp
public interface IIssueRepository : IRepository<Issue, Guid>
{
    Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec);
}
````

Renamed `GetInActiveIssuesAsync` to simple `GetIssuesAsync` by taking a specification object. Since the **specification (the filter) has been moved out of the repository**, we no longer need to create different methods to get issues with different conditions (like `GetAssignedIssues(...)`, `GetLockedIssues(...)`, etc.)

Updated implementation of the repository can be like that:

````csharp
public class EfCoreIssueRepository :
    EfCoreRepository<IssueTrackingDbContext, Issue, Guid>,
    IIssueRepository
{
    public EfCoreIssueRepository(
        IDbContextProvider<IssueTrackingDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Issue>> GetIssuesAsync(ISpecification<Issue> spec)
    {
        return await DbSet
            .Where(spec.ToExpression())
            .ToListAsync();
    }
}
````

Since `ToExpression()` method returns an expression, it can be directly passed to the `Where` method to filter the entities.

Finally, we can pass any Specification instance to the `GetIssuesAsync` method:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IIssueRepository _issueRepository;

    public IssueAppService(IIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task DoItAsync()
    {
        var issues = await _issueRepository.GetIssuesAsync(
            new InActiveIssueSpecification()
        );
    }
}
````

##### With Default Repositories

Actually, you don't have to create custom repositories to be able to use specifications. The standard `IRepository` already extends the `IQueryable`, so you can use the standard LINQ extension methods over it:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IRepository<Issue, Guid> _issueRepository;

    public IssueAppService(IRepository<Issue, Guid> issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task DoItAsync()
    {
        var issues = AsyncExecuter.ToListAsync(
            _issueRepository.Where(new InActiveIssueSpecification())
        );
    }
}
````

`AsyncExecuter` is a utility provided by the ABP Framework to use asynchronous LINQ extension methods (like `ToListAsync` here) without depending on the EF Core NuGet package. See the [Repositories document](https://docs.abp.io/en/abp/latest/Repositories) for more information.

#### Combining the Specifications

One powerful side of the Specifications is they are combinable. Assume that we have another specification that returns `true` only if the `Issue` is in a Milestone:

````csharp
public class MilestoneSpecification : Specification<Issue>
{
    public Guid MilestoneId { get; }

    public MilestoneSpecification(Guid milestoneId)
    {
        MilestoneId = milestoneId;
    }

    public override Expression<Func<Issue, bool>> ToExpression()
    {
        return i => i.MilestoneId == MilestoneId;
    }
}
````

This Specification is *parametric* as a difference from the `InActiveIssueSpecification`. We can combine both specifications to get a list of inactive issues in a specific milestone:

````csharp
public class IssueAppService : ApplicationService, IIssueAppService
{
    private readonly IRepository<Issue, Guid> _issueRepository;

    public IssueAppService(IRepository<Issue, Guid> issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task DoItAsync(Guid milestoneId)
    {
        var issues = AsyncExecuter.ToListAsync(
            _issueRepository
                .Where(
                    new InActiveIssueSpecification()
                        .And(new MilestoneSpecification(milestoneId))
                        .ToExpression()
                )
        );
    }
}
````

The example above uses the `And` extension method to combine the specifications. There are more combining methods are available, like `Or(...)` and `AndNot(...)`.

> See the [Specifications document](https://docs.abp.io/en/abp/latest/Specifications) for more details about the specification infrastructure provided by the ABP Framework.

### Domain Services

Domain Services implement domain logic which;

* Depends on **services and repositories**.
* Needs to work with **multiple aggregates**, so the logic doesn't properly fit in any of the aggregates.

Domain Services work with Domain Objects. Their methods can **get and return entities, value objects, primitive types**... etc. However, **they don't get/return DTOs**. DTOs is a part of the Application Layer.

**Example: Assigning an issue to a user**

Remember how an issue assignment has been implemented in the `Issue` entity:

````csharp
public class Issue : AggregateRoot<Guid>
{
    //...
    public Guid? AssignedUserId { get; private set; }

    public async Task AssignToAsync(AppUser user, IUserIssueService userIssueService)
    {
        var openIssueCount = await userIssueService.GetOpenIssueCountAsync(user.Id);

        if (openIssueCount >= 3)
        {
            throw new BusinessException("IssueTracking:ConcurrentOpenIssueLimit");
        }

        AssignedUserId = user.Id;
    }

    public void CleanAssignment()
    {
        AssignedUserId = null;
    }
}
````

Here, we will move this logic into a Domain Service.

First, changing the `Issue` class:

````csharp
public class Issue : AggregateRoot<Guid>
{
    //...
    public Guid? AssignedUserId { get; internal set; }
}
````

* Removed the assign-related methods.
* Changed `AssignedUserId` property's setter from `private` to `internal`, to allow to set it from the Domain Service.

The next step is to create a domain service, named `IssueManager`, that has `AssignToAsync` to assign the given issue to the given user.

````csharp
public class IssueManager : DomainService
{
    private readonly IRepository<Issue, Guid> _issueRepository;

    public IssueManager(IRepository<Issue, Guid> issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task AssignToAsync(Issue issue, AppUser user)
    {
        var openIssueCount = await _issueRepository.CountAsync(
            i => i.AssignedUserId == user.Id && !i.IsClosed
        );

        if (openIssueCount >= 3)
        {
            throw new BusinessException("IssueTracking:ConcurrentOpenIssueLimit");
        }

        issue.AssignedUserId = user.Id;
    }
}
````

`IssueManager` can inject any service dependency and use to query open issue count on the user.

> We prefer and suggest to use the `Manager` suffix for the Domain Services.

The only problem of this design is that `Issue.AssignedUserId` is now open to set out of the class. However, it is not `public`. It is `internal` and changing it is possible only inside the same Assembly, the `IssueTracking.Domain` project for this example solution. We think this is reasonable;

* Domain Layer developers are already aware of domain rules and they use the `IssueManager`.
* Application Layer developers are already forces to use the `IssueManager` since they don't directly set it.

While there is a tradeoff between two approaches, we prefer to create Domain Services when the business logic requires to work with external services.

> If you don't have a good reason, we think **there is no need to create interfaces** (like `IIssueManager` for the `IssueManager`) for Domain Services.

### Application Services

An [Application Service](https://docs.abp.io/en/abp/latest/Application-Services) is a stateless service that implements **use cases** of the application. An application service typically **gets and returns DTOs**. It is used by the Presentation Layer. It **uses and coordinates the domain objects** (entities, repositories, etc.) to implement the use cases.

Common principles of an application service are;

* Implement the **application logic** that is specific to the current use-case. Do not implement the core domain logic inside the application services. We will come back to differences between Application  Domain logics.
* **Never get or return entities** for an application service method. This breaks the encapsulation of the Domain Layer. Always get and return DTOs.

**Example: Assign an Issue to a User**

````csharp
using System;
using System.Threading.Tasks;
using IssueTracking.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace IssueTracking.Issues
{
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

        [Authorize]
        public async Task AssignAsync(IssueAssignDto input)
        {
            var issue = await _issueRepository.GetAsync(input.IssueId);
            var user = await _userRepository.GetAsync(input.UserId);

            await _issueManager.AssignToAsync(issue, user);

            await _issueRepository.UpdateAsync(issue);
        }
    }
}
````

An application service method typically has three steps those are implemented here;

1. Get the related domain objects from database to implement the use case.
2. Use domain objects (domain services, entities, etc.) to perform the actual operation.
3. Update the changed entities in the database.

> The last *Update* is not necessary if your are using EF Core since it has a Change Tracking system. If you want to take advantage of this EF Core feature, please see the *Discussion About the Database Independence Principle* section above.

`IssueAssignDto` in this example is a simple DTO class:

````csharp
using System;

namespace IssueTracking.Issues
{
    public class IssueAssignDto
    {
        public Guid IssueId { get; set; }
        public Guid UserId { get; set; }
    }
}
````

### Data Transfer Objects

A [DTO](https://docs.abp.io/en/abp/latest/Data-Transfer-Objects) is a simple object that is used to transfer state (data) between the Application and Presentation Layers. So, Application Service methods gets and returns DTOs.

#### Common DTO Principles & Best Practices

* A DTO **should be serializable**, by its nature. Because, most of the time it is transferred over network. So, it should have a **parameterless (empty) constructor**.
* Should not contain any **business logic**.
* **Never** inherit from or reference to **entities**.

**Input DTOs** (those are passed to the Application Service methods) have different natures than **Output DTOs** (those are returned from the Application Service methods). So, they will be treated differently.

#### Input DTO Best Practices

##### Do not Define Unused Properties for Input DTOs

Define **only the properties needed** for the use case! Otherwise, it will be **confusing for the clients** to use the Application Service method. You can surely define **optional properties**, but they should effect how the use case is working, when the client provides them.

This rule seems unnecessary first. Who would define unused parameters (input DTO properties) for a method? But it happens, especially when you try to reuse input DTOs.

##### Do not Re-Use Input DTOs

Define a **specialized input DTO for each use case** (Application Service method). Otherwise, some properties are not used in some cases and this violates the rule defined above: *Do not Define Unused Properties for Input DTOs*.

Sometimes, it seems appealing to reuse the same DTO class for two use cases, because they are almost same. Even if they are same now, they will probably become different by the time and you will come to the same problem. **Code duplication is a better practice than coupling use cases**.

Another way of reusing input DTOs is **inheriting** DTOs from each other. While this can be useful in some rare cases, most of the time it brings you to the same point.

**Example: User Application Service**

````csharp
public interface IUserAppService : IApplicationService
{
    Task CreateAsync(UserDto input);
    Task UpdateAsync(UserDto input);
    Task ChangePasswordAsync(UserDto input);
}
````

`IUserAppService` uses `UserDto` as the input DTO in all methods (use cases). `UserDto` is defined below:

````csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreationTime { get; set; }
}
````

For this example;

* `Id` is not used in *Create* since the server determines it.
* `Password` is not used in *Update* since we have another method for it.
* `CreationTime` is never used since we can't allow client to send the Creation Time. It should be set in the server.

A true implementation can be like that:

````csharp
public interface IUserAppService : IApplicationService
{
    Task CreateAsync(UserCreationDto input);
    Task UpdateAsync(UserUpdateDto input);
    Task ChangePasswordAsync(UserChangePasswordDto input);
}
````

With the given input DTO classes:

````csharp
public class UserCreationDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserUpdateDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}

public class UserChangePasswordDto
{
    public Guid Id { get; set; }
    public string Password { get; set; }
}
````

This is more maintainable approach although more code is written.

**Exceptional Case**: There can be some exceptions for this rule: If you always want to develop two methods **in parallel**, they may share the same input DTO (by inheritance or direct reuse). For example, if you have a reporting page that has some filters and you have multiple Application Service methods (like screen report, excel report and csv report methods) use the same filters but returns different results, you may want to reuse the same filter input DTO to **couple these use cases**. Because, in this example, whenever you change a filter, you have to make the necessary changes in all the methods to have a consistent reporting system.

##### Input DTO Validation Logic

* Implement only **formal validation** inside the DTO. Use Data Annotation Validation Attributes or implement `IValidatableObject` for formal validation.
* **Do not perform domain validation**. For example, don't try to check unique username constraint in the DTOs.

**Example: Using Data Annotation Attributes**

````csharp
using System.ComponentModel.DataAnnotations;

namespace IssueTracking.Users
{
    public class UserCreationDto
    {
        [Required]
        [StringLength(UserConsts.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(UserConsts.MaxEmailLength)]
        public string Email { get; set; }

        [Required]
        [StringLength(UserConsts.MaxEmailLength,
        MinimumLength = UserConsts.MinPasswordLength)]
        public string Password { get; set; }
    }
}
````

ABP Framework automatically validates input DTOs, throws `AbpValidationException` and returns HTTP Status `400` to the client in case of an invalid input.

> Some developers think it is better to separate the validation rules and DTO classes. We think the declarative (Data Annotation) approach is practical and useful and doesn't cause any design problem. However, ABP also supports [FluentValidation integration](https://docs.abp.io/en/abp/latest/FluentValidation) if you prefer the other approach.
>
> See the [Validation document](https://docs.abp.io/en/abp/latest/Validation) for all validation options.

#### Output DTO Best Practices

* Keep output **DTO count minimum**. **Reuse** where possible (exception: Do not reuse input DTOs as output DTOs).
* Output DTOs can contain **more properties** than used in the client code.
* Return entity DTO from **Create** and **Update** methods.

The main goals of these suggestions are;

* Make client code easy to develop and extend;
  * Dealing with **similar, but not same** DTOs are problematic on the client side.
  * It is common to **need to other properties** on the UI/client in the future. Returning all properties (by considering security and privileges) of an entity makes client code easy to improve without requiring to touch to the backend code.
  * If you are opening your API to **3rd-party clients** that you don't know requirements of each client.
* Make the server side code easy to develop and extend;
  * You have less class to **understand and maintain**.
  * You can reuse the Entity->DTO **object mapping** code.
  * Returning same types from different methods make it easy and clear to create **new methods**.

**Example: Returning Different DTO types from different methods**

````csharp
public interface IUserAppService : IApplicationService
{
    UserDto Get(Guid id);    
    List<UserNameAndEmailDto> GetUserNameAndEmail(Guid id);    
    List<string> GetRoles(Guid id);
    List<UserListDto> GetList();
    UserCreateResultDto Create(UserCreationDto input);
    UserUpdateResultDto Update(UserUpdateDto input);
}
````

*(We didn't use async methods to make the example cleaner, but use async in your real world application!)*

The example code above returns different DTO types for each method. As you can guess, there will be a lot of code duplications for querying data, mapping entities to DTOs.

The `IUserAppService` service above can be simplified:

````csharp
public interface IUserAppService : IApplicationService
{
    UserDto Get(Guid id);
    List<UserDto> GetList();
    UserDto Create(UserCreationDto input);
    UserDto Update(UserUpdateDto input);
}
````

With a single output DTO:

````csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateTime CreationTime { get; set; }
    public List<string> Roles { get; set; }
}
````

* Removed `GetUserNameAndEmail` and `GetRoles` since `Get` method already returns the necessary information.
* `GetList` now returns the same with `Get`.
* `Create` and `Update` also returns the same `UserDto`.

Using the same DTO has a lot of advantages as explained before. For example, think a scenario where you show a **data grid** of Users on the UI. After updating a user, you can get the return value and **update it on the UI**. So, you don't need to call `GetList` again. This is why we suggest to return the entity DTO (`UserDto` here) as return value from the `Create` and `Update` operations.

##### Discussion

Some of the output DTO suggestions may not fit in every scenario. These suggestions can be ignored for **performance** reasons, especially when **large data sets** returned or when you create services for your own UI and you have **too many concurrent requests**.

In these cases, you may want to create **specialized output DTOs with minimal information**. The suggestions above are especially for applications where **maintaining the codebase** is more important than **negligible performance lost**.

#### Object to Object Mapping

Automatic [object to object mapping](https://docs.abp.io/en/abp/latest/Object-To-Object-Mapping) is a useful approach to copy values from one object to another when two objects have same or similar properties.

DTO and Entity classes generally have same/similar properties and you typically need to create DTO objects from Entities. ABP's [object to object mapping system](https://docs.abp.io/en/abp/latest/Object-To-Object-Mapping) with [AutoMapper](http://automapper.org/) integration makes these operations much easier comparing to manual mapping.

* **Use** auto object mapping only for **Entity to output DTO** mappings.
* **Do not use** auto object mapping for **input DTO to Entity** mappings.

There are some reasons why you **should not use** input DTO to Entity auto mapping;

1. An Entity class typically has a **constructor** that takes parameters and ensures valid object creation. Auto object mapping operation generally requires an empty constructor.
2. Most of the entity properties will have **private setters** and you should use methods to change these properties in a controlled way.
3. You typically need to **carefully validate and process** the user/client input rather than blindly mapping to the entity properties.

While some of these problems can be solved through mapping configurations (For example, AutoMapper allows to define custom mapping rules), it makes your business code **implicit/hidden** and **tightly coupled** to the infrastructure. We think the business code should be explicit, clear and easy to understand.

See the *Entity Creation* section below for an example implementation of the suggestions made in this section.

[Introduction](Introduction.md) | [Next part: Example Use Cases](ExampleUseCases.md)
