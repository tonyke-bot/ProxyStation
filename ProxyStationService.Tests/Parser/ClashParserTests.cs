using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyStation.Model;
using ProxyStation.ProfileParser;

namespace ProxyStation.UnitTests.ProfileParser
{
    [TestClass]
    public class ClashParserTests
    {
        ClashParser parser;

        public ClashParserTests()
        {
            parser = new ClashParser(new LoggerFactory().CreateLogger("test"));
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [TestMethod]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("Clash.yaml")));

            Assert.AreEqual(servers.Length, 4);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[0].Type);
            Assert.AreEqual("gg.gol.proxy", servers[0].Host);
            Assert.AreEqual(152, servers[0].Port);
            Assert.AreEqual("asdbasv", servers[0].Password);
            Assert.AreEqual("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[0].Name);
            Assert.AreEqual("chacha20-ietf-poly1305", (servers[0] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[0] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("tls", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.AreEqual("adfadfads", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.IsTrue((servers[0] as ShadowsocksServer).UDPRelay);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[1].Type);
            Assert.AreEqual("gg.gol.proxy", servers[1].Host);
            Assert.AreEqual(152, servers[1].Port);
            Assert.AreEqual("asdbasv", servers[1].Password);
            Assert.AreEqual("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[1].Name);
            Assert.AreEqual("chacha20-ietf-poly1305", (servers[1] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[1] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("tls", ((servers[1] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.AreEqual("asdfasdf", ((servers[1] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.IsFalse((servers[1] as ShadowsocksServer).UDPRelay);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[2].Type);
            Assert.AreEqual("server", servers[2].Host);
            Assert.AreEqual(443, servers[2].Port);
            Assert.AreEqual("password", servers[2].Password);
            Assert.AreEqual("ss3", servers[2].Name);
            Assert.AreEqual("AEAD_CHACHA20_POLY1305", (servers[2] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[2] as ShadowsocksServer).PluginOptions, typeof(V2RayPluginOptions));
            Assert.AreEqual("websocket", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Mode);
            Assert.AreEqual("bing.com", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Host);
            Assert.AreEqual("/", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Path);
            Assert.IsTrue(((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).SkipCertVerification);
            Assert.IsTrue(((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).EnableTLS);
            Assert.AreEqual(2, ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers.Count);
            Assert.AreEqual("value", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers["custom"]);
            Assert.AreEqual("bar", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers["foo"]);
            Assert.IsFalse((servers[2] as ShadowsocksServer).UDPRelay);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[3].Type);
            Assert.AreEqual("server", servers[3].Host);
            Assert.AreEqual(443, servers[3].Port);
            Assert.AreEqual("password", servers[3].Password);
            Assert.AreEqual("ss5", servers[3].Name);
            Assert.AreEqual("AEAD_CHACHA20_POLY1305", (servers[3] as ShadowsocksServer).Method);
            Assert.AreEqual(PluginType.None, (servers[3] as ShadowsocksServer).PluginType);
            Assert.IsNull((servers[3] as ShadowsocksServer).PluginOptions);
        }
    }
}