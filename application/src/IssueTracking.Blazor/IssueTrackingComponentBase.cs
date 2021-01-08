using IssueTracking.Localization;
using Volo.Abp.AspNetCore.Components;

namespace IssueTracking.Blazor
{
    public abstract class IssueTrackingComponentBase : AbpComponentBase
    {
        protected IssueTrackingComponentBase()
        {
            LocalizationResource = typeof(IssueTrackingResource);
        }
    }
}
