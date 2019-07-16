using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

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

        public override Server[] Do(Server[] servers)
        {
            var newServers = new List<Server>();
            var match = true;

            foreach (var server in servers)
            {
                switch (Matching)
                {
                    case NameFilterMatching.HasPrefix: match = server.Name.StartsWith(Keyword); break;
                    case NameFilterMatching.HasSuffix: match = server.Name.EndsWith(Keyword); break;
                    case NameFilterMatching.Contains: match = server.Name.Contains(Keyword); break;
                }

                if (match == (Mode == FilterMode.WhiteList)) newServers.Add(server);
            }

            return newServers.ToArray();
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