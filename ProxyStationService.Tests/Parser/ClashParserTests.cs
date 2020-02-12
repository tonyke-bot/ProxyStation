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
            Assert.Equal("asdbasv", ((ShadowsocksServer)servers[0]).Password);
            Assert.Equal("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[0].Name);
            Assert.Equal("chacha20-ietf-poly1305", ((ShadowsocksServer)servers[0]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[0]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.TLS, (((ShadowsocksServer)servers[0]).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("adfadfads", (((ShadowsocksServer)servers[0]).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.True(((ShadowsocksServer)servers[0]).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[1].Type);
            Assert.Equal("gg.gol.proxy", servers[1].Host);
            Assert.Equal(152, servers[1].Port);
            Assert.Equal("asdbasv", ((ShadowsocksServer)servers[1]).Password);
            Assert.Equal("🇺🇸 中国上海 -> 美国 01 | IPLC", servers[1].Name);
            Assert.Equal("chacha20-ietf-poly1305", ((ShadowsocksServer)servers[1]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[1]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.TLS, (((ShadowsocksServer)servers[1]).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("asdfasdf", (((ShadowsocksServer)servers[1]).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.False(((ShadowsocksServer)servers[1]).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[2].Type);
            Assert.Equal("server", servers[2].Host);
            Assert.Equal(443, servers[2].Port);
            Assert.Equal("password", ((ShadowsocksServer)servers[2]).Password);
            Assert.Equal("ss3", servers[2].Name);
            Assert.Equal("AEAD_CHACHA20_POLY1305", ((ShadowsocksServer)servers[2]).Method);
            Assert.IsType<V2RayPluginOptions>(((ShadowsocksServer)servers[2]).PluginOptions);
            Assert.Equal(V2RayPluginMode.WebSocket, (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Mode);
            Assert.Equal("bing.com", (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Host);
            Assert.Equal("/", (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Path);
            Assert.True((((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).SkipCertVerification);
            Assert.True((((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).EnableTLS);
            Assert.Equal(2, (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Headers.Count);
            Assert.Equal("value", (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Headers["custom"]);
            Assert.Equal("bar", (((ShadowsocksServer)servers[2]).PluginOptions as V2RayPluginOptions).Headers["foo"]);
            Assert.False(((ShadowsocksServer)servers[2]).UDPRelay);

            Assert.Equal(ProxyType.Shadowsocks, servers[3].Type);
            Assert.Equal("server", servers[3].Host);
            Assert.Equal(443, servers[3].Port);
            Assert.Equal("password", ((ShadowsocksServer)servers[3]).Password);
            Assert.Equal("ss5", servers[3].Name);
            Assert.Equal("AEAD_CHACHA20_POLY1305", ((ShadowsocksServer)servers[3]).Method);
            Assert.Null(((ShadowsocksServer)servers[3]).PluginOptions);
        }
    }
}