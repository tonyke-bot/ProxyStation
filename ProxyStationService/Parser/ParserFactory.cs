using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;

namespace ProxyStation.ProfileParser
{
    public static class ParserFactory
    {
        public static IProfileParser GetParser(ProfileType type, ILogger logger, IDownloader downloader)
        {
            return type switch
            {
                ProfileType.Original => new NullParser(),
                ProfileType.General => new GeneralParser(logger),
                ProfileType.Surge => new SurgeParser(logger),
                ProfileType.SurgeList => new SurgeListParser(logger),
                ProfileType.Clash => new ClashParser(logger),
                ProfileType.ClashProxyProvider => new ClashProxyProviderParser(logger),
                ProfileType.QuantumultX => new QuantumultXParser(logger, downloader),
                ProfileType.QuantumultXList => new QuantumultXListParser(logger, downloader),
                _ => null,
            };
        }

        public static IProfileParser GetParser(string type, ILogger logger, IDownloader downloader)
        {
            var profileType = (type.ToLower()) switch
            {
                "original" => ProfileType.Original,
                "general" => ProfileType.General,
                "surge" => ProfileType.Surge,
                "surge-list" => ProfileType.SurgeList,
                "clash" => ProfileType.Clash,
                "clash-proxy-provider" => ProfileType.ClashProxyProvider,
                "quantumult-x" => ProfileType.QuantumultX,
                "quantumult-x-list" => ProfileType.QuantumultXList,
                _ => ProfileType.None,
            };

            if (profileType == ProfileType.None)
            {
                return null;
            }

            return ParserFactory.GetParser(profileType, logger, downloader);
        }
    }
}