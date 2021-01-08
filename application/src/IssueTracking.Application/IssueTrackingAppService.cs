using System;
using System.Collections.Generic;
using System.Text;
using IssueTracking.Localization;
using Volo.Abp.Application.Services;

namespace IssueTracking
{
    /* Inherit your application services from this class.
     */
    public abstract class IssueTrackingAppService : ApplicationService
    {
        protected IssueTrackingAppService()
        {
            LocalizationResource = typeof(IssueTrackingResource);
        }
    }
}
