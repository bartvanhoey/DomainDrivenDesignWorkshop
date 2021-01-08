using IssueTracking.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace IssueTracking.Permissions
{
    public class IssueTrackingPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup(IssueTrackingPermissions.GroupName);

            //Define your own permissions here. Example:
            //myGroup.AddPermission(IssueTrackingPermissions.MyPermission1, L("Permission:MyPermission1"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<IssueTrackingResource>(name);
        }
    }
}
