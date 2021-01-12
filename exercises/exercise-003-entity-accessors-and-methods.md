# Exercise 003: Responsibilities of a well-designed constructor

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_003)

## Responsibilities constructor

If we declare all the properties with **public setters** (like in the `Issue` class), we can't force **validity** and **integrity** of the entity in its lifecycle. So;

* Use **private setter** for a property when you need to perform any **logic** while setting that property.
* Define public methods to manipulate such properties.

## Put it into practice

### Checkout branch exercise_003

```bash
git checkout exercise_003
```

### Let's make it work by adding some code

1. Update class **Issue** in the **Issues** folder of the **Domain** project.

    ```csharp

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
    ```

2. Update method **UpdateAsync** of **IssueAppService** class in the **Issues** folder of the  **Application** project. You must use the **SetTitle** method as the Title property has no public setter anymore.

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

        //.....
    }
    ```

### Run application and Test the AddComment method

* Delete **database IssueTracking** in **SQL Server** to have a clean start.

* Open a **command prompt** in the **DbMigrator** project and enter `dotnet run` to apply migrations and seed the data.

* Select **ApiDevelopment** in the **dropdown** of the **Debug Window** and run the **HttpApi.Host** project.

* Open a command prompt in the **Blazor** project and enter `dotnet run`.

* Make sure you are logged in. Goto the **Issues** list and double-click on an issue to have its comments displayed and click on the **AddComment** button in the **Actions** dropdown.

* Enter a comment and check if it gets added to the issue.

### Stop application

* Stop both the API (by pressing `SHIFT+F5`) and the Blazor project (by pressing `CTRL+C` in the command prompt).

[< back to theory](../docs/part3/part3-Implementation-The-Building-Blocks.md#theory_exercise_003)
