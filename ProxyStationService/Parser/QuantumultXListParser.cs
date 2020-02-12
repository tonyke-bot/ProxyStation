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

        public string Encode(Server[] servers, EncodeOptions options) => this.parentParser.EncodeProxyList(servers, out Server[] _);

        public Server[] Parse(string profile) => this.parentParser.ParseProxyList(profile);

        public string ExtName() => ".conf";
    }
}