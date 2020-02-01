using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using ProxyStation.HttpTrigger;
using ProxyStation.Util;
using Xunit;

namespace ProxyStation.Tests.HttpTrigger
{
    public class FunctionsTests
    {
        [Fact]
        public async Task ShouldSuccessWithSurge()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile1;

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();
            var logger = Substitute.For<ILogger>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);

            var result = await Functions.Run(request, profileName, "surge-list", logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(Fixtures.SurgeListProfile1, resultProfile);
        }

        [Fact]
        public async Task ShouldFailBecauseOfInvalidTemplate()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile1;

            var templateUrl = "https://template-url";
            var templateContent = "asdafsdafsadf";

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();
            var logger = Substitute.For<ILogger>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.Run(request, profileName, "clash", logger);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ShouldSuccessWithCustomClashTemplate()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile1;

            var templateUrl = "https://template-url";
            var templateContent = Fixtures.ClashTemplate1;


            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();
            var logger = Substitute.For<ILogger>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.Run(request, profileName, "clash", logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(Fixtures.CustomClashProfile1, resultProfile);
        }

        [Fact]
        public async Task ShouldSuccessWithCustomSurgeTemplate()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile1;

            var templateUrl = "https://template-url";
            var templateContent = Fixtures.SurgeTemplate1;

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();
            var logger = Substitute.For<ILogger>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.Run(request, profileName, "surge", logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(Fixtures.CustomSurgeProfile1, resultProfile);
        }

        [Fact]
        public async Task ShouldSuccessWithCustomTemplateName()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile1;

            var templateName = "Test1";
            var templateUrl = "https://template-url";
            var templateContent = Fixtures.SurgeTemplate1;

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();
            var logger = Substitute.For<ILogger>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateName } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            environmentManager.Get("Template" + templateName).Returns(templateUrl);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.Run(request, profileName, "surge", logger);
            Assert.IsType<FileContentResult>(result);

#pragma warning disable 4014
            downloader.Received().Download(templateUrl);
#pragma warning restore 4014
        }
    }
}