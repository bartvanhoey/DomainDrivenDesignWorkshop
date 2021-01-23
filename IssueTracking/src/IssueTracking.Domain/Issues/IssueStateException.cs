using System;
using Volo.Abp;

namespace IssueTracking.Domain.Issues
{
  public class IssueStateException : BusinessException
  {
    public IssueStateException(string code) : base(code)
    {

    }
  }
}