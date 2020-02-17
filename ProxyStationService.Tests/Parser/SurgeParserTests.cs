using System.IO;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using Xunit;
using Xunit.Abstractions;

namespace ProxyStation.Tests.Parser
{
    public class SurgeParserTests
    {
        readonly SurgeParser parser;

        public SurgeParserTests(ITestOutputHelper helper)
        {
            parser = new SurgeParser(helper.BuildLogger());
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [Fact]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("Surge.conf")));

            Assert.IsType<ShadowsocksServer>(servers[0]);
            Assert.Equal("12381293", servers[0].Host);
            Assert.Equal(123, servers[0].Port);
            Assert.Equal("1231341", ((ShadowsocksServer)servers[0]).Password);
            Assert.Equal("sadfasd", servers[0].Name);
            Assert.Equal("aes-128-gcm", ((ShadowsocksServer)servers[0]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[0]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.HTTP, (((ShadowsocksServer)servers[0]).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("2341324124", (((ShadowsocksServer)servers[0]).PluginOptions as SimpleObfsPluginOptions).Host);

            Assert.IsType<ShadowsocksServer>(servers[1]);
            Assert.Equal("hk5.edge.iplc.app", servers[1].Host);
            Assert.Equal(155, servers[1].Port);
            Assert.Equal("asdads", ((ShadowsocksServer)servers[1]).Password);
            Assert.Equal("rc4-md5", ((ShadowsocksServer)servers[1]).Method);
            Assert.True(((ShadowsocksServer)servers[1]).UDPRelay);

            Assert.IsType<ShadowsocksServer>(servers[2]);
            Assert.Equal("123.123.123.123", servers[2].Host);
            Assert.Equal(10086, servers[2].Port);
            Assert.Equal("gasdas", ((ShadowsocksServer)servers[2]).Password);
            Assert.Equal("🇭🇰 中国杭州 -> 香港 01 | IPLC", servers[2].Name);
            Assert.Equal("xchacha20-ietf-poly1305", ((ShadowsocksServer)servers[2]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[2]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.TLS, (((ShadowsocksServer)servers[2]).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("download.windowsupdate.com", (((ShadowsocksServer)servers[2]).PluginOptions as SimpleObfsPluginOptions).Host);
            Assert.True(((ShadowsocksServer)servers[2]).UDPRelay);

            Assert.IsType<ShadowsocksServer>(servers[3]);
            Assert.Equal("12381293", servers[3].Host);
            Assert.Equal(123, servers[3].Port);
            Assert.Equal("1231341", ((ShadowsocksServer)servers[3]).Password);
            Assert.Equal("sadfasd", servers[3].Name);
            Assert.Equal("aes-128-gcm", ((ShadowsocksServer)servers[3]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[3]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.HTTP, (((ShadowsocksServer)servers[3]).PluginOptions as SimpleObfsPluginOptions).Mode);
            Assert.Equal("2341324124", (((ShadowsocksServer)servers[3]).PluginOptions as SimpleObfsPluginOptions).Host);
        }
    }
}