using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace IssueTracking.EntityFrameworkCore
{
    [DependsOn(
        typeof(IssueTrackingEntityFrameworkCoreModule)
        )]
    public class IssueTrackingEntityFrameworkCoreDbMigrationsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<IssueTrackingMigrationsDbContext>();
        }
    }
}
