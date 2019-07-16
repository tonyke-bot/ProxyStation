using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;
using ProxyStation.Util;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace ProxyStation.ProfileParser
{
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

        public string Encode(Server[] servers, IEncodeOptions options)
        {
            var dataDict = new Dictionary<string, object>()
            {
                { "port", "7890" },
                { "socks-port", "7891" },
                { "allow-lan", "true" },
                { "mode", "Rule" },
                { "log-level", "info" },
                { "external-controller", "127.0.0.1:9090" },
            };

            var dnsSettings = new Dictionary<string, object>()
            {
                { "enable", true },
                { "ipv6", false },
                { "listen", "0.0.0.0:53" },
                { "enhanced-mode", "redir-host" },
                { "nameserver", new string[]
                    {
                        "117.50.10.10",
                        "119.29.29.29",
                        "223.5.5.5",
                        "tls://dns.rubyfish.cn:853",
                    }
                },
                { "fallback", new string[]
                    {
                        "tls://1.1.1.1:853",
                        "tls://1.0.0.1:853",
                        "tls://dns.google:853",
                    }
                },
            };
            dataDict.Add("dns", dnsSettings);

            var proxySettings = servers.Select(s =>
            {
                switch (s)
                {
                    case ShadowsocksServer ss:
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
                        switch (ss.PluginOptions)
                        {
                            case SimpleObfsPluginOptions obfsOptions:
                                pluginOptions.Add("mode", obfsOptions.Mode);
                                if (obfsOptions.Mode == "http")
                                    pluginOptions.Add("host", String.IsNullOrEmpty(obfsOptions.Host) ? Constant.ObfsucationHost : obfsOptions.Host);

                                proxy.Add("plugin", "obfs");
                                proxy.Add("plugin-opts", pluginOptions);
                                break;
                            case V2RayPluginOptions v2rayOptions:
                                pluginOptions.Add("mode", v2rayOptions.Mode);
                                if (v2rayOptions.Mode == "websocket")
                                {
                                    pluginOptions.Add("host", v2rayOptions.Host);
                                    pluginOptions.Add("path", String.IsNullOrEmpty(v2rayOptions.Path) ? "/" : v2rayOptions.Path);
                                    if (v2rayOptions.SkipCertVerification) pluginOptions.Add("skip-cert-verify", true);
                                    if (v2rayOptions.EnableTLS) pluginOptions.Add("tls", true);
                                    if (v2rayOptions.Headers.Count > 0) pluginOptions.Add("headers", v2rayOptions.Headers);
                                }

                                proxy.Add("plugin", "v2ray-plugin");
                                proxy.Add("plugin-opts", pluginOptions);
                                break;
                        }

                        return proxy;
                }

                return null as object;
            });
            dataDict.Add("Proxy", proxySettings);

            var proxyGroup = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "name", "Proxy" },
                    { "type", "url-test" },
                    { "url", "http://www.gstatic.com/generate_204" },
                    { "interval", 300 },
                    { "proxies", servers.Select(s => s.Name).ToArray() },
                },
                new Dictionary<string, object>()
                {
                    { "name", "Default" },
                    { "type", "select" },
                    { "proxies", new string[] { "Proxy", "DIRECT" } },
                },
                new Dictionary<string, object>()
                {
                    { "name", "AdBlock" },
                    { "type", "select" },
                    { "proxies", new string[] { "REJECT", "DIRECT", "Proxy" } },
                },
            };
            dataDict.Add("Proxy Group", proxyGroup);

            var serializer = new SerializerBuilder().Build();
            var profile = serializer.Serialize(dataDict);

            return profile + ProfileSnippet.ClashRule;
        }

        public string Encode(Server[] servers) => Encode(servers, null);

        public string ExtName()
        {
            return ".yaml";
        }

        public static Server ParseShadowsocksServer(YamlMappingNode proxy)
        {
            int port;
            if (!Int32.TryParse(Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "port", "0"), out port))
                return null;

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
                switch (Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "plugin"))
                {
                    case "obfs":
                        server.PluginType = PluginType.SimpleObfs;
                        server.PluginOptions = new SimpleObfsPluginOptions()
                        {
                            Mode = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "mode"),
                            Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "host"),
                        };
                        break;
                    case "v2ray-plugin":
                        // also refer to official v2ray-plugin to parse v2ray-plugin options
                        // https://github.com/shadowsocks/v2ray-plugin/blob/c7017f45bb1e12cf1e4b739bcb8f42f3eb8b22cd/main.go#L126
                        var options = new V2RayPluginOptions();
                        server.PluginType = PluginType.V2Ray;
                        server.PluginOptions = options;

                        options.Host = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "host");
                        options.Mode = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "mode");
                        options.Path = Yaml.GetStringOrDefaultFromYamlChildrenNode(pluginOptionsNode, "path");
                        options.EnableTLS = Yaml.GetTruthFromYamlChildrenNode(pluginOptionsNode, "tls");
                        options.SkipCertVerification = Yaml.GetTruthFromYamlChildrenNode(pluginOptionsNode, "skip-cert-verify");
                        options.Headers = new Dictionary<string, string>();

                        YamlNode headersNode;
                        if (!(pluginOptionsNode as YamlMappingNode).Children.TryGetValue("headers", out headersNode))
                            break;
                        if (headersNode.NodeType != YamlNodeType.Mapping) break;

                        foreach (var header in (headersNode as YamlMappingNode))
                        {
                            if (header.Value.NodeType != YamlNodeType.Scalar) continue;
                            options.Headers.Add((header.Key as YamlScalarNode).Value, (header.Value as YamlScalarNode).Value);
                        }
                        break;
                }
            }


            if (server.PluginType == PluginType.None)
            {
                var obfs = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "obfs");
                var obfsHost = Yaml.GetStringOrDefaultFromYamlChildrenNode(proxy, "obfs-host");

                if (!String.IsNullOrEmpty(obfs))
                {
                    server.PluginType = PluginType.SimpleObfs;
                    server.PluginOptions = new SimpleObfsPluginOptions()
                    {
                        Mode = obfs,
                        Host = obfsHost
                    };
                }
            }

            return server;
        }
    }
}