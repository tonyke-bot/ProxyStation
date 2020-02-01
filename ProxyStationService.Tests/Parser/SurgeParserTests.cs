using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using Xunit;

namespace ProxyStation.Tests.Parser
{
    public class SurgeParserTests
    {
        SurgeParser parser;

        public SurgeParserTests()
        {
            parser = new SurgeParser(new LoggerFactory().CreateLogger("test"));
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [Fact]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("Surge.conf")));

            Assert.Equal(ProxyType.Shadowsocks, servers[0].Type);
            Assert.Equal("12381293", servers[0].Host);
            Assert.Equal(123, servers[0].Port);
            Assert.Equal("1231341", servers[0].Password);
            Assert.Equal("sadfasd=", servers[0].Name);
            Assert.Equal("aes-128-gcm", (servers[0] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[0] as ShadowsocksServer).PluginOptions);
            Assert.Equal("http", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("2341324124", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);

            Assert.Equal(ProxyType.Shadowsocks, servers[1].Type);
            Assert.Equal("hk5.edge.iplc.app", servers[1].Host);
            Assert.Equal(155, servers[1].Port);
            Assert.Equal("asdads", servers[1].Password);
            Assert.Equal("rc4-md5", (servers[1] as ShadowsocksServer).Method);
            Assert.True((servers[1] as ShadowsocksServer).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[2].Type);
            Assert.Equal("123.123.123.123", servers[2].Host);
            Assert.Equal(10086, servers[2].Port);
            Assert.Equal("gasdas", servers[2].Password);
            Assert.Equal("🇭🇰 中国杭州 -> 香港 01 | IPLC", servers[2].Name);
            Assert.Equal("xchacha20-ietf-poly1305", (servers[2] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[2] as ShadowsocksServer).PluginOptions);
            Assert.Equal("tls", ((servers[2] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("download.windowsupdate.com", ((servers[2] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.True((servers[2] as ShadowsocksServer).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[3].Type);
            Assert.Equal("12381293", servers[3].Host);
            Assert.Equal(123, servers[3].Port);
            Assert.Equal("1231341", servers[3].Password);
            Assert.Equal("sadfasd", servers[3].Name);
            Assert.Equal("aes-128-gcm", (servers[3] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[3] as ShadowsocksServer).PluginOptions);
            Assert.Equal("http", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("2341324124", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
        }
    }
}