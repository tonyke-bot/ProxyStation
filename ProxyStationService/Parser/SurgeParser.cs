using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser.Template;
using ProxyStation.Util;

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

        public bool ValidateTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return true;
            }

            return template.Contains(Surge.ServerListPlaceholder) && template.Contains(Surge.ServerNamesPlaceholder);
        }

        public Server ParseSnellServer(string[] properties)
        {
            // snell, [SERVER ADDRESS], [GENERATED PORT], psk=[GENERATED PSK], obfs=http
            if (properties.Length < 3)
            {
                this.logger.LogError("Invaild surge shadowsocks proxy: " + string.Join(", ", properties));
                return null;
            }

            if (!int.TryParse(properties[2].Trim(), out int port))
            {
                this.logger.LogError("Invalid snell port: ", properties[2].Trim());
                return null;
            };


            var server = new SnellServer()
            {
                Host = properties[1].Trim(),
                Port = port,
            };

            var keyValues = properties
                .Skip(3)
                .Select(p => p.Trim().Split("=", 2))
                .Where(p => p.Length == 2)
                .Select(p => new string[] { p[0].TrimEnd(), p[1].TrimStart() });

            string obfs = null, obfsHost = null;
            foreach (var keyValue in keyValues)
            {
                var key = keyValue[0];
                if (key == "psk" || key == "password")
                {
                    server.Password = keyValue[1];
                }
                else if (key == "obfs")
                {
                    obfs = keyValue[1];
                }
                else if (key == "obfs-host")
                {
                    obfsHost = keyValue[1];
                }
                else if (key == "tfo")
                {
                    server.FastOpen = keyValue[1] == "true";
                }
                else if (key != "interface" && key != "allow-other-interface")
                {
                    this.logger.LogWarning($"Unrecognized snell options {key}={keyValue[1]}");
                }
            }

            if (server.Password == null)
            {
                this.logger.LogError("Snell password is missing!");
                return null;
            }

            var obfsMode = default(SnellObfuscationMode);
            switch (obfs)
            {
                case "http":
                    obfsMode = SnellObfuscationMode.HTTP;
                    break;
                case "tls":
                    obfsMode = SnellObfuscationMode.TLS;
                    break;
                case null:
                    obfsMode = SnellObfuscationMode.None;
                    obfsHost = null;
                    break;
                default:
                    this.logger.LogError("Unknown snell obfusaction mode: " + obfs);
                    return null;
            }

            server.ObfuscationMode = obfsMode;
            server.ObfuscationHost = obfsHost;
            return server;
        }

        public Server ParseClassicShadowsocksServer(string[] properties)
        {
            if (properties.Length < 5)
            {
                this.logger.LogError("Invaild surge shadowsocks proxy: " + string.Join(", ", properties));
                return null;
            }

            if (!int.TryParse(properties[2].Trim(), out int port))
            {
                this.logger.LogError("Invalid shadowsocks port: ", properties[2].Trim());
                return null;
            };


            var server = new ShadowsocksServer()
            {
                Host = properties[1].Trim(),
                Port = port,
                Method = properties[3].Trim(),
                Password = properties[4].Trim(),
            };

            var keyValues = properties
                .Skip(3)
                .Select(p => p.Trim().Split("=", 2))
                .Where(p => p.Length == 2)
                .Select(p => new string[] { p[0].TrimEnd(), p[1].TrimStart() });

            string obfs = null, obfsHost = null;
            foreach (var keyValue in keyValues)
            {
                var key = keyValue[0];
                if (key == "psk" || key == "password")
                {
                    server.Password = keyValue[1];
                }
                else if (key == "encrypt-method")
                {
                    server.Method = keyValue[1];
                }
                else if (key == "obfs")
                {
                    obfs = keyValue[1];
                }
                else if (key == "obfs-host")
                {
                    obfsHost = keyValue[1];
                }
                else if (key == "tfo")
                {
                    server.FastOpen = keyValue[1] == "true";
                }
                else if (key == "udp-relay")
                {
                    server.UDPRelay = keyValue[1] == "true";
                }
                else if (key != "interface" && key != "allow-other-interface")
                {
                    this.logger.LogWarning($"Unrecognized shadowsocks options {key}={keyValue[1]}");
                }
            }

            if (server.Password == null)
            {
                this.logger.LogError("Shadowsocks password is missing!");
                return null;
            }

            if (server.Method == null)
            {
                this.logger.LogError("Shadowsocks method is missing!");
                return null;
            }

            switch (obfs)
            {
                case "http":
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = SimpleObfsPluginMode.HTTP,
                        Host = obfsHost,
                    };
                    break;
                case "tls":
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = SimpleObfsPluginMode.TLS,
                        Host = obfsHost,
                    };
                    break;
                case null:
                    break;
                default:
                    this.logger.LogError("Unknown shadowsocks obfusaction mode: " + obfs);
                    return null;
            }

            return server;
        }

        public Server ParseShadowsocksServer(string[] properties)
        {
            if (properties.Length < 3)
            {
                this.logger.LogError("Invaild surge shadowsocks proxy: " + string.Join(", ", properties));
                return null;
            }

            if (!int.TryParse(properties[2].Trim(), out int port))
            {
                this.logger.LogError("Invalid shadowsocks port: ", properties[2].Trim());
                return null;
            };


            var server = new ShadowsocksServer()
            {
                Host = properties[1].Trim(),
                Port = port,
            };

            var keyValues = properties
                .Skip(3)
                .Select(p => p.Trim().Split("=", 2))
                .Where(p => p.Length == 2)
                .Select(p => new string[] { p[0].TrimEnd(), p[1].TrimStart() });

            string obfs = null, obfsHost = null;
            foreach (var keyValue in keyValues)
            {
                var key = keyValue[0];
                if (key == "psk" || key == "password")
                {
                    server.Password = keyValue[1];
                }
                else if (key == "encrypt-method")
                {
                    server.Method = keyValue[1];
                }
                else if (key == "obfs")
                {
                    obfs = keyValue[1];
                }
                else if (key == "obfs-host")
                {
                    obfsHost = keyValue[1];
                }
                else if (key == "tfo")
                {
                    server.FastOpen = keyValue[1] == "true";
                }
                else if (key == "udp-relay")
                {
                    server.UDPRelay = keyValue[1] == "true";
                }
                else if (key != "interface" && key != "allow-other-interface")
                {
                    this.logger.LogWarning($"Unrecognized shadowsocks options {key}={keyValue[1]}");
                }
            }

            if (server.Password == null)
            {
                this.logger.LogError("Shadowsocks password is missing!");
                return null;
            }

            if (server.Method == null)
            {
                this.logger.LogError("Shadowsocks method is missing!");
                return null;
            }

            switch (obfs)
            {
                case "http":
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = SimpleObfsPluginMode.HTTP,
                        Host = obfsHost,
                    };
                    break;
                case "tls":
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = SimpleObfsPluginMode.TLS,
                        Host = obfsHost,
                    };
                    break;
                case null:
                    break;
                default:
                    this.logger.LogError("Unknown shadowsocks obfusaction mode: " + obfs);
                    return null;
            }


            return server;
        }

        public Server[] ParseProxyList(string profile)
            => string.IsNullOrWhiteSpace(profile) ? null : this.ParseProxyList(profile.Trim().Split("\n"));

        public Server[] ParseProxyList(IEnumerable<string> plainProxies)
        {
            var servers = new List<Server>();

            foreach (var proxy in plainProxies)
            {
                var parts = proxy.Trim().Split("=", 2);
                if (parts.Length != 2)
                {
                    this.logger.LogError("Ignore invalid surge proxy line: " + proxy);
                }

                var name = parts[0];
                var properties = parts[1].Split(",");

                Server server = properties[0].Trim() switch
                {
                    "ss" => this.ParseShadowsocksServer(properties),
                    "custom" => this.ParseClassicShadowsocksServer(properties),
                    "snell" => this.ParseSnellServer(properties),
                    _ => null,
                };

                if (server == null)
                {
                    this.logger.LogError("Ignore unsupported protocol: " + properties[0]);
                }
                else
                {
                    server.Name = name.TrimEnd();
                    servers.Add(server);
                }
            }
            return servers.ToArray();
        }

        public Server[] Parse(string profile)
        {
            var properties = Misc.ParsePropertieFile(profile);

            var plainProxies = properties.GetValueOrDefault("Proxy");
            if (plainProxies == null)
            {
                return null;
            }

            return ParseProxyList(plainProxies);
        }

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
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
                    template = $"#!MANAGED-CONFIG {surgeOptions.ProfileURL} interval=43200\r\n" + template;
                }
            }

            return template
                .Replace(Surge.ServerListPlaceholder, EncodeProxyList(servers, out encodedServers))
                .Replace(Surge.ServerNamesPlaceholder, string.Join(", ", servers.Select(s => s.Name)));
        }

        public string EncodeProxyList(Server[] servers, out Server[] encodedServers)
        {
            var encodedServerList = new List<Server>();
            var sb = new StringBuilder();
            foreach (var server in servers)
            {
                if (server is ShadowsocksServer ssServer)
                {
                    sb.Append($"{server.Name} = ss, {server.Host}, {server.Port}, encrypt-method={ssServer.Method}, password={ssServer.Password}");

                    if (ssServer.PluginOptions is SimpleObfsPluginOptions options)
                    {
                        var obfsHost = string.IsNullOrEmpty(options.Host) ? Constant.ObfsucationHost : options.Host;
                        sb.Append($", obfs={SurgeParser.FormatSimpleObfsPluginMode(options.Mode)}, obfs-host={obfsHost}");
                    }

                    if (ssServer.UDPRelay)
                    {
                        sb.Append(", udp-relay=true");
                    }

                    if (ssServer.FastOpen)
                    {
                        sb.Append(", tfo=true");
                    }

                    sb.AppendLine();
                    encodedServerList.Add(server);
                }
                else if (server is SnellServer snellServer)
                {
                    sb.Append($"{server.Name} = ss, {server.Host}, {server.Port}, psk={snellServer.Password}");

                    if (snellServer.ObfuscationMode != SnellObfuscationMode.None)
                    {
                        sb.Append(", obfs=" + snellServer.ObfuscationMode.ToString().ToLower());
                        sb.Append(", obfs-host=" + (string.IsNullOrWhiteSpace(snellServer.ObfuscationHost) ? Constant.ObfsucationHost : snellServer.ObfuscationHost));
                    }

                    if (snellServer.FastOpen)
                    {
                        sb.Append(", tfo=true");
                    }

                    sb.AppendLine();
                    encodedServerList.Add(server);
                }
                else
                {
                    this.logger.LogInformation($"Server {server} is ignored.");
                }
            }

            encodedServers = encodedServerList.ToArray();
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