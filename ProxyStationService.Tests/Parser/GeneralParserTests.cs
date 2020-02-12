using System.IO;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser;
using Xunit;

namespace ProxyStation.Tests.Parser
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

            Assert.IsType<ShadowsocksRServer>(servers[0]);
            Assert.Equal("127.0.0.1", servers[0].Host);
            Assert.Equal(1234, servers[0].Port);
            Assert.Equal("aaabbb", ((ShadowsocksRServer)servers[0]).Password);
            Assert.Equal("测试中文", servers[0].Name);
            Assert.Equal("aes-128-cfb", ((ShadowsocksRServer)servers[0]).Method);
            Assert.Equal("auth_aes128_md5", ((ShadowsocksRServer)servers[0]).Protocol);
            Assert.Null(((ShadowsocksRServer)servers[0]).ProtocolParameter);
            Assert.Equal("tls1.2_ticket_auth", ((ShadowsocksRServer)servers[0]).Obfuscation);
            Assert.Equal("breakwa11.moe", ((ShadowsocksRServer)servers[0]).ObfuscationParameter);
            Assert.False(((ShadowsocksRServer)servers[0]).UDPOverTCP);
            Assert.Equal(0, ((ShadowsocksRServer)servers[0]).UDPPort);

            Assert.IsType<ShadowsocksRServer>(servers[1]);
            Assert.Equal("127.0.0.1", servers[1].Host);
            Assert.Equal(1234, servers[1].Port);
            Assert.Equal("aaabbb", ((ShadowsocksRServer)servers[1]).Password);
            Assert.Equal("aes-128-cfb", ((ShadowsocksRServer)servers[1]).Method);
            Assert.Equal("auth_aes128_md5", ((ShadowsocksRServer)servers[1]).Protocol);
            Assert.Null(((ShadowsocksRServer)servers[1]).ProtocolParameter);
            Assert.Equal("tls1.2_ticket_auth", ((ShadowsocksRServer)servers[1]).Obfuscation);
            Assert.Equal("breakwa11.moe", ((ShadowsocksRServer)servers[1]).ObfuscationParameter);
            Assert.False(((ShadowsocksRServer)servers[1]).UDPOverTCP);
            Assert.Equal(0, ((ShadowsocksRServer)servers[1]).UDPPort);

            Assert.IsType<ShadowsocksServer>(servers[2]);
            Assert.Equal("192.168.100.1", servers[2].Host);
            Assert.Equal(8888, servers[2].Port);
            Assert.Equal("test", ((ShadowsocksServer)servers[2]).Password);
            Assert.Equal("Example1", servers[2].Name);
            Assert.Equal("aes-128-gcm", ((ShadowsocksServer)servers[2]).Method);

            Assert.IsType<ShadowsocksServer>(servers[3]);
            Assert.Equal("192.168.100.1", servers[3].Host);
            Assert.Equal(8888, servers[3].Port);
            Assert.Equal("passwd", ((ShadowsocksServer)servers[3]).Password);
            Assert.Equal("Example2", servers[3].Name);
            Assert.Equal("rc4-md5", ((ShadowsocksServer)servers[3]).Method);
            Assert.IsType<SimpleObfsPluginOptions>(((ShadowsocksServer)servers[3]).PluginOptions);
            Assert.Equal(SimpleObfsPluginMode.HTTP, ((SimpleObfsPluginOptions)((ShadowsocksServer)servers[3]).PluginOptions).Mode);

            Assert.IsType<ShadowsocksServer>(servers[4]);
            Assert.Equal("192.168.100.1", servers[4].Host);
            Assert.Equal(8888, servers[4].Port);
            Assert.Equal("test", ((ShadowsocksServer)servers[4]).Password);
            Assert.Equal("Dummy profile name", servers[4].Name);
            Assert.Equal("bf-cfb", ((ShadowsocksServer)servers[4]).Method);
        }
    }
}