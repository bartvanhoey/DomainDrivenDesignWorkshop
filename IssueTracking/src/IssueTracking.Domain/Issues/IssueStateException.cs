using System;

namespace IssueTracking.Domain.Issues
{
    public class IssueStateException : Exception
    {
        public IssueStateException(string message) : base(message)
        {
            
        }
    }
}