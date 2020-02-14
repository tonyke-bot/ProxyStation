using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.ServerFilter
{
    public enum NameFilterMatching
    {
        HasPrefix,
        HasSuffix,
        Contains,
    }

    public class NameFilter : BaseFilter
    {
        public string Keyword { get; set; }

        public NameFilterMatching Matching { get; set; }

        public override bool Match(Server server, ILogger logger)
        {
            return this.Matching switch
            {
                NameFilterMatching.HasPrefix => server.Name.StartsWith(Keyword),
                NameFilterMatching.HasSuffix => server.Name.EndsWith(Keyword),
                NameFilterMatching.Contains => server.Name.Contains(Keyword),
                _ => throw new NotImplementedException(),
            };
        }

        public override void LoadOptions(YamlNode node, ILogger logger)
        {
            base.LoadOptions(node, logger);
            if (node == null || node.NodeType != YamlNodeType.Mapping) return;

            Keyword = Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "keyword");
            Matching = (Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "matching")) switch
            {
                "prefix" => NameFilterMatching.HasPrefix,
                "suffix" => NameFilterMatching.HasSuffix,
                "contains" => NameFilterMatching.Contains,
                _ => NameFilterMatching.Contains,
            };
        }
    }
}