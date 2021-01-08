using IssueTracking.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace IssueTracking.Controllers
{
    /* Inherit your controllers from this class.
     */
    public abstract class IssueTrackingController : AbpController
    {
        protected IssueTrackingController()
        {
            LocalizationResource = typeof(IssueTrackingResource);
        }
    }
}