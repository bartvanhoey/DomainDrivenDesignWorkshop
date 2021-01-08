using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace IssueTracking.EntityFrameworkCore
{
    public static class IssueTrackingDbContextModelCreatingExtensions
    {
        public static void ConfigureIssueTracking(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            /* Configure your own tables/entities inside here */

            //builder.Entity<YourEntity>(b =>
            //{
            //    b.ToTable(IssueTrackingConsts.DbTablePrefix + "YourEntities", IssueTrackingConsts.DbSchema);
            //    b.ConfigureByConvention(); //auto configure for the base class props
            //    //...
            //});
        }
    }
}