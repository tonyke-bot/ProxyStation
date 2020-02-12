using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.ProfileParser.Template;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace ProxyStation.ProfileParser
{
    public class ClashEncodeOptions : EncodeOptions
    {
    }

    public class ClashParser : IProfileParser
    {
        private ILogger logger;

        public ClashParser(ILogger logger)
        {
            this.logger = logger;
        }

        public Server[] Parse(string profile)
        {
            var servers = new List<Server>();
            using (var reader = new StringReader(profile))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var proxyNode = (yaml.Documents[0].RootNode as YamlMappingNode).Children["Proxy"];
                var proxies = (proxyNode as YamlSequenceNode).Children.ToList();

                foreach (YamlMappingNode proxy in proxies)
                {
                    switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "type"))
                    {
                        case "ss":
                            var server = ParseShadowsocksServer(proxy);
                            if (server != null) servers.Add(server);
                            break;
                        case "vmess":
                        default: break;
                    }
                }
            }

            return servers.ToArray();
        }

        public string Encode(EncodeOptions options, Server[] servers, out Server[] encodedServers)
        {
            var template = string.IsNullOrEmpty(options.Template) ? Clash.Template : options.Template;
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<object>(template);

            var rootElement = result as Dictionary<object, object>;
            if (rootElement == null)
            {
                throw new InvalidTemplateException();
            }

            var proxyGroupsElement = rootElement.GetValueOrDefault("Proxy Group") as List<object>;
            if (proxyGroupsElement == null)
            {
                throw new InvalidTemplateException();
            }

            var proxyGroupElement = proxyGroupsElement.ElementAtOrDefault(0) as Dictionary<object, object>;
            if (proxyGroupElement == null)
            {
                throw new InvalidTemplateException();
            }

            var encodedServersList = new List<Server>();
            // template modification
            var proxySettings = servers.Select(s =>
            {
                if (s is ShadowsocksServer ss)
                {
                    var proxy = new Dictionary<string, object>() {
                            { "name", ss.Name },
                            { "type", "ss" },
                            { "server", ss.Host },
                            { "port", ss.Port },
                            { "cipher", ss.Method },
                            { "password", ss.Password },
                        };
                    if (ss.UDPRelay) proxy.Add("udp", true);

                    var pluginOptions = new Dictionary<string, object>();
                    if (ss.PluginOptions is SimpleObfsPluginOptions obfsOptions)
                    {
                        pluginOptions.Add("mode", obfsOptions.Mode.ToString().ToLower());
                        if (obfsOptions.Mode == SimpleObfsPluginMode.HTTP)
                        {
                            pluginOptions.Add("host", string.IsNullOrEmpty(obfsOptions.Host) ? Constant.ObfsucationHost : obfsOptions.Host);
                        }
                        proxy.Add("plugin", "obfs");
                        proxy.Add("plugin-opts", pluginOptions);
                    }
                    else if (ss.PluginOptions is V2RayPluginOptions v2rayOptions)
                    {
                        if (v2rayOptions.Mode == V2RayPluginMode.WebSocket)
                        {
                            pluginOptions.Add("mode", v2rayOptions.Mode.ToString().ToLower());
                            pluginOptions.Add("host", v2rayOptions.Host);
                            pluginOptions.Add("path", string.IsNullOrEmpty(v2rayOptions.Path) ? "/" : v2rayOptions.Path);
                            if (v2rayOptions.SkipCertVerification) pluginOptions.Add("skip-cert-verify", true);
                            if (v2rayOptions.EnableTLS) pluginOptions.Add("tls", true);
                            if (v2rayOptions.Headers.Count > 0) pluginOptions.Add("headers", v2rayOptions.Headers);
                        }
                        else
                        {
                            this.logger.LogError($"Clash doesn't support v2ray-plugin on QUIC. This server will be ignored");
                            return null;
                        }
                        proxy.Add("plugin", "v2ray-plugin");
                        proxy.Add("plugin-opts", pluginOptions);
                    }

                    encodedServersList.Add(s);
                    return proxy;

                }
                else
                {
                    this.logger.LogDebug($"Server {s} is ignored.");
                    return null;
                }

            });
            rootElement.Add("Proxy", proxySettings);

            var proxyNames = proxySettings.Select(s => s["name"]).ToArray();
            proxyGroupElement["proxies"] = proxyNames;

            encodedServers = encodedServersList.ToArray();
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(rootElement);
        }

        public string ExtName() => ".yaml";

        public Server ParseShadowsocksServer(YamlMappingNode proxy)
        {
            string portString = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "port", "0");
            int port;
            if (!int.TryParse(portString, out port))
            {
                this.logger.LogError($"Invalid port: {port}.");
                return null;
            }

            var server = new ShadowsocksServer()
            {
                Port = port,
                Name = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "name"),
                Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "server"),
                Password = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "password"),
                Method = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "cipher"),
                UDPRelay = Yaml.GetTruthFromYamlChildrenNode(proxy, "udp"),
            };

            YamlNode pluginOptionsNode;
            // refer to offical clash to parse plugin options
            // https://github.com/Dreamacro/clash/blob/34338e7107c1868124f8aab2446f6b71c9b0640f/adapters/outbound/shadowsocks.go#L135
            if (proxy.Children.TryGetValue("plugin-opts", out pluginOptionsNode) && pluginOptionsNode.NodeType == YamlNodeType.Mapping)
            {
                switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "plugin").ToLower())
                {
                    case "obfs":
                        var simpleObfsModeString = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "mode");
                        var simpleObfsOptions = new SimpleObfsPluginOptions();

                        if (SimpleObfsPluginOptions.TryParseMode(simpleObfsModeString, out SimpleObfsPluginMode simpleObfsMode))
                        {
                            simpleObfsOptions.Mode = simpleObfsMode;
                        }
                        else if (!string.IsNullOrWhiteSpace(simpleObfsModeString))
                        {
                            this.logger.LogError($"Unsupported simple-obfs mode: {simpleObfsModeString}. This server will be ignored.");
                            return null;
                        }
                        simpleObfsOptions.Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "host");

                        server.PluginOptions = simpleObfsOptions;
                        break;
                    case "v2ray-plugin":
                        // also refer to official v2ray-plugin to parse v2ray-plugin options
                        // https://github.com/shadowsocks/v2ray-plugin/blob/c7017f45bb1e12cf1e4b739bcb8f42f3eb8b22cd/main.go#L126
                        var v2rayModeString = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "mode");
                        var options = new V2RayPluginOptions();
                        server.PluginOptions = options;

                        if (V2RayPluginOptions.TryParseMode(v2rayModeString, out V2RayPluginMode v2rayMode))
                        {
                            options.Mode = v2rayMode;
                        }
                        else
                        {
                            this.logger.LogError($"Unsupported v2ray-plugin mode: {v2rayModeString}. This server will be ignored.");
                            return null;
                        }

                        options.Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "host");
                        options.Path = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "path");
                        options.EnableTLS = Yaml.GetTruthFromYamlChildrenNode(pluginOptionsNode, "tls");
                        options.SkipCertVerification = Yaml.GetTruthFromYamlChildrenNode(pluginOptionsNode, "skip-cert-verify");
                        options.Headers = new Dictionary<string, string>();

                        YamlNode headersNode;
                        if (!(pluginOptionsNode as YamlMappingNode).Children.TryGetValue("headers", out headersNode))
                        {
                            break;
                        }
                        if (headersNode.NodeType != YamlNodeType.Mapping)
                        {
                            break;
                        }

                        foreach (var header in (headersNode as YamlMappingNode))
                        {
                            if (header.Value.NodeType != YamlNodeType.Scalar) continue;
                            options.Headers.Add((header.Key as YamlScalarNode).Value, (header.Value as YamlScalarNode).Value);
                        }
                        break;
                }
            }

            if (server.PluginOptions == null)
            {
                var simpleObfsModeString = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "obfs");

                if (SimpleObfsPluginOptions.TryParseMode(simpleObfsModeString, out SimpleObfsPluginMode simpleObfsMode))
                {
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = simpleObfsMode,
                        Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "obfs-host")
                    };
                }
                else if (!string.IsNullOrWhiteSpace(simpleObfsModeString))
                {
                    this.logger.LogError($"Unsupported simple-obfs mode: {simpleObfsModeString}");
                    return null;
                }
            }

            return server;
        }
    }
}