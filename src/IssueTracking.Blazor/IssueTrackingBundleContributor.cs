using Volo.Abp.Bundling;

namespace IssueTracking.Blazor
{
    public class IssueTrackingBundleContributor : IBundleContributor
    {
        public void AddScripts(BundleContext context)
        {
        }

        public void AddStyles(BundleContext context)
        {
            context.Add("main.css", true);
        }
    }
}