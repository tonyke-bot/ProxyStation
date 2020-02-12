using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using Xunit;
using Xunit.Abstractions;

namespace ProxyStation.Tests.Parser
{
    public class ClashParserTests
    {
        readonly ClashParser parser;

        public ClashParserTests(ITestOutputHelper output)
        {
            parser = new ClashParser(output.BuildLogger());
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [Fact]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("Clash.yaml")));

            Assert.Equal(4, servers.Length);

            Assert.Equal(ProxyType.Shadowsocks, servers[0].Type);
            Assert.Equal("gg.gol.proxy", servers[0].Host);
            Assert.Equal(152, servers[0].Port);
            Assert.Equal("asdbasv", servers[0].Password);
            Assert.Equal("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[0].Name);
            Assert.Equal("chacha20-ietf-poly1305", (servers[0] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[0] as ShadowsocksServer).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.TLS, ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("adfadfads", ((servers[0] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.True((servers[0] as ShadowsocksServer).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[1].Type);
            Assert.Equal("gg.gol.proxy", servers[1].Host);
            Assert.Equal(152, servers[1].Port);
            Assert.Equal("asdbasv", servers[1].Password);
            Assert.Equal("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[1].Name);
            Assert.Equal("chacha20-ietf-poly1305", (servers[1] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[1] as ShadowsocksServer).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.TLS, ((servers[1] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("asdfasdf", ((servers[1] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.False((servers[1] as ShadowsocksServer).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[2].Type);
            Assert.Equal("server", servers[2].Host);
            Assert.Equal(443, servers[2].Port);
            Assert.Equal("password", servers[2].Password);
            Assert.Equal("ss3", servers[2].Name);
            Assert.Equal("AEAD_CHACHA20_POLY1305", (servers[2] as ShadowsocksServer).Method);
            Assert.IsType<V2RayPluginOptions>((servers[2] as ShadowsocksServer).PluginOptions);
            Assert.Equal(V2RayPluginMode.WebSocket, ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Mode);
            Assert.Equal("bing.com", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Host);
            Assert.Equal("/", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Path);
            Assert.True(((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).SkipCertVerification);
            Assert.True(((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).EnableTLS);
            Assert.Equal(2, ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers.Count);
            Assert.Equal("value", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers["custom"]);
            Assert.Equal("bar", ((servers[2] as ShadowsocksServer).PluginOptions as V2RayPluginOptions).Headers["foo"]);
            Assert.False((servers[2] as ShadowsocksServer).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[3].Type);
            Assert.Equal("server", servers[3].Host);
            Assert.Equal(443, servers[3].Port);
            Assert.Equal("password", servers[3].Password);
            Assert.Equal("ss5", servers[3].Name);
            Assert.Equal("AEAD_CHACHA20_POLY1305", (servers[3] as ShadowsocksServer).Method);
            Assert.Null((servers[3] as ShadowsocksServer).PluginOptions);
        }
    }
}