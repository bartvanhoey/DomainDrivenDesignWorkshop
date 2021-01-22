using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace IssueTracking.Blazor
{
    [Dependency(ReplaceServices = true)]
    public class IssueTrackingBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "IssueTracking";
    }
}
