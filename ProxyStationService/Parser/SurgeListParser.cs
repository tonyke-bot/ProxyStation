using Microsoft.Extensions.Logging;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public class SurgeListParser : IProfileParser
    {
        private ILogger logger;

        public SurgeListParser(ILogger logger)
        {
            this.logger = logger;
        }

        public Server[] Parse(string profile) => new SurgeParser(logger).ParseProxyList(profile);

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
            => new SurgeParser(logger).EncodeProxyList(servers, out encodedServers);

        public string ExtName() => "";
    }
}