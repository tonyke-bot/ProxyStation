﻿using System;
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

namespace ProxyStation.HttpTrigger
{
    public static class Functions
    {
        public static IDownloader Downloader { get; set; } = new Downloader();

        public static IEnvironmentManager EnvironmentManager { get; set; } = new EnvironmentManager();

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
            IProfileParser targetProfileParser = ParserFactory.GetParser(typeName ?? "", logger);
            if (targetProfileParser == null)
            {
                var userAgent = req.Headers["user-agent"];
                var probableType = Functions.GuessTypeFromUserAgent(userAgent);
                targetProfileParser = ParserFactory.GetParser(probableType, logger);
                logger.LogInformation("Attempt to guess target type from user agent, UserAgent={userAgent}, Result={targetType}", userAgent, targetProfileParser.GetType());
            }

            // Get profile chain
            var profileChain = new List<Profile>();
            var nextProfileName = Misc.KebabCase2PascalCase(profileName);
            while (true)
            {
                var profile = ProfileFactory.Get(nextProfileName);
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

            // Download content and determine if original profile should be returned
            var profileContent = await sourceProfile.Download(Functions.Downloader);
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
                    FileDownloadName = profileChain.First().Name
                };
            }

            // Download template, parse profile and apply filters
            var template = await Functions.GetTemplate(templateUrlOrName);
            var profileParser = ParserFactory.GetParser(sourceProfile.Type, logger);
            if (profileParser == null)
            {
                logger.LogError($"Profile parser for {sourceProfile.Type} is not implemented! Complete profile alias chain is `{Functions.ProfileChainToString(profileChain)}`");
                return new ForbidResult();
            }
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
                SurgeParser surgeParser => new SurgeEncodeOptions()
                {
                    ProfileURL = requestUrl
                },
                _ => new EncodeOptions(),
            };
            options.Template = template;
            options.ProfileName = profileChain.First().Name;

            try
            {
                var newProfile = targetProfileParser.Encode(servers, options);
                return new FileContentResult(Encoding.UTF8.GetBytes(newProfile), "text/plain; charset=UTF-8")
                {
                    FileDownloadName = profileChain.First().Name
                };
            }
            catch (InvalidTemplateException)
            {
                return new BadRequestResult();
            }
        }

        #endregion Functions

        public static async Task<string> GetTemplate(string templateUrlOrName)
        {
            if (!string.IsNullOrEmpty(templateUrlOrName) && !templateUrlOrName.StartsWith("https://"))
            {
                templateUrlOrName = TemplateFactory.GetTemplateUrl(templateUrlOrName);
            }

            if (!string.IsNullOrEmpty(templateUrlOrName) && templateUrlOrName.StartsWith("https://"))
            {
                return await Functions.Downloader.Download(templateUrlOrName);
            }
            return null;
        }

        public static string ProfileChainToString(IEnumerable<Profile> profileChain)
        {
            return string.Join("->", profileChain.Select(s => s.Name));
        }

        public static string GetCurrentURL(HttpRequest req) => $"{req.Scheme}://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}{req.Path}";

        public static ProfileType GuessTypeFromUserAgent(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("surge")) return ProfileType.Surge;
            else if (userAgent.Contains("clash")) return ProfileType.Clash;
            return ProfileType.General;
        }
    }
}