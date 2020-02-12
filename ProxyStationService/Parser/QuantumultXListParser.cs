using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;

namespace ProxyStation.ProfileParser
{
    public class QuantumultXListParser : IProfileParser
    {
        private readonly ILogger logger;

        private readonly QuantumultXParser parentParser;

        public QuantumultXListParser(ILogger logger, IDownloader downloader)
        {
            this.logger = logger;
            this.parentParser = new QuantumultXParser(logger, downloader);
        }

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
            => this.parentParser.EncodeProxyList(servers, out encodedServers);

        public Server[] Parse(string profile) => this.parentParser.ParseProxyList(profile);

        public string ExtName() => ".conf";
    }
}