using System.Linq;
using Microsoft.Extensions.Logging;
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
        public string Name { get; set; }

        public FilterMode Mode { get; set; }

        public abstract bool ShouldKeep(Server server);

        public virtual void LoadOptions(YamlNode node)
        {
            switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "mode").ToLower())
            {
                case "whitelist": Mode = FilterMode.WhiteList; break;
                case "blacklist":
                default: Mode = FilterMode.BlackList; break;
            }
        }

        public Server[] Do(Server[] servers, ILogger logger)
        {
            return servers
                .Where(s =>
                {
                    var keep = this.ShouldKeep(s);
                    if (!keep)
                    {
                        logger.LogDebug($"[{{ComponentName}}] Server {s.Name} is removed.", this.GetType().ToString());
                    }
                    return keep;
                })
                .ToArray();
        }
    }
}