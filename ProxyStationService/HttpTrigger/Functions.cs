using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ProxyStation.ProfileParser;
using ProxyStation.Model;
using ProxyStation.Util;

namespace ProxyStation.HttpTrigger
{
    public static class Functions
    {
        public static IDownloader Downloader { get; set; } = new Downloader();

        public static IEnvironmentManager EnvironmentManager { get; set; } = new EnvironmentManager();

        public static string GetCurrentURL(HttpRequest req) => $"{req.Scheme}://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}{req.Path}";

        [FunctionName("GetTrain")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "train/{profileName}/{typeName?}")] HttpRequest req,
            string profileName,
            string typeName,
            ILogger logger)
        {
            var templateUrlOrName = req.Query.ContainsKey("template") ? req.Query["template"].ToString() : String.Empty;

            ProfileFactory.SetEnvironmentManager(Functions.EnvironmentManager);
            TemplateFactory.SetEnvironmentManager(Functions.EnvironmentManager);

            var profile = ProfileFactory.Get(profileName);
            if (profile == null) return new NotFoundResult();

            var profileParser = ParserFactory.GetParser(profile.Type, logger);
            if (profileParser == null)
            {
                logger.LogWarning($"Profile parser for {profile.Type.ToString()} is not implemented!");
                return new NotFoundResult();
            }

            IProfileParser targetProfileParser = ParserFactory.GetParser(typeName ?? "", logger);
            if (targetProfileParser == null)
            {
                var userAgent = req.Headers["user-agent"];
                var probableType = Functions.GuessTypeFromUserAgent(userAgent);
                targetProfileParser = ParserFactory.GetParser(probableType, logger);
                logger.LogInformation("Attempt to guess target type from user agent, UserAgent={userAgent}, Result={targetType}", userAgent, targetProfileParser.GetType());
            }

            string newProfile;
            var profileContent = await profile.Download(Functions.Downloader);
            var fileName = $"{profileName}{targetProfileParser.ExtName()}";

            if (profile.AllowDirectAccess && profileParser.GetType() == targetProfileParser.GetType())
            {
                newProfile = profileContent;
            }
            else
            {
                var template = string.Empty;

                if (!String.IsNullOrEmpty(templateUrlOrName) && !templateUrlOrName.StartsWith("https://"))
                {
                    templateUrlOrName = TemplateFactory.GetTemplateUrl(templateUrlOrName);
                }

                if (!String.IsNullOrEmpty(templateUrlOrName) && templateUrlOrName.StartsWith("https://"))
                {
                    template = await Functions.Downloader.Download(templateUrlOrName);
                }

                var servers = profileParser.Parse(profileContent);
                logger.LogInformation($"Download profile `{profile.Name}` and get {servers.Length} servers");

                foreach (var filter in profile.Filters)
                {
                    var previousCount = servers.Length;
                    servers = filter.Do(servers);
                    logger.LogInformation($"Performed filter `{filter.GetType()}`, result: {servers.Length} servers");
                    if (servers.Length == 0) break;
                }

                EncodeOptions options;
                switch (targetProfileParser)
                {
                    case SurgeParser surgeParser:
                        options = new SurgeEncodeOptions()
                        {
                            ProfileURL = Functions.GetCurrentURL(req)
                        };
                        break;
                    default:
                        options = new EncodeOptions();
                        break;
                }
                options.Template = template;
                options.ProfileName = profile.Name;

                try
                {
                    newProfile = targetProfileParser.Encode(servers, options);
                }
                catch (InvalidTemplateException)
                {
                    return new BadRequestResult();
                }
            }

            var result = new FileContentResult(Encoding.UTF8.GetBytes(newProfile), $"text/plain; charset=UTF-8");
            result.FileDownloadName = fileName;
            return result;
        }

        public static ProfileType GuessTypeFromUserAgent(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("surge")) return ProfileType.Surge;
            else if (userAgent.Contains("clash")) return ProfileType.Clash;
            return ProfileType.General;
        }
    }
}
