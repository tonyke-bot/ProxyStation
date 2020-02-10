using System.Collections.Generic;
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

        public override bool ShouldKeep(Server server)
        {
            var match = this.Matching switch
            {
                NameFilterMatching.HasPrefix => server.Name.StartsWith(Keyword),
                NameFilterMatching.HasSuffix => server.Name.EndsWith(Keyword),
                NameFilterMatching.Contains => server.Name.Contains(Keyword),
                _ => throw new System.NotImplementedException(),
            };

            return match == (Mode == FilterMode.WhiteList);
        }

        public override void LoadOptions(YamlNode node)
        {
            base.LoadOptions(node);
            if (node == null || node.NodeType != YamlNodeType.Mapping) return;

            Keyword = Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "keyword");
            switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "matching"))
            {
                case "prefix": Matching = NameFilterMatching.HasPrefix; break;
                case "suffix": Matching = NameFilterMatching.HasSuffix; break;
                case "contains":
                default: Matching = NameFilterMatching.Contains; break;
            }
        }
    }
}