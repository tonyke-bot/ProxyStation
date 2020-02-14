using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.ServerFilter
{
    public class RegexFilter : BaseFilter
    {
        Regex regex;

        public override void LoadOptions(YamlNode node, ILogger logger)
        {
            var pattern = Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "pattern");
            this.regex = new Regex(pattern);
        }

        public override bool Match(Server server, ILogger logger)
        {
            var match = this.regex.Match(server.Name);
            return match.Success;
        }
    }
}