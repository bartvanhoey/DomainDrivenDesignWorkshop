using IssueTracking.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace IssueTracking
{
    [DependsOn(
        typeof(IssueTrackingEntityFrameworkCoreTestModule)
        )]
    public class IssueTrackingDomainTestModule : AbpModule
    {

    }
}