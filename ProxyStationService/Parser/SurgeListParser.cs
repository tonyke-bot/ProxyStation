using System;
using System.Collections.Generic;
using System.Text;
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

        public string Encode(Server[] servers, IEncodeOptions options) => new SurgeParser(logger).EncodeProxyList(servers);

        public string Encode(Server[] servers) => Encode(servers, null);

        public string ExtName() => "";
    }
}