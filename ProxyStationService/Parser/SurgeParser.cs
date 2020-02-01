using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser.Template;

namespace ProxyStation.ProfileParser
{
    public class SurgeEncodeOptions : EncodeOptions
    {
        public string ProfileURL { get; set; }
    }

    public class SurgeParser : IProfileParser
    {

        private ILogger logger;

        public SurgeParser(ILogger logger)
        {
            this.logger = logger;
        }

        public static Regex profileSectionRegex = new Regex(@"^\[([^\]]*?)\]$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        public static Regex proxyRegex = new Regex(@"^(.*?)\s*\=\s*(\w.*?)$",
                RegexOptions.Compiled);

        static string TrimPrefix(string str, string prefix) => str.StartsWith(prefix) ? str.Substring(prefix.Length) : str;

        public bool ValidateTemplate(string template)
        {
            if (String.IsNullOrEmpty(template))
            {
                return true;
            }

            return template.Contains(Surge.ServerListPlaceholder) && template.Contains(Surge.ServerNamesPlaceholder);
        }

        public Server[] ParseProxyList(string profile)
        {
            var servers = new List<Server>();
            var plainProxies = profile.Trim().Split("\n");
            foreach (var proxy in plainProxies)
            {
                var trimed = proxy.Trim();
                if (trimed.StartsWith("#")) continue;

                var match = proxyRegex.Match(proxy);
                if (!match.Success) continue;

                var server = new ShadowsocksServer();
                server.Name = match.Groups[1].Value;

                var serverInfos = match.Groups[2].Value.Trim().Split(",");
                switch (serverInfos[0].Trim().ToLower())
                {
                    case "ss":
                    case "custom":
                        break;
                    default:
                        continue;
                }

                int port = 0;
                if (Int32.TryParse((serverInfos[2] ?? ""), out port)) server.Port = port;
                server.Host = (serverInfos[1] ?? "").Trim();
                server.Method = TrimPrefix((serverInfos[3] ?? "").Trim(), "encrypt-method=");
                server.Password = TrimPrefix((serverInfos[4] ?? "").Trim(), "password=");

                var pluginOptions = new SimpleObfsPluginOptions();
                for (var i = 5; i < serverInfos.Length; i++)
                {
                    var trimedInfo = serverInfos[i].Trim();
                    if (String.IsNullOrEmpty(pluginOptions.Mode) && trimedInfo.StartsWith("obfs="))
                        pluginOptions.Mode = trimedInfo.Substring("obfs=".Length).TrimStart();

                    else if (String.IsNullOrEmpty(pluginOptions.Host) && trimedInfo.StartsWith("obfs-host="))
                        pluginOptions.Host = trimedInfo.Substring("obfs-host=".Length).TrimStart();

                    else if (!server.UDPRelay && trimedInfo.StartsWith("udp-relay="))
                        server.UDPRelay = trimedInfo.Substring("udp-relay=".Length).TrimStart() == "true";

                    // module is no more needed for newer version of surge
                    else if (trimedInfo.StartsWith("http://") || trimedInfo.StartsWith("https://"))
                        continue;

                    else
                        logger.LogWarning($"Unsupported surge proxy parameter found: {trimedInfo}");
                }

                if (!String.IsNullOrEmpty(pluginOptions.Mode))
                {
                    server.PluginType = PluginType.SimpleObfs;
                    server.PluginOptions = pluginOptions;
                }

                servers.Add(server);
            }

            return servers.ToArray();
        }

        public Server[] Parse(string profile)
        {
            var servers = new List<Server>();

            var proxyStartPos = -1;
            var proxyEndPos = -1;
            foreach (Match match in profileSectionRegex.Matches(profile))
            {
                if (match.Groups[1].Value.ToLower() == "proxy")
                {
                    proxyStartPos = match.Index + match.Length;
                    continue;
                }

                if (proxyStartPos != -1)
                {
                    proxyEndPos = match.Index - 1;
                    break;
                }
            }
            if (proxyStartPos == -1)
            {
                return servers.ToArray();
            }

            var plainProxies = profile.Substring(proxyStartPos, proxyEndPos - proxyStartPos + 1).Trim();
            return ParseProxyList(plainProxies);
        }

        public string Encode(Server[] servers, EncodeOptions options)
        {
            if (!this.ValidateTemplate(options.Template))
            {
                throw new InvalidTemplateException();
            }

            var template = String.IsNullOrEmpty(options.Template) ? Surge.Template : options.Template;

            if (options is SurgeEncodeOptions)
            {
                var surgeOptions = options as SurgeEncodeOptions;
                if (!String.IsNullOrEmpty(surgeOptions.ProfileURL))
                {
                    template = $"#!MANAGED-CONFIG {surgeOptions.ProfileURL} interval=43200\n" + template;
                }
            }

            return template.Replace(Surge.ServerListPlaceholder, EncodeProxyList(servers))
                .Replace(Surge.ServerNamesPlaceholder, String.Join(", ", servers.Select(s => s.Name)));
        }

        public string EncodeProxyList(Server[] servers)
        {
            var sb = new StringBuilder();
            foreach (var server in servers)
            {
                if (server.Type != ProxyType.Shadowsocks) continue;
                var ssServer = server as ShadowsocksServer;
                var line = $"{server.Name} = ss, {server.Host}, {server.Port}, encrypt-method={ssServer.Method}, password={ssServer.Password}";
                if (ssServer.PluginType == PluginType.SimpleObfs)
                {
                    var pluginOptions = ssServer.PluginOptions as SimpleObfsPluginOptions;
                    var obfsHost = String.IsNullOrEmpty(pluginOptions.Host) ? Constant.ObfsucationHost : pluginOptions.Host;
                    line += $", obfs={pluginOptions.Mode}, obfs-host={obfsHost}";
                }
                line += ", udp-relay=true";
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        public string ExtName() => "";
    }
}