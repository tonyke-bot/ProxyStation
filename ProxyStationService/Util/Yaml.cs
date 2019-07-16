using YamlDotNet.RepresentationModel;

namespace ProxyStation.Util {
    public class Yaml {
        public static bool TryGetStringFromYamlChildrenNode(YamlNode rootNode, string keyName, out string buffer)
        {
            buffer = "";
            if (!(rootNode as YamlMappingNode).Children.ContainsKey(keyName)) return false;

            var node = (rootNode as YamlMappingNode).Children[keyName];
            if (!(node is YamlScalarNode)) return false;

            buffer = (node as YamlScalarNode).Value;
            return true;
        }

        public static string GetStringOrDefaultFromYamlChildrenNode(YamlNode rootNode, string keyName, string defaultValue = "")
        {
            if (!(rootNode as YamlMappingNode).Children.ContainsKey(keyName)) return defaultValue;

            var node = (rootNode as YamlMappingNode).Children[keyName];
            if (!(node is YamlScalarNode)) return defaultValue;

            return (node as YamlScalarNode).Value;
        }

        public static bool GetTruthFromYamlChildrenNode(YamlNode rootNode, string keyName)
        {
            if (!(rootNode as YamlMappingNode).Children.ContainsKey(keyName)) return default;

            var node = (rootNode as YamlMappingNode).Children[keyName];
            if (!(node is YamlScalarNode)) return default;

            return (node as YamlScalarNode).Value == "true";
        }
    }
}