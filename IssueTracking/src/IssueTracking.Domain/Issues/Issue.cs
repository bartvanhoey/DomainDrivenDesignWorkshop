using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IssueTracking.Domain.Shared.Issues;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace IssueTracking.Domain.Issues
{
  public class Issue : AggregateRoot<Guid>, IHasCreationTime
  {
    public Guid RepositoryId { get; private set; }
    public string Title { get; private set; }
    public string Text { get; set; }
    public bool IsLocked { get; private set; }
    public IssueCloseReason? CloseReason { get; private set; }
    public ICollection<IssueLabel> Labels { get; private set; }
    public ICollection<Comment> Comments { get; private set; }
    
    public bool IsClosed { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? LastCommentTime { get; private set; }

    public Issue(Guid id, Guid repositoryId, string title, string text = null, Guid? assignedUserId = null) : base(id)
    {
      RepositoryId = repositoryId;
      Title = Check.NotNullOrWhiteSpace(title, nameof(title));

      Text = text;
      AssignedUserId = assignedUserId;

      // added for seeding
      var random = new Random();
      var days = random.Next(-60,0);
      CreationTime = DateTime.Now.AddDays(days);

      Labels = new Collection<IssueLabel>();
      Comments = new Collection<Comment>();



    }

    private Issue() { /* for deserialization & ORMs */ }

    public void AddComment(Guid userId, string text)
    {
      Comments ??= new Collection<Comment>();
      Comments.Add(new Comment { IssueId = this.Id, Text = text, UserId = userId });
    }

    public void SetTitle(string title)
    {
      Title = Check.NotNullOrWhiteSpace(title, nameof(title));
    }

    public void SetAssignedUserId(Guid? assignedUserId)
    {
      AssignedUserId = assignedUserId;
    }

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
        throw new IssueStateException("Can not open a locked issue! Unlock it first.");
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
            "Can not lock an open issue! Close it first."
        );
      }

      IsLocked = true;
    }

    public void Unlock()
    {
      IsLocked = false;
    }

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
}