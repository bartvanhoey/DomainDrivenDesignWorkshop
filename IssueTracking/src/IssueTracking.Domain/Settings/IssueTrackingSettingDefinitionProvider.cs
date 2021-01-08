using Volo.Abp.Settings;

namespace IssueTracking.Settings
{
    public class IssueTrackingSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(IssueTrackingSettings.MySetting1));
        }
    }
}
