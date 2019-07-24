using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using Xunit;

namespace ProxyStation.UnitTests.ProfileParser
{
    public class GeneralParserTests
    {
        GeneralParser parser;

        public GeneralParserTests()
        {
            parser = new GeneralParser(new LoggerFactory().CreateLogger("test"));
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [Fact]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("General.conf")));

            Assert.Equal(ProxyType.ShadowsocksR, servers[0].Type);
            Assert.Equal("127.0.0.1", servers[0].Host);
            Assert.Equal(1234, servers[0].Port);
            Assert.Equal("aaabbb", servers[0].Password);
            Assert.Equal("测试中文", servers[0].Name);
            Assert.Equal("aes-128-cfb", (servers[0] as ShadowsocksRServer).Method);
            Assert.Equal("auth_aes128_md5", (servers[0] as ShadowsocksRServer).Protocol);
            Assert.Null((servers[0] as ShadowsocksRServer).ProtocolParameter);
            Assert.Equal("tls1.2_ticket_auth", (servers[0] as ShadowsocksRServer).Obfuscation);
            Assert.Equal("breakwa11.moe", (servers[0] as ShadowsocksRServer).ObfuscationParameter);
            Assert.False((servers[0] as ShadowsocksRServer).UDPOverTCP);
            Assert.Equal(0, (servers[0] as ShadowsocksRServer).UDPPort);

            Assert.Equal(ProxyType.ShadowsocksR, servers[1].Type);
            Assert.Equal("127.0.0.1", servers[1].Host);
            Assert.Equal(1234, servers[1].Port);
            Assert.Equal("aaabbb", servers[1].Password);
            Assert.Equal("aes-128-cfb", (servers[1] as ShadowsocksRServer).Method);
            Assert.Equal("auth_aes128_md5", (servers[1] as ShadowsocksRServer).Protocol);
            Assert.Null((servers[1] as ShadowsocksRServer).ProtocolParameter);
            Assert.Equal("tls1.2_ticket_auth", (servers[1] as ShadowsocksRServer).Obfuscation);
            Assert.Equal("breakwa11.moe", (servers[1] as ShadowsocksRServer).ObfuscationParameter);
            Assert.False((servers[1] as ShadowsocksRServer).UDPOverTCP);
            Assert.Equal(0, (servers[1] as ShadowsocksRServer).UDPPort);

            Assert.Equal(ProxyType.Shadowsocks, servers[2].Type);
            Assert.Equal("192.168.100.1", servers[2].Host);
            Assert.Equal(8888, servers[2].Port);
            Assert.Equal("test", servers[2].Password);
            Assert.Equal("Example1", servers[2].Name);
            Assert.Equal("aes-128-gcm", (servers[2] as ShadowsocksServer).Method);

            Assert.Equal(ProxyType.Shadowsocks, servers[3].Type);
            Assert.Equal("192.168.100.1", servers[3].Host);
            Assert.Equal(8888, servers[3].Port);
            Assert.Equal("passwd", servers[3].Password);
            Assert.Equal("Example2", servers[3].Name);
            Assert.Equal("rc4-md5", (servers[3] as ShadowsocksServer).Method);
            Assert.IsType<SimpleObfsPluginOptions>((servers[3] as ShadowsocksServer).PluginOptions);
            Assert.Equal("http", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);

            Assert.Equal(ProxyType.Shadowsocks, servers[4].Type);
            Assert.Equal("192.168.100.1", servers[4].Host);
            Assert.Equal(8888, servers[4].Port);
            Assert.Equal("test", servers[4].Password);
            Assert.Equal("Dummy profile name", servers[4].Name);
            Assert.Equal("bf-cfb", (servers[4] as ShadowsocksServer).Method);
        }
    }
}