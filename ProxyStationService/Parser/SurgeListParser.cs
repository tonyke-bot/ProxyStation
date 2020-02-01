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

        public bool ValidateTemplate(string template)
        {
            return true;
        }

        public Server[] Parse(string profile) => new SurgeParser(logger).ParseProxyList(profile);

        public string Encode(Server[] servers, EncodeOptions options) => new SurgeParser(logger).EncodeProxyList(servers);

        public string ExtName() => "";
    }
}