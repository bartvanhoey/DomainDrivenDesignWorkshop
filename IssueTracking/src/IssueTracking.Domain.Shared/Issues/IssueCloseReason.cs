namespace IssueTracking.Domain.Shared.Issues
{
  public enum IssueCloseReason
  {
    DueDatePassed = 0,
    Irrelevant = 1,
    Solved = 2,
    OutOfScope = 3,
    NoBandWith = 4
  }
}