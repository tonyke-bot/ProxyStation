using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public class SurfboardEncodeOptions : IEncodeOptions
    {
        public string ProfileURL { get; set; }
    }

    public class SurfboardParser : IProfileParser
    {

        private ILogger logger;

        public SurfboardParser(ILogger logger)
        {
            this.logger = logger;
        }

        public static Regex profileSectionRegex = new Regex(@"^\[([^\]]*?)\]$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        public static Regex proxyRegex = new Regex(@"^(.*?)\s*\=\s*(\w.*?)$",
                RegexOptions.Compiled);

        static string TrimPrefix(string str, string prefix) => str.StartsWith(prefix) ? str.Substring(prefix.Length) : str;

        public Server[] Parse(string profile)
        {
            throw new NotImplementedException();
        }

        public string Encode(Server[] servers, IEncodeOptions options)
        {
            var stringBuilder = new StringBuilder();

            if (options is SurgeEncodeOptions)
            {
                var surgeOptions = options as SurgeEncodeOptions;
                if (!String.IsNullOrEmpty(surgeOptions.ProfileURL))
                {
                    stringBuilder.AppendLine($"#!MANAGED-CONFIG {surgeOptions.ProfileURL} interval=43200");
                    stringBuilder.AppendLine();
                }
            }
            stringBuilder.AppendLine(ProfileSnippet.SurfboardCommon);

            stringBuilder.AppendLine("[Proxy]");
            stringBuilder.AppendLine(EncodeProxyList(servers));

            var proxies = servers.Select(s => s.Name).ToList();
            if (proxies.Count == 0) proxies.Add("DIRECT");
            var proxyNames = String.Join(", ", proxies);
            stringBuilder.AppendLine("[Proxy Group]");
            stringBuilder.AppendLine("Default = select, Proxy, DIRECT");
            stringBuilder.AppendLine($"Proxy = url-test, {proxyNames}, url=http://captive.apple.com, interval=600, tolerance=200");
            stringBuilder.AppendLine(ProfileSnippet.SurfboardRule);
            return stringBuilder.ToString();
        }

        public string Encode(Server[] servers) => Encode(servers, null);

        public string EncodeProxyList(Server[] servers)
        {
            var sb = new StringBuilder();
            foreach (var server in servers)
            {
                if (server.Type != ProxyType.Shadowsocks) continue;
                var ssServer = server as ShadowsocksServer;
                var line = $"{server.Name} = custom, {server.Host}, {server.Port}, {ssServer.Method}, {ssServer.Password}, https://dler.cloud/SSEncrypt.module";
                if (ssServer.PluginType == PluginType.SimpleObfs)
                {
                    var pluginOptions = ssServer.PluginOptions as SimpleObfsPluginOptions;
                    var obfsHost = String.IsNullOrEmpty(pluginOptions.Host) ? Constant.ObfsucationHost : pluginOptions.Host;
                    line += $", obfs={pluginOptions.Mode}, obfs-host={obfsHost}";
                }
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        public string ExtName()
        {
            return ".conf";
        }
    }
}