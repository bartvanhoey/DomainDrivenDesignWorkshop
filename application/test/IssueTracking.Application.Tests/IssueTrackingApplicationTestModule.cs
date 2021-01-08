using Volo.Abp.Modularity;

namespace IssueTracking
{
    [DependsOn(
        typeof(IssueTrackingApplicationModule),
        typeof(IssueTrackingDomainTestModule)
        )]
    public class IssueTrackingApplicationTestModule : AbpModule
    {

    }
}