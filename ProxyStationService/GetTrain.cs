using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProxyStation.ProfileParser;
using System.Text;
using System;

namespace ProxyStation
{
    public static class GetTrain
    {
        public static string GetCurrentURL(HttpRequest req) => $"{req.Scheme}://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}{req.Path}";

        [FunctionName("GetTrain")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "train/{profileName}/{typeName}")] HttpRequest req,
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

            var targetProfileParser = ParserFactory.GetParser(typeName, logger);
            if (targetProfileParser == null) return new NotFoundResult();

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
    }
}
