using IssueTracking.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace IssueTracking.DbMigrator
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(IssueTrackingEntityFrameworkCoreDbMigrationsModule),
        typeof(IssueTrackingApplicationContractsModule)
        )]
    public class IssueTrackingDbMigratorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
