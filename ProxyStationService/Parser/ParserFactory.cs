using Microsoft.Extensions.Logging;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public static class ParserFactory
    {
        public static IProfileParser GetParser(ProfileType type, ILogger logger)
        {
            switch (type)
            {
                case ProfileType.General: return new GeneralParser(logger);
                case ProfileType.Surge: return new SurgeParser(logger);
                case ProfileType.SurgeList: return new SurgeListParser(logger);
                case ProfileType.Clash: return new ClashParser(logger);
                default: return null;
            }
        }

        public static IProfileParser GetParser(string type, ILogger logger)
        {
            switch (type)
            {
                case "general": return GetParser(ProfileType.General, logger);
                case "surge": return GetParser(ProfileType.Surge, logger);
                case "surge-list": return GetParser(ProfileType.SurgeList, logger);
                case "clash": return GetParser(ProfileType.Clash, logger);
                default: return null;
            }
        }
    }
}