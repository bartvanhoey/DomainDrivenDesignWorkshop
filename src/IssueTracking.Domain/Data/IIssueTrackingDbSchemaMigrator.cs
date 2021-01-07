using System.Threading.Tasks;

namespace IssueTracking.Data
{
    public interface IIssueTrackingDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
