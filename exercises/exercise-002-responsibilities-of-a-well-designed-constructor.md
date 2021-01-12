# Exercise 002: Responsibilities of a well-designed constructor

[< back to theory](../docs/part3/part3-implementation-the-building-blocks.md#constructors-of-the-aggregate-roots-entities)

## Responsibilities constructor

* Gets the required entity properties as parameters to create a valid entity. Should force to pass only for the required parameters and may get non-required properties as optional parameters.
* Checks validity of the parameters.
* Initializes sub-collections.

Add a **constructor** to the **Issue** class to achieve these requirements.

## Put it into practice

### Checkout branch exercise_002

```bash
git checkout exercise_002
```

### Let's make it work by adding some code

1. Add a **constructor** to class **Issue** in the **Issues** folder of the **Domain** project.

    ```csharp

    // import usings
    // using Volo.Abp;
    
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
            Comments = new Collection<Comment>();
    }

    private Issue() { /* for deserialization & ORMs */ }
    ```

2. Update method **CreateAsync** of **IssueAppService** class in the **Issues** folder of the  **Application** project. You must use a **constructor** because it's **no longer possible to initialize an object with an object initializer**. You also need to add a **IGuidGenerator** field and have it injected in the constructor.

    ```csharp
    // import usings
    // using Volo.Abp.Guids;
    // using Volo.Abp.Users;

    public class IssueAppService : ApplicationService, IIssueAppService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IGuidGenerator _guidGenerator;

        public IssueAppService(IIssueRepository issueRepository, IGuidGenerator guidGenerator )
        {
            _issueRepository = issueRepository;
            _guidGenerator = guidGenerator;
        }
        
        public async Task<IssueDto> CreateAsync(CreateIssueDto input)
        {
            var issue = new Issue(_guidGenerator.Create(), input.RepositoryId, input.Title, input.Text);

            await _issueRepository.InsertAsync(issue);
            return ObjectMapper.Map<Issue, IssueDto>(issue);
        }

        // Other methods here ...
    }
    ```

3. Update the **SeedAsync** method of the **IssueDataSeederContributor** class in the **Domain** project. You also need to use a **constructor** instead of a **object initializer** here.

    ```csharp
    foreach (var issue in DataSeederIssues)
    {
        var issueToInsert = new Issue(Guid.NewGuid(), Guid.NewGuid(), issue.Title, issue.Text);
        var insertedIssue = await _issueRepository.InsertAsync(issueToInsert, autoSave: true);

        // Other code here ...
    } 
    ```

### Run application and Test the AddComment method

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Start the **HttpApi.Host** project by hitting `F5`.

* Open a command prompt in the **Blazor** project and enter `dotnet run`

* Make sure you are logged in. Goto the **Issues** list and double-click on an issue to have its comments displayed and click on the **AddComment** button in the **Actions** dropdown.

* Enter a comment and check if it gets added to the issue.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt).

[< back to theory](../docs/part3/part3-implementation-the-building-blocks.md#constructors-of-the-aggregate-roots-entities)
