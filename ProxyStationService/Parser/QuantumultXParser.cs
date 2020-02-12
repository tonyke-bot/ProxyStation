using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser.Template;
using ProxyStation.Util;

namespace ProxyStation.ProfileParser
{
    public class QuantumultXParser : IProfileParser
    {
        readonly ILogger logger;

        readonly IDownloader downloader;

        readonly GeneralParser fallbackParser;

        public QuantumultXParser(ILogger logger, IDownloader downloader)
        {
            this.downloader = downloader;
            this.logger = logger;
            this.fallbackParser = new GeneralParser(logger);
        }
        public bool ValidateTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return true;
            }

            return template.Contains(QuantumultX.ServerListPlaceholder) && template.Contains(QuantumultX.ServerNamesPlaceholder);
        }

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
        {
            if (!this.ValidateTemplate(options.Template))
            {
                throw new InvalidTemplateException();
            }

            var template = string.IsNullOrEmpty(options.Template) ? QuantumultX.Template : options.Template;
            var profile = template
                .Replace(QuantumultX.ServerListPlaceholder, this.EncodeProxyList(servers, out encodedServers))
                .Replace(QuantumultX.ServerNamesPlaceholder, string.Join(", ", encodedServers.Select(s => s.Name)));

            return profile;
        }

        public string EncodeShadowsocksServer(ShadowsocksServer server)
        {
            var properties = new List<string>
            {
                $"shadowsocks={server.Host}:{server.Port}",
                $"method={server.Method}",
                $"password={server.Password}",
            };

            switch (server.PluginOptions)
            {
                case V2RayPluginOptions options:
                    if (options.Mode == V2RayPluginMode.QUIC)
                    {
                        // v2ray-plugin with QUIC is not supported
                        return null;
                    }
                    else if (options.Mode == V2RayPluginMode.WebSocket)
                    {
                        properties.Add("obfs=" + (options.EnableTLS ? "wss" : "ws"));
                        properties.Add($"obfs-host=" + (string.IsNullOrWhiteSpace(options.Host) ? Constant.ObfsucationHost : options.Host));

                        if (!string.IsNullOrWhiteSpace(options.Path) || options.Path == "/")
                        {
                            properties.Add($"obfs-uri={options.Path}");
                        }
                    }
                    break;
                case SimpleObfsPluginOptions options:
                    properties.Add($"obfs={options.Mode.ToString().ToLower()}");
                    properties.Add($"obfs-host=" + (string.IsNullOrWhiteSpace(options.Host) ? Constant.ObfsucationHost : options.Host));

                    if (options.Mode == SimpleObfsPluginMode.HTTP)
                    {
                        properties.Add($"obfs-uri={options.Uri}");
                    }
                    break;
            }

            properties.Add($"fast-open={server.FastOpen.ToString().ToLower()}");
            properties.Add($"udp-relay={server.UDPRelay.ToString().ToLower()}");
            properties.Add($"tag={server.Name}");
            return string.Join(", ", properties);
        }

        public string EncodeShadowsocksRServer(ShadowsocksRServer server)
        {
            var properties = new List<string>();
            properties.Add($"shadowsocks={server.Host}:{server.Port}");
            properties.Add($"method={server.Method}");
            properties.Add($"password={server.Password}");
            properties.Add($"ssr-protocol={server.Protocol}");
            properties.Add($"ssr-protocol-param={server.ProtocolParameter}");
            properties.Add($"obfs={server.Obfuscation}");
            properties.Add($"obfs-host={server.ObfuscationParameter}");
            properties.Add($"tag={server.Name}");
            return string.Join(", ", properties);
        }

        public string EncodeProxyList(Server[] servers, out Server[] encodedServers)
        {
            var encodedServersList = new List<Server>();
            var serverLines = servers
                .Select(server =>
                {
                    string serverLine = server switch
                    {
                        ShadowsocksServer ssServer => this.EncodeShadowsocksServer(ssServer),
                        ShadowsocksRServer ssrServer => this.EncodeShadowsocksRServer(ssrServer),
                        _ => null,
                    };
                    if (!string.IsNullOrWhiteSpace(serverLine))
                    {
                        encodedServersList.Add(server);
                    }
                    else
                    {
                        this.logger.LogInformation($"Server {server} is ignored.");
                    }
                    return serverLine;
                })
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            encodedServers = encodedServersList.ToArray();
            return string.Join('\n', serverLines);
        }

        public Server[] Parse(string profile)
        {
            var properties = Misc.ParsePropertieFile(profile);
            var servers = new List<Server>();

            var downloadRemoteServerListTasks = properties.GetValueOrDefault("server_remote")
                ?.Select(s =>
                {
                    var parts = s.Split(",");
                    if (parts.First(s => s.Trim().ToLower() == "enabled=false") != null)
                    {
                        return null;
                    }
                    return parts[0].Trim();
                })
                .Where(s => !string.IsNullOrWhiteSpace(s) && (s.StartsWith("https://") || s.StartsWith("http://") || s.StartsWith("ftp://")))
                .Select(async url =>
                {
                    this.logger.LogInformation("Downloading remote servers from " + url);
                    var plainProxies = await this.downloader.Download(url);

                    var remoteServers = this.ParseProxyList(plainProxies);
                    this.logger.LogInformation($"Get {remoteServers.Length} remote servers from {url}");
                    servers.AddRange(remoteServers);
                })
                .ToArray();
            Task.WaitAll(downloadRemoteServerListTasks);

            var localServers = this.ParseProxyList(properties.GetValueOrDefault("server_local"));
            servers.AddRange(localServers);

            return servers.ToArray();
        }

        public Server[] ParseProxyList(string plainProxies)
        {
            if (string.IsNullOrWhiteSpace(plainProxies)) return new Server[] { };

            var servers = this.ParseProxyList(plainProxies.Trim().Split("\n"));
            if (servers.Length != 0)
            {
                return servers;
            }

            // Try GeneralParser
            return fallbackParser.Parse(plainProxies);
        }

        public Server[] ParseProxyList(IEnumerable<string> plainProxies)
        {
            return plainProxies
                ?.Select(line =>
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) return null;
                    else if (line.StartsWith("ss://")) return this.fallbackParser.ParseShadowsocksURI(line);
                    else if (line.StartsWith("ssr://")) return this.fallbackParser.ParseShadowsocksURI(line);
                    else if (line.StartsWith("shadowsocks=")) return this.ParseShadowsocksLine(line);
                    else if (line.StartsWith("vmess=")) return null;
                    else if (line.StartsWith("http=")) return null;
                    else
                    {
                        this.logger.LogWarning($"[{{ComponentName}}] Unsupported proxy setting: {line}", this.GetType());
                        return null;
                    }
                })
                .Where(s => s != null)
                .ToArray();
        }

        /// <summary>
        /// Parse QuantumultX shadowsocks proxy setting
        /// Sample:
        ///   shadowsocks=example.com:80, method=chacha20, password=pwd, obfs=http, obfs-host=bing.com, obfs-uri=/resource/file, fast-open=false, udp-relay=false, server_check_url=http://www.apple.com/generate_204, tag=ss-01
        ///   shadowsocks=example.com:443, method=chacha20, password=pwd, ssr-protocol=auth_chain_b, ssr-protocol-param=def, obfs=tls1.2_ticket_fastauth, obfs-host=bing.com, tag=ssr
        /// </summary>
        /// <param name="line">A string starts with `shadowsocks=`</param>
        /// <returns>Could be an instance of <see cref="ShadowsocksRServer"> or <see cref="ShadowsocksServer"></returns>
        public Server ParseShadowsocksLine(string line)
        {
            var properties = new Dictionary<string, string>();
            foreach (var kvPair in line.Split(","))
            {
                var parts = kvPair.Split("=", 2);
                if (parts.Length != 2)
                {
                    this.logger.LogWarning($"[{{ComponentName}}] Invaild shadowsocks setting: {line}", this.GetType());
                    return null;
                }

                properties.TryAdd(parts[0].Trim().ToLower(), parts[1].Trim());
            }

            var host = default(string);
            var port = default(int);
            var method = properties.GetValueOrDefault("method");
            var password = properties.GetValueOrDefault("password");

            if (!Misc.SplitHostAndPort(properties.GetValueOrDefault("shadowsocks"), out host, out port))
            {
                this.logger.LogWarning($"[{{ComponentName}}] Host and port are invalid and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                return null;
            }

            if (string.IsNullOrEmpty(method))
            {
                this.logger.LogWarning($"[{{ComponentName}}] Encryption method is missing and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                return null;
            }

            if (string.IsNullOrEmpty(password))
            {
                this.logger.LogWarning($"[{{ComponentName}}] Password is missing and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                return null;
            }

            Server server;

            if (this.IsShadowsocksRServer(properties))
            {
                var ssrServer = new ShadowsocksRServer
                {
                    Method = method,
                };

                ssrServer.Obfuscation = properties.GetValueOrDefault("obfs");
                ssrServer.ObfuscationParameter = properties.GetValueOrDefault("obfs-host");
                ssrServer.Protocol = properties.GetValueOrDefault("ssr-protocol");
                ssrServer.ProtocolParameter = properties.GetValueOrDefault("ssr-protocol-param");
                // QuantumultX doesn't support following parameter yet
                //   * UDP Port
                //   * UDP over TCP

                server = ssrServer;
            }
            else
            {
                var ssServer = new ShadowsocksServer
                {
                    Method = method,
                };

                if (bool.TryParse(properties.GetValueOrDefault("udp-relay", "false"), out bool udpRelay))
                {
                    ssServer.UDPRelay = udpRelay;
                }

                if (bool.TryParse(properties.GetValueOrDefault("fast-open", "false"), out bool fastOpen))
                {
                    ssServer.FastOpen = fastOpen;
                }

                var obfs = properties.GetValueOrDefault("obfs", "").ToLower();
                switch (obfs)
                {
                    case "tls":
                    case "http":
                        {
                            if (!SimpleObfsPluginOptions.TryParseMode(obfs, out SimpleObfsPluginMode mode))
                            {
                                this.logger.LogWarning($"[{{ComponentName}}] Simple-obfs mode `{obfs}` is not supported and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                                return null;
                            }

                            var options = new SimpleObfsPluginOptions()
                            {
                                Host = properties.GetValueOrDefault("obfs-host"),
                                Mode = mode,
                            };

                            if (obfs != "tls")
                            {
                                options.Uri = properties.GetValueOrDefault("obfs-uri");
                            }

                            ssServer.PluginOptions = options;
                        }
                        break;
                    case "wss":
                    case "ws":
                        {
                            if (!V2RayPluginOptions.TryParseMode(obfs, out V2RayPluginMode mode))
                            {
                                this.logger.LogWarning($"[{{ComponentName}}] Simple-obfs mode `{obfs}` is not supported and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                                return null;
                            }

                            var options = new V2RayPluginOptions()
                            {
                                Mode = mode,
                                Host = properties.GetValueOrDefault("obfs-host"),
                                Path = properties.GetValueOrDefault("obfs-uri"),
                                EnableTLS = obfs == "wss",
                            };

                            ssServer.PluginOptions = options;
                        }
                        break;
                    default:
                        this.logger.LogWarning($"[{{ComponentName}}] Obfuscation `{obfs}` is not supported and this proxy setting is ignored. Invaild shadowsocks setting : {line}", this.GetType());
                        return null;
                }

                server = ssServer;
            }

            server.Host = host;
            server.Port = port;
            server.Name = properties.GetValueOrDefault("tag", $"{server.Host}:{server.Port}");

            return server;
        }

        private bool IsShadowsocksRServer(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("ssr-protocol"))
            {
                return true;
            }

            return false;
        }

        // TODO: VMess Parser
        // ;vmess=example.com:80, method=none, password=23ad6b10-8d1a-40f7-8ad0-e3e35cd32291, fast-open=false, udp-relay=false, tag=vmess-01
        // ;vmess=example.com:80, method=aes-128-gcm, password=23ad6b10-8d1a-40f7-8ad0-e3e35cd32291, fast-open=false, udp-relay=false, tag=vmess-02
        // ;vmess=example.com:443, method=none, password=23ad6b10-8d1a-40f7-8ad0-e3e35cd32291, obfs=over-tls, fast-open=false, udp-relay=false, tag=vmess-tls
        // ;vmess=example.com:80, method=chacha20-poly1305, password=23ad6b10-8d1a-40f7-8ad0-e3e35cd32291, obfs=ws, obfs-uri=/ws, fast-open=false, udp-relay=false, tag=vmess-ws
        // ;vmess=example.com:443, method=chacha20-poly1305, password=23ad6b10-8d1a-40f7-8ad0-e3e35cd32291, obfs=wss, obfs-uri=/ws, fast-open=false, udp-relay=false, tag=vmess-ws-tls
        // public Server ParseVMessURI(string uri) {

        // }

        public string ExtName() => ".conf";
    }
}