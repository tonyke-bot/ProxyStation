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
using System.Collections.Generic;
using System.Web;

namespace ProxyStation.HttpTrigger
{
    public static class Functions
    {
        public static IEnvironmentManager EnvironmentManager { get; set; } = new EnvironmentManager();

        private static IDownloader downloader;

        public static IDownloader Downloader
        {
            get
            {
                if (Functions.downloader == null)
                {
                    var useCache = Functions.EnvironmentManager.Get("USE_CACHE") == "1";
                    if (useCache)
                    {
                        var connectionString = Functions.EnvironmentManager.Get("AzureWebJobsStorage");
                        Functions.downloader = new Downloader(connectionString);
                    }
                    else
                    {
                        Functions.downloader = new Downloader();
                    }
                }

                return Functions.downloader;
            }
            set
            {
                Functions.downloader = value;
            }
        }
        #region Functions

        [FunctionName("GetTrain")]
        public static async Task<IActionResult> GetTrain(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "train/{profileName}/{typeName?}")] HttpRequest req,
            string profileName,
            string typeName,
            ILogger logger)
        {
            var templateUrlOrName = req.Query.ContainsKey("template") ? req.Query["template"].ToString() : String.Empty;
            var requestUrl = Functions.GetCurrentURL(req);

            ProfileFactory.SetEnvironmentManager(Functions.EnvironmentManager);
            TemplateFactory.SetEnvironmentManager(Functions.EnvironmentManager);

            // Parse target parser type
            IProfileParser targetProfileParser = ParserFactory.GetParser(typeName ?? "", logger, Functions.Downloader);
            if (targetProfileParser == null)
            {
                var userAgent = req.Headers["user-agent"];
                var probableType = Functions.GuessTypeFromUserAgent(userAgent);
                targetProfileParser = ParserFactory.GetParser(probableType, logger, Functions.Downloader);
                logger.LogInformation("Attempt to guess target type from user agent, UserAgent={userAgent}, Result={targetType}", userAgent, targetProfileParser.GetType());
            }

            // Get profile chain
            var profileChain = new List<Profile>();
            var nextProfileName = Misc.KebabCase2PascalCase(profileName);
            while (true)
            {
                var profile = ProfileFactory.Get(nextProfileName, logger);
                if (profile == null)
                {
                    var chainString = Functions.ProfileChainToString(profileChain);
                    if (!string.IsNullOrEmpty(chainString)) chainString += "->";
                    chainString += nextProfileName;

                    logger.LogError($"Profile `{chainString}` is not found.");
                    return new NotFoundResult();
                }

                if (profileChain.Contains(profile))
                {
                    return new ForbidResult();
                }

                profileChain.Add(profile);
                if (profile.Type != ProfileType.Alias) break;
                nextProfileName = profile.Source;
            }

            var sourceProfile = profileChain.Last();
            var profileParser = ParserFactory.GetParser(sourceProfile.Type, logger, Functions.Downloader);
            if (profileParser == null)
            {
                logger.LogError($"Profile parser for {sourceProfile.Type} is not implemented! Complete profile alias chain is `{Functions.ProfileChainToString(profileChain)}`");
                return new ForbidResult();
            }

            // Download content and determine if original profile should be returned
            var profileContent = await sourceProfile.Download(logger, Functions.Downloader);
            if (targetProfileParser is NullParser)
            {
                if (!sourceProfile.AllowDirectAccess)
                {
                    logger.LogError($"Original profile access is denied for profile `{Functions.ProfileChainToString(profileChain)}`.");
                    return new ForbidResult();
                }

                logger.LogInformation("Return original profile");
                return new FileContentResult(Encoding.UTF8.GetBytes(profileContent), "text/plain; charset=UTF-8")
                {
                    FileDownloadName = profileChain.First().Name + profileParser.ExtName(),
                };
            }

            // Download template, parse profile and apply filters
            var template = await Functions.GetTemplate(logger, templateUrlOrName);
            var servers = profileParser.Parse(profileContent);
            logger.LogInformation($"Download profile `{Functions.ProfileChainToString(profileChain)}` and get {servers.Length} servers");
            foreach (var profile in profileChain.AsEnumerable().Reverse())
            {
                foreach (var filter in profile.Filters)
                {
                    servers = filter.Do(servers, logger);
                    logger.LogInformation($"Apply filter `{filter.GetType()} from profile `{profile.Name}` and get {servers.Length} servers");
                    if (servers.Length == 0) break;
                }
                if (servers.Length == 0) break;
            }
            if (servers.Length == 0)
            {
                logger.LogError($"There are no available servers left. Complete profile alias chain is `{Functions.ProfileChainToString(profileChain)}`");
                return new NoContentResult();
            }

            // Encode profile
            logger.LogInformation($"{servers.Length} will be encoded");
            var options = targetProfileParser switch
            {
                SurgeParser _ => new SurgeEncodeOptions()
                {
                    ProfileURL = requestUrl + (string.IsNullOrEmpty(template) ? "" : $"?template={HttpUtility.UrlEncode(templateUrlOrName)}")
                },
                QuantumultXParser _ => new QuantumultXEncodeOptions()
                {
                    QuantumultXListUrl = Functions.GetCurrentURL(req) + "-list",
                },
                ClashParser _ => new ClashEncodeOptions()
                {
                    ClashProxyProviderUrl = Functions.GetCurrentURL(req) + "-proxy-provider",
                },
                _ => new EncodeOptions(),
            };
            options.Template = template;
            options.ProfileName = profileChain.First().Name;

            try
            {
                var newProfile = targetProfileParser.Encode(options, servers, out Server[] encodedServer);
                if (encodedServer.Length == 0)
                {
                    return new NoContentResult();
                }

                return new FileContentResult(Encoding.UTF8.GetBytes(newProfile), "text/plain; charset=UTF-8")
                {
                    FileDownloadName = profileChain.First().Name + targetProfileParser.ExtName(),
                };
            }
            catch (InvalidTemplateException)
            {
                return new BadRequestResult();
            }
        }

        #endregion Functions

        public static async Task<string> GetTemplate(ILogger logger, string templateUrlOrName)
        {
            if (!string.IsNullOrEmpty(templateUrlOrName) && !templateUrlOrName.StartsWith("https://"))
            {
                templateUrlOrName = TemplateFactory.GetTemplateUrl(templateUrlOrName);
            }

            if (!string.IsNullOrEmpty(templateUrlOrName) && templateUrlOrName.StartsWith("https://"))
            {
                return await Functions.Downloader.Download(logger, templateUrlOrName);
            }
            return null;
        }

        public static string ProfileChainToString(IEnumerable<Profile> profileChain)
        {
            return string.Join("->", profileChain.Select(s => s.Name));
        }

        public static string GetCurrentURL(HttpRequest req) => $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";

        public static ProfileType GuessTypeFromUserAgent(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("surge")) return ProfileType.Surge;
            else if (userAgent.Contains("clash")) return ProfileType.Clash;
            return ProfileType.General;
        }
    }
}
