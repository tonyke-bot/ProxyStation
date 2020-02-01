using System;
using ProxyStation.Util;

namespace ProxyStation
{
    public static class TemplateFactory
    {
        static IEnvironmentManager environmentManager;

        public static void SetEnvironmentManager(IEnvironmentManager environmentManager)
        {
            TemplateFactory.environmentManager = environmentManager;
        }

        public static string GetTemplateUrl(string templateName)
        {
            var data = TemplateFactory.environmentManager.Get("Template" + Misc.KebabCase2PascalCase(templateName.ToLower()));
            return data;
        }
    }
}