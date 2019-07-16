using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProxyStation.Model;
using ProxyStation.ServerFilter;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;

namespace ProxyStation
{
    public static class ProfileFactory
    {
        public static ProfileType ParseProfileTypeName(string profileType)
        {
            switch (profileType)
            {
                case "general": return ProfileType.General;
                case "surge": return ProfileType.Surge;
                case "clash": return ProfileType.Clash;
                case "surfboard": return ProfileType.Surfboard;
                case "surge-list": return ProfileType.SurgeList;
                default: return ProfileType.None;
            }
        }

        public static string KebabCase2PascalCase(string kebabCase)
        {
            var words = kebabCase
                .Split('-')
                .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1));

            return String.Join("", words);
        }

        public static Profile Get(string profileName)
        {
            Environment.GetEnvironmentVariables();
            var data = Environment.GetEnvironmentVariable(KebabCase2PascalCase(profileName.ToLower()));
            if (String.IsNullOrEmpty(data)) return null;

            var profile = new Profile();
            using (var reader = new StringReader(data))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);
                var mapping = (yaml.Documents[0].RootNode as YamlMappingNode).Children;

                profile.Source = (mapping["source"] as YamlScalarNode).Value;
                profile.Name = (mapping["name"] as YamlScalarNode).Value;
                profile.Type = ParseProfileTypeName((mapping["type"] as YamlScalarNode).Value);
                profile.AllowDirectAccess = Yaml.GetTruthFromYamlChildrenNode(yaml.Documents[0].RootNode, "allowDirectAccess");

                YamlNode filtersNode;
                if (!mapping.TryGetValue("filters", out filtersNode) || filtersNode.NodeType != YamlNodeType.Sequence)
                {
                    goto EndReading;
                }

                foreach (var filterNode in (filtersNode as YamlSequenceNode).Children)
                {
                    if (filterNode.NodeType != YamlNodeType.Mapping) continue;
                    var filter = FilterFactory.GetFilter(Yaml.GetStringOrDefaultFromYamlChildrenNode(filterNode, "name"));
                    if (filter == null) continue;
                    filter.LoadOptions(filterNode);
                    profile.Filters.Add(filter);
                }
            }

        EndReading:
            return profile;
        }
    }
}