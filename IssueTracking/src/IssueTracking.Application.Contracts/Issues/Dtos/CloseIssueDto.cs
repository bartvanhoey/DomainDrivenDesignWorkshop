using System;
using IssueTracking.Domain.Shared.Issues;
using Volo.Abp.Application.Dtos;

namespace IssueTracking.Application.Contracts.Issues
{
    public class CloseIssueDto :  EntityDto<Guid>
    {
        public IssueCloseReason CloseReason { get; set; }
    }
}