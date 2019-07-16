using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProxyStation.ProfileParser;
using System.Text;
using System;
using ProxyStation.Model;

namespace ProxyStation
{
    public static class GetTrain
    {
        public static string GetCurrentURL(HttpRequest req) => $"{req.Scheme}://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}{req.Path}";

        [FunctionName("GetTrain")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "train/{profileName}/{typeName?}")] HttpRequest req,
            string profileName,
            string typeName,
            ILogger logger)
        {
            var profile = ProfileFactory.Get(profileName);
            if (profile == null) return new NotFoundResult();

            var profileParser = ParserFactory.GetParser(profile.Type, logger);
            if (profileParser == null)
            {
                logger.LogWarning($"Profile parser for {profile.Type.ToString()} is not implemented!");
                return new NotFoundResult();
            }

            IProfileParser targetProfileParser = ParserFactory.GetParser(typeName ?? "", logger);
            if (targetProfileParser == null) {
                var userAgent = req.Headers["user-agent"];
                var probableType = GuessTypeFromUserAgent(userAgent);
                targetProfileParser = ParserFactory.GetParser(probableType, logger);
                logger.LogInformation("Attempt to guess target type from user agent, UserAgent={userAgent}, Result={targetType}", userAgent, targetProfileParser.GetType());
            }

            string newProfile;
            var profileContent = await profile.Download();
            var fileName = $"{profileName}{targetProfileParser.ExtName()}";

            if (profile.AllowDirectAccess && profileParser.GetType() == targetProfileParser.GetType())
            {
                newProfile = profileContent;
            }
            else
            {
                var servers = profileParser.Parse(profileContent);
                logger.LogInformation($"Download profile `{profile.Name}` and get {servers.Length} servers");

                foreach (var filter in profile.Filters)
                {
                    var previousCount = servers.Length;
                    servers = filter.Do(servers);
                    logger.LogInformation($"Performed filter `{filter.GetType()}`, result: {servers.Length} servers");
                    if (servers.Length == 0) break;
                }

                if (targetProfileParser is SurgeParser)
                {
                    newProfile = targetProfileParser.Encode(servers, new SurgeEncodeOptions()
                    {
                        ProfileURL = GetCurrentURL(req),
                    });
                }
                else newProfile = targetProfileParser.Encode(servers);
            }

            var result = new FileContentResult(Encoding.UTF8.GetBytes(newProfile), $"{MimeTypes.GetMimeType(fileName)}; charset=UTF-8");
            result.FileDownloadName = fileName;
            return result;
        }

        public static ProfileType GuessTypeFromUserAgent(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("surge")) return ProfileType.Surge;
            else if (userAgent.Contains("clash")) return ProfileType.Clash;
            else if (userAgent.Contains("surfboard")) return ProfileType.Surfboard;
            return ProfileType.General;
        }
    }
}
