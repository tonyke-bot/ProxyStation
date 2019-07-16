using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.ServerFilter
{
    public enum FilterMode
    {
        WhiteList,
        BlackList,
    }

    public abstract class BaseFilter
    {
        public FilterMode Mode { get; set; }

        public virtual void LoadOptions(YamlNode node)
        {
            switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "mode").ToLower())
            {
                case "whitelist": Mode = FilterMode.WhiteList; break;
                case "blacklist":
                default: Mode = FilterMode.BlackList; break;
            }
        }

        public abstract Server[] Do(Server[] servers);
    }
}