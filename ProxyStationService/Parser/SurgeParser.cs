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
            if (string.IsNullOrEmpty(template))
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

                var serverInfos = match.Groups[2].Value.Trim().Split(",");
                switch (serverInfos[0].Trim().ToLower())
                {
                    case "ss":
                    case "custom":
                        break;
                    default:
                        continue;
                }

                var server = new ShadowsocksServer
                {
                    Name = match.Groups[1].Value,
                    Host = (serverInfos[1] ?? "").Trim(),
                    Method = TrimPrefix((serverInfos[3] ?? "").Trim(), "encrypt-method="),
                    Password = TrimPrefix((serverInfos[4] ?? "").Trim(), "password=")
                };
                if (int.TryParse(serverInfos[2] ?? "", out int port))
                {
                    server.Port = port;
                }

                var pluginOptions = new SimpleObfsPluginOptions();

                // Parse plugin
                for (var i = 5; i < serverInfos.Length; i++)
                {
                    var trimedInfo = serverInfos[i].Trim();
                    if (trimedInfo.StartsWith("obfs="))
                    {
                        if (SimpleObfsPluginOptions.TryParseMode(trimedInfo.Substring("obfs=".Length).Trim(), out SimpleObfsPluginMode mode))
                        {
                            server.PluginOptions = pluginOptions;
                            pluginOptions.Mode = mode;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (string.IsNullOrEmpty(pluginOptions.Host) && trimedInfo.StartsWith("obfs-host="))
                    {
                        pluginOptions.Host = trimedInfo.Substring("obfs-host=".Length).TrimStart();
                    }
                    else if (!server.UDPRelay && trimedInfo.StartsWith("udp-relay="))
                    {
                        server.UDPRelay = trimedInfo.Substring("udp-relay=".Length).TrimStart() == "true";
                    }
                    else if (trimedInfo.StartsWith("http://") || trimedInfo.StartsWith("https://"))
                    {
                        // module is no more needed for newer version of surge
                        continue;
                    }
                    else
                    {
                        logger.LogWarning($"Unsupported surge proxy parameter found: {trimedInfo}");
                    }
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

            var template = string.IsNullOrEmpty(options.Template) ? Surge.Template : options.Template;

            if (options is SurgeEncodeOptions)
            {
                var surgeOptions = options as SurgeEncodeOptions;
                if (!string.IsNullOrEmpty(surgeOptions.ProfileURL))
                {
                    template = $"#!MANAGED-CONFIG {surgeOptions.ProfileURL} interval=43200\n" + template;
                }
            }

            return template
                .Replace(Surge.ServerListPlaceholder, EncodeProxyList(servers))
                .Replace(Surge.ServerNamesPlaceholder, string.Join(", ", servers.Select(s => s.Name)));
        }

        public string EncodeProxyList(Server[] servers)
        {
            var sb = new StringBuilder();
            foreach (var server in servers)
            {
                if (server.Type != ProxyType.Shadowsocks) continue;
                var ssServer = server as ShadowsocksServer;
                sb.Append($"{server.Name} = ss, {server.Host}, {server.Port}, encrypt-method={ssServer.Method}, password={ssServer.Password}");

                if (ssServer.PluginOptions is SimpleObfsPluginOptions options)
                {
                    var obfsHost = string.IsNullOrEmpty(options.Host) ? Constant.ObfsucationHost : options.Host;
                    sb.Append($", obfs={SurgeParser.FormatSimpleObfsPluginMode(options.Mode)}, obfs-host={obfsHost}");
                }
                sb.Append(", udp-relay=true");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ExtName() => "";

        static string FormatSimpleObfsPluginMode(SimpleObfsPluginMode mode)
        {
            return mode switch
            {
                SimpleObfsPluginMode.HTTP => "http",
                SimpleObfsPluginMode.TLS => "tls",
                _ => throw new NotImplementedException(),
            };
        }
    }
}