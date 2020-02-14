using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ServerFilter;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.Tests.ServerFilter
{
    public class NameFilterTests
    {
        NameFilter filter;

        Server[] servers;

        ILogger logger;

        public NameFilterTests(ITestOutputHelper output)
        {
            this.logger = output.BuildLogger();
            this.filter = new NameFilter();
            this.servers = new Server[]{
                new ShadowsocksServer() { Name = "goodserver 1" },
                new ShadowsocksServer() { Name = "goodserver 2" },
                new ShadowsocksServer() { Name = "goodserver 3" },
                new ShadowsocksServer() { Name = "badserver 1" },
                new ShadowsocksServer() { Name = "badserver 2" },
            };
        }

        [Fact]
        public void ShouldSuccessWithWhitelistMode()
        {
            var optionsYaml = @"
name: name
mode: whitelist
keyword: goodserver
matching: prefix
            ";
            var yaml = new YamlStream();
            yaml.Load(new StringReader(optionsYaml));

            filter.LoadOptions(yaml.Documents[0].RootNode, this.logger);
            Assert.Equal(FilterMode.WhiteList, filter.Mode);
            Assert.Equal(NameFilterMatching.HasPrefix, filter.Matching);
            Assert.Equal("goodserver", filter.Keyword);

            var filtered = filter.Do(servers, this.logger);
            Assert.Equal(3, filtered.Length);
        }

        [Fact]
        public void ShouldSuccessWithBlacklistMode()
        {
            var optionsYaml = @"
name: name
mode: blacklist
keyword: badserver
matching: prefix
            ";
            var yaml = new YamlStream();
            yaml.Load(new StringReader(optionsYaml));

            filter.LoadOptions(yaml.Documents[0].RootNode, this.logger);
            Assert.Equal(FilterMode.BlackList, filter.Mode);
            Assert.Equal(NameFilterMatching.HasPrefix, filter.Matching);
            Assert.Equal("badserver", filter.Keyword);

            var filtered = filter.Do(servers, this.logger);
            Assert.Equal(3, filtered.Length);
        }

        [Fact]
        public void ShouldSuccessWithContainsMode()
        {
            var optionsYaml = @"
name: name
mode: whitelist
keyword: 'server '
matching: contains
            ";
            var yaml = new YamlStream();
            yaml.Load(new StringReader(optionsYaml));

            filter.LoadOptions(yaml.Documents[0].RootNode, this.logger);
            Assert.Equal(FilterMode.WhiteList, filter.Mode);
            Assert.Equal(NameFilterMatching.Contains, filter.Matching);
            Assert.Equal("server ", filter.Keyword);

            var filtered = filter.Do(servers, this.logger);
            Assert.Equal(5, filtered.Length);
        }
    }
}