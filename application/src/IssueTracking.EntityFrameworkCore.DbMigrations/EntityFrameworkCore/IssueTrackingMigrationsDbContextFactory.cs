using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IssueTracking.EntityFrameworkCore
{
    /* This class is needed for EF Core console commands
     * (like Add-Migration and Update-Database commands) */
    public class IssueTrackingMigrationsDbContextFactory : IDesignTimeDbContextFactory<IssueTrackingMigrationsDbContext>
    {
        public IssueTrackingMigrationsDbContext CreateDbContext(string[] args)
        {
            IssueTrackingEfCoreEntityExtensionMappings.Configure();

            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<IssueTrackingMigrationsDbContext>()
                .UseSqlServer(configuration.GetConnectionString("Default"));

            return new IssueTrackingMigrationsDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../IssueTracking.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
