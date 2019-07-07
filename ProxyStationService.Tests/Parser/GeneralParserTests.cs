using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyStation.Model;
using ProxyStation.ProfileParser;

namespace ProxyStation.UnitTests.ProfileParser
{
    [TestClass]
    public class GeneralParserTests
    {
        GeneralParser parser;

        public GeneralParserTests()
        {
            parser = new GeneralParser(new LoggerFactory().CreateLogger("test"));
        }

        private string GetFixturePath(string relativePath) => Path.Combine("../../..", "./Parser/Fixtures", relativePath);

        [TestMethod]
        public void ShouldSuccess()
        {
            var servers = parser.Parse(File.ReadAllText(GetFixturePath("General.conf")));

            Assert.AreEqual(ProxyType.ShadowsocksR, servers[0].Type);
            Assert.AreEqual("127.0.0.1", servers[0].Host);
            Assert.AreEqual(1234, servers[0].Port);
            Assert.AreEqual("aaabbb", servers[0].Password);
            Assert.AreEqual("测试中文", servers[0].Name);
            Assert.AreEqual("aes-128-cfb", (servers[0] as ShadowsocksRServer).Method);
            Assert.AreEqual("auth_aes128_md5", (servers[0] as ShadowsocksRServer).Protocol);
            Assert.AreEqual(null, (servers[0] as ShadowsocksRServer).ProtocolParameter);
            Assert.AreEqual("tls1.2_ticket_auth", (servers[0] as ShadowsocksRServer).Obfuscation);
            Assert.AreEqual("breakwa11.moe", (servers[0] as ShadowsocksRServer).ObfuscationParameter);
            Assert.AreEqual(false, (servers[0] as ShadowsocksRServer).UDPOverTCP);
            Assert.AreEqual(0, (servers[0] as ShadowsocksRServer).UDPPort);

            Assert.AreEqual(ProxyType.ShadowsocksR, servers[1].Type);
            Assert.AreEqual("127.0.0.1", servers[1].Host);
            Assert.AreEqual(1234, servers[1].Port);
            Assert.AreEqual("aaabbb", servers[1].Password);
            Assert.AreEqual("aes-128-cfb", (servers[1] as ShadowsocksRServer).Method);
            Assert.AreEqual("auth_aes128_md5", (servers[1] as ShadowsocksRServer).Protocol);
            Assert.AreEqual(null, (servers[1] as ShadowsocksRServer).ProtocolParameter);
            Assert.AreEqual("tls1.2_ticket_auth", (servers[1] as ShadowsocksRServer).Obfuscation);
            Assert.AreEqual("breakwa11.moe", (servers[1] as ShadowsocksRServer).ObfuscationParameter);
            Assert.AreEqual(false, (servers[1] as ShadowsocksRServer).UDPOverTCP);
            Assert.AreEqual(0, (servers[1] as ShadowsocksRServer).UDPPort);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[2].Type);
            Assert.AreEqual("192.168.100.1", servers[2].Host);
            Assert.AreEqual(8888, servers[2].Port);
            Assert.AreEqual("test", servers[2].Password);
            Assert.AreEqual("Example1", servers[2].Name);
            Assert.AreEqual("aes-128-gcm", (servers[2] as ShadowsocksServer).Method);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[3].Type);
            Assert.AreEqual("192.168.100.1", servers[3].Host);
            Assert.AreEqual(8888, servers[3].Port);
            Assert.AreEqual("passwd", servers[3].Password);
            Assert.AreEqual("Example2", servers[3].Name);
            Assert.AreEqual("rc4-md5", (servers[3] as ShadowsocksServer).Method);
            Assert.IsInstanceOfType((servers[3] as ShadowsocksServer).PluginOptions, typeof(SimpleObfsPluginOptions));
            Assert.AreEqual("http", ((servers[3] as ShadowsocksServer).PluginOptions as SimpleObfsPluginOptions).Mode);

            Assert.AreEqual(ProxyType.Shadowsocks, servers[4].Type);
            Assert.AreEqual("192.168.100.1", servers[4].Host);
            Assert.AreEqual(8888, servers[4].Port);
            Assert.AreEqual("test", servers[4].Password);
            Assert.AreEqual("Dummy profile name", servers[4].Name);
            Assert.AreEqual("bf-cfb", (servers[4] as ShadowsocksServer).Method);
        }
    }
}