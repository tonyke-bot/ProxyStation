using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyStation.Model;
using ProxyStation.ProfileParser;

namespace ProxyStation.UnitTests.ProfileParser
{
    [TestClass]
    public class SurgeParserTests
    {
        SurgeParser parser;

        public SurgeParserTests()
        {
            parser = new SurgeParser(new LoggerFactory().CreateLogger("test"));
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [TestMethod]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("Surge.conf")));

            Assert.AreEqual(ProxyType.Shadowsocks, servers[0].Type);
            Assert.AreEqual("12381293", servers[0].Host);
            Assert.AreEqual(123, servers[0].Port);
            Assert.AreEqual("1231341", servers[0].Password);
            Assert.AreEqual("sadfasd=", servers[0].Name);
            Assert.AreEqual("aes-128-gcm", (servers[0] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[0] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("http", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.AreEqual("2341324124", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[1].Type);
            Assert.AreEqual("hk5.edge.iplc.app", servers[1].Host);
            Assert.AreEqual(155, servers[1].Port);
            Assert.AreEqual("asdads", servers[1].Password);
            Assert.AreEqual("rc4-md5", (servers[1] as ShadowsocksServer).Method);
            Assert.AreEqual(true, (servers[1] as ShadowsocksServer).UDPRelay);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[2].Type);
            Assert.AreEqual("123.123.123.123", servers[2].Host);
            Assert.AreEqual(10086, servers[2].Port);
            Assert.AreEqual("gasdas", servers[2].Password);
            Assert.AreEqual("🇭🇰 中国杭州 -> 香港 01 | IPLC", servers[2].Name);
            Assert.AreEqual("xchacha20-ietf-poly1305", (servers[2] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[2] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("tls", ((servers[2] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.AreEqual("download.windowsupdate.com", ((servers[2] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.AreEqual(true, (servers[2] as ShadowsocksServer).UDPRelay);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[3].Type);
            Assert.AreEqual("12381293", servers[3].Host);
            Assert.AreEqual(123, servers[3].Port);
            Assert.AreEqual("1231341", servers[3].Password);
            Assert.AreEqual("sadfasd", servers[3].Name);
            Assert.AreEqual("aes-128-gcm", (servers[3] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[3] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("http", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.AreEqual("2341324124", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
        }
    }
}