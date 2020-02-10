using Microsoft.Extensions.Logging;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public static class ParserFactory
    {
        public static IProfileParser GetParser(ProfileType type, ILogger logger)
        {
            return type switch
            {
                ProfileType.Original => new NullParser(),
                ProfileType.General => new GeneralParser(logger),
                ProfileType.Surge => new SurgeParser(logger),
                ProfileType.SurgeList => new SurgeListParser(logger),
                ProfileType.Clash => new ClashParser(logger),
                _ => null,
            };
        }

        public static IProfileParser GetParser(string type, ILogger logger)
        {
            return (type.ToLower()) switch
            {
                "original" => GetParser(ProfileType.Original, logger),
                "general" => GetParser(ProfileType.General, logger),
                "surge" => GetParser(ProfileType.Surge, logger),
                "surge-list" => GetParser(ProfileType.SurgeList, logger),
                "clash" => GetParser(ProfileType.Clash, logger),
                _ => null,
            };
        }
    }
}