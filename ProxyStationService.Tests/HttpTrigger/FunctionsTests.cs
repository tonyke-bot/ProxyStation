using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using ProxyStation.HttpTrigger;
using ProxyStation.Util;
using Xunit;
using Xunit.Abstractions;

namespace ProxyStation.Tests.HttpTrigger
{
    public class FunctionsTests
    {
        private readonly ILogger logger;

        public FunctionsTests(ITestOutputHelper output)
        {
            this.logger = output.BuildLogger();
        }

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

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);

            var result = await Functions.GetTrain(request, profileName, "surge-list", this.logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(Fixtures.SurgeListProfile1, resultProfile);
        }

        [Fact]
        public async Task ShouldSuccessWithOriginal()
        {
            var profileUrl = "test";
            var profileName = "Test";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\", \"allowDirectAccess\": true}}";
            var profileContent = Fixtures.SurgeProfile1;

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);

            var result = await Functions.GetTrain(request, profileName, "original", this.logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(profileContent, resultProfile);
        }

        [Fact]
        public async Task ShouldSuccessWithAliasType()
        {
            var profileUrl = "test";
            var profileName = "TestProfile";
            var profileConfig = $"{{\"source\": \"{profileUrl}\", \"type\": \"surge\", \"name\": \"{profileName}\"}}";
            var profileContent = Fixtures.SurgeProfile2;

            var profileName1 = "TestProfileAlias1";
            var profileConfig1 = $"{{\"source\": \"{profileName}\", \"type\": \"alias\", \"name\": \"{profileName1}\", \"filters\": [{{\"name\": \"name\", \"mode\": \"whitelist\", \"keyword\": \"香港\"}}]}}";

            var profileName2 = "TestProfileAlias2";
            var profileConfig2 = $"{{\"source\": \"{profileName1}\", \"type\": \"alias\", \"name\": \"{profileName2}\", \"filters\": [{{\"name\": \"name\", \"mode\": \"whitelist\", \"keyword\": \"中继\"}}]}}";

            var profileName3 = "TestProfileAlias3";
            var profileConfig3 = $"{{\"source\": \"{profileName2}\", \"type\": \"alias\", \"name\": \"{profileName3}\", \"filters\": [{{\"name\": \"name\", \"mode\": \"whitelist\", \"keyword\": \"高级\"}}]}}";

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            environmentManager.Get(profileName).Returns(profileConfig);
            environmentManager.Get(profileName1).Returns(profileConfig1);
            environmentManager.Get(profileName2).Returns(profileConfig2);
            environmentManager.Get(profileName3).Returns(profileConfig3);
            downloader.Download(profileUrl).Returns(profileContent);

            var result = await Functions.GetTrain(request, "test-profile-alias-3", "surge-list", this.logger);
            Assert.IsType<FileContentResult>(result);

            var resultProfile = Encoding.UTF8.GetString((result as FileContentResult).FileContents);
            Assert.Equal(Fixtures.SurgeListProfile2, resultProfile);
        }

        [Fact]
        public async Task ShouldReturn403IfCircularAliasReferenceIsDetected()
        {
            var profileName1 = "TestProfileAlias1";
            var profileName2 = "TestProfileAlias2";

            var profileConfig1 = $"{{\"source\": \"{profileName2}\", \"type\": \"alias\", \"name\": \"{profileName1}\"}}";
            var profileConfig2 = $"{{\"source\": \"{profileName1}\", \"type\": \"alias\", \"name\": \"{profileName2}\"}}";

            var downloader = Substitute.For<IDownloader>();
            var environmentManager = Substitute.For<IEnvironmentManager>();
            var request = Substitute.For<HttpRequest>();

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            environmentManager.Get(profileName1).Returns(profileConfig1);
            environmentManager.Get(profileName2).Returns(profileConfig2);

            var result = await Functions.GetTrain(request, "test-profile-alias-1", "surge-list", this.logger);
            Assert.IsType<ForbidResult>(result);
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

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.GetTrain(request, profileName, "clash", this.logger);
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

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.GetTrain(request, profileName, "clash", this.logger);
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

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateUrl } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.GetTrain(request, profileName, "surge", this.logger);
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

            Functions.EnvironmentManager = environmentManager;
            Functions.Downloader = downloader;

            request.Query.Returns(new QueryCollection(new Dictionary<String, StringValues>() { { "template", templateName } }));
            environmentManager.Get(profileName).Returns(profileConfig);
            environmentManager.Get("Template" + templateName).Returns(templateUrl);
            downloader.Download(profileUrl).Returns(profileContent);
            downloader.Download(templateUrl).Returns(templateContent);

            var result = await Functions.GetTrain(request, profileName, "surge", this.logger);
            Assert.IsType<FileContentResult>(result);

#pragma warning disable 4014
            downloader.Received().Download(templateUrl);
#pragma warning restore 4014
        }
    }
}