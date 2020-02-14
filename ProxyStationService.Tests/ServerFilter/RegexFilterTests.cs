using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ServerFilter;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.Tests.ServerFilter
{
    public class RegexFilterTests
    {
        readonly RegexFilter filter;

        readonly Server[] servers;

        readonly ILogger logger;

        public RegexFilterTests(ITestOutputHelper output)
        {
            this.logger = output.BuildLogger();
            this.filter = new RegexFilter();
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
name: regex
mode: whitelist
pattern: bad.*?
            ";
            var yaml = new YamlStream();
            yaml.Load(new StringReader(optionsYaml));

            filter.LoadOptions(yaml.Documents[0].RootNode, this.logger);
            Assert.Equal(FilterMode.WhiteList, filter.Mode);

            var filtered = filter.Do(this.servers, this.logger);
            Assert.Equal(2, filtered.Length);
        }
    }
}