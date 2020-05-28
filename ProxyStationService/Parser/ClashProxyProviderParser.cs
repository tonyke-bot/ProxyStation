using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace ProxyStation.ProfileParser
{
    public class ClashProxyProviderParser : IProfileParser
    {
        private ILogger logger;

        private ClashParser parentParser;

        public ClashProxyProviderParser(ILogger logger)
        {
            this.logger = logger;
            this.parentParser = new ClashParser(logger);
        }

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
        {
            var rootElement = new Dictionary<object, object>();
            var proxyServers = this.parentParser.InternalEncode(options, servers, out encodedServers);
            rootElement.Add("proxies", proxyServers);
            return new SerializerBuilder().Build().Serialize(rootElement);
        }

        public Server[] Parse(string profile)
        {
            using (var reader = new StringReader(profile))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var proxyNode = (yaml.Documents[0].RootNode as YamlMappingNode).Children["proxies"];
                return this.parentParser.Parse(proxyNode);
            }
        }

        public string ExtName() => ".yaml";
    }
}