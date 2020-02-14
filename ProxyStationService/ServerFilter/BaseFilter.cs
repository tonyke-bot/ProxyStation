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

        public abstract bool Match(Server server, ILogger logger);

        public virtual void LoadOptions(YamlNode node, ILogger logger)
        {
            Mode = (Yaml.GetStringOrDefaultFromYamlChildrenNode(node, "mode").ToLower()) switch
            {
                "whitelist" => FilterMode.WhiteList,
                "blacklist" => FilterMode.BlackList,
                _ => FilterMode.BlackList,
            };
        }

        public Server[] Do(Server[] servers, ILogger logger)
        {
            return servers
                .Where(s =>
                {
                    var match = this.Match(s, logger);
                    var keep = match == (this.Mode == FilterMode.WhiteList);
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