using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using ProxyStation.ServerFilter;
using YamlDotNet.RepresentationModel;

namespace ProxyStation.UnitTests.ServerFilter
{
    [TestClass]
    public class NameFilterTests
    {
        NameFilter filter;

        Server[] servers;

        public NameFilterTests()
        {
            filter = new NameFilter();
            servers = new Server[]{
                new ShadowsocksServer() { Name = "goodserver 1" },
                new ShadowsocksServer() { Name = "goodserver 2" },
                new ShadowsocksServer() { Name = "goodserver 3" },
                new ShadowsocksServer() { Name = "badserver 1" },
                new ShadowsocksServer() { Name = "badserver 2" },
            };
        }

        [TestMethod]
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

            filter.LoadOptions(yaml.Documents[0].RootNode);
            Assert.AreEqual(FilterMode.WhiteList, filter.Mode);
            Assert.AreEqual(NameFilterMatching.HasPrefix, filter.Matching);
            Assert.AreEqual("goodserver", filter.Keyword);

            var filtered = filter.Do(servers);
            Assert.AreEqual(3, filtered.Length);
        }

        [TestMethod]
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

            filter.LoadOptions(yaml.Documents[0].RootNode);
            Assert.AreEqual(FilterMode.BlackList, filter.Mode);
            Assert.AreEqual(NameFilterMatching.HasPrefix, filter.Matching);
            Assert.AreEqual("badserver", filter.Keyword);

            var filtered = filter.Do(servers);
            Assert.AreEqual(3, filtered.Length);
        }

        [TestMethod]
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

            filter.LoadOptions(yaml.Documents[0].RootNode);
            Assert.AreEqual(FilterMode.WhiteList, filter.Mode);
            Assert.AreEqual(NameFilterMatching.Contains, filter.Matching);
            Assert.AreEqual("server ", filter.Keyword);

            var filtered = filter.Do(servers);
            Assert.AreEqual(5, filtered.Length);
        }
    }
}