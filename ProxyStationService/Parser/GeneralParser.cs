using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public class GeneralParser : IProfileParser
    {
        private ILogger logger;

        public GeneralParser(ILogger logger)
        {
            this.logger = logger;
        }

        public Server[] Parse(string profile)
        {
            var bytes = WebSafeBase64Decode(profile.Trim());
            var plain = Encoding.UTF8.GetString(bytes);
            var URIs = plain.Split('\n');
            List<Server> servers = new List<Server>();

            foreach (var uri in URIs)
            {
                if (uri.StartsWith("ss://")) servers.Add(ParseShadowsocksURI(uri));
                else if (uri.StartsWith("ssr://")) servers.Add(ParseShadowsocksRURI(uri));
                else if (uri.StartsWith("vmess://"))
                {
                    // a common but not official standard scheme comes here
                    // https://github.com/2dust/v2rayN/wiki/%E5%88%86%E4%BA%AB%E9%93%BE%E6%8E%A5%E6%A0%BC%E5%BC%8F%E8%AF%B4%E6%98%8E(ver-2)
                }
                else
                {
                    // TODO: log error
                }
            }

            return servers.ToArray();
        }

        /// <summary>
        /// Follow scheme https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html
        /// to parse a given shadowsocks server uri.
        /// </summary>
        private ShadowsocksServer ParseShadowsocksURI(string ssURI)
        {
            var u = new Uri(ssURI);
            var server = new ShadowsocksServer();

            // UserInfo: WebSafe Base64(method:password)
            var plainUserInfo = Encoding.UTF8.GetString(WebSafeBase64Decode(HttpUtility.UrlDecode(u.UserInfo))).Split(':', 2);
            server.Method = plainUserInfo[0];
            server.Password = plainUserInfo.ElementAtOrDefault(1);

            // Host & Port
            server.Host = u.Host;
            server.Port = u.Port;

            // Name
            if (u.Fragment.StartsWith("#"))
                server.Name = HttpUtility.UrlDecode(u.Fragment.Substring(1));

            // Plugin
            var otherParams = HttpUtility.ParseQueryString(u.Query);
            var plugin = (otherParams.Get("plugin") ?? "").Trim();
            var pluginInfos = plugin.Split(";");
            switch (pluginInfos[0].Trim())
            {
                case "simple-obfs":
                case "obfs-local":
                    var options = new SimpleObfsPluginOptions();
                    server.PluginType = PluginType.SimpleObfs;
                    server.PluginOptions = options;

                    foreach (var info in pluginInfos)
                    {
                        var trimedInfo = info.Trim();
                        if (String.IsNullOrEmpty(options.Mode) && trimedInfo.StartsWith("obfs="))
                        {
                            options.Mode = trimedInfo.Substring("obfs=".Length);
                        }
                        else if (String.IsNullOrEmpty(options.Host) && trimedInfo.StartsWith("obfs-host="))
                        {
                            options.Host = trimedInfo.Substring("obfs-host=".Length);
                        }
                        else
                        {
                            // TODO: log
                        }
                    }
                    break;
            }



            return server;
        }


        /// <summary>
        /// Follow scheme https://github.com/shadowsocksr-backup/shadowsocks-rss/wiki/SSR-QRcode-scheme
        /// to parse a given shadowsocksR server uri.
        /// </summary>
        private ShadowsocksRServer ParseShadowsocksRURI(string ssrURI)
        {
            ssrURI = ssrURI.Substring("ssr://".Length);
            var splitted = Encoding.UTF8.GetString(WebSafeBase64Decode(HttpUtility.UrlDecode(ssrURI))).Split("/?");

            // host:port:protocol:method:obfs:base64pass
            var serverInfo = splitted[0].Split(":", 6);
            var server = new ShadowsocksRServer()
            {
                Host = serverInfo[0],
                Port = Convert.ToInt32(serverInfo.ElementAtOrDefault(1) ?? "0"),
                Protocol = serverInfo.ElementAtOrDefault(2),
                Method = serverInfo.ElementAtOrDefault(3),
                Obfuscation = serverInfo.ElementAtOrDefault(4),
                Password = Encoding.ASCII.GetString(Convert.FromBase64String(serverInfo.ElementAtOrDefault(5) ?? "")),
            };

            // /?obfsparam=base64param&protoparam=base64param&remarks=base64remarks&group=base64group&udpport=0&uot=0
            var otherInfos = splitted.ElementAtOrDefault(1);
            if (!String.IsNullOrEmpty(otherInfos))
            {
                var qs = HttpUtility.ParseQueryString(otherInfos);
                server.ProtocolParameter = qs["protoparam"] != null ? Encoding.UTF8.GetString(WebSafeBase64Decode(qs["protoparam"])) : null;
                server.ObfuscationParameter = qs["obfsparam"] != null ? Encoding.UTF8.GetString(WebSafeBase64Decode(qs["obfsparam"])) : null;
                server.Name = qs["remarks"] != null ? Encoding.UTF8.GetString(WebSafeBase64Decode(qs["remarks"])) : null;
                server.UDPOverTCP = qs["uot"] != null && qs["uot"] != "0";
                int UDPPort;
                if (Int32.TryParse(qs["udpport"], out UDPPort)) server.UDPPort = UDPPort;
            }

            return server;
        }

        private string EncodeShadowsocksServer(ShadowsocksServer server, string groupName = null)
        {
            var userInfo = WebSafeBase64Encode(Encoding.UTF8.GetBytes($"{server.Method}:{server.Password}"));
            var uri = new UriBuilder();
            var queryBuilder = new QueryBuilder();

            uri.UserName = userInfo;
            uri.Host = server.Host;
            uri.Port = server.Port;
            uri.Path = "/";
            uri.Fragment = HttpUtility.UrlEncode(server.Name).Replace("+", "%20");
            uri.Scheme = "ss";

            if (server.PluginType == PluginType.SimpleObfs)
            {
                var options = server.PluginOptions as SimpleObfsPluginOptions;
                var obfsHost = String.IsNullOrEmpty(options.Host) ? Constant.ObfsucationHost : options.Host;
                queryBuilder.Add("plugin", $"obfs-local;obfs={options.Mode};obfs-host={obfsHost}");
            }

            if (!String.IsNullOrEmpty(groupName))
                queryBuilder.Add("name", WebSafeBase64Encode(Encoding.UTF8.GetBytes(groupName)));

            uri.Query = queryBuilder.ToString().Replace(";", "%3B");

            return uri.ToString();
        }

        private string EncodeShadowsocksRServer(ShadowsocksRServer server, string groupName = null)
        {
            // base64(host:port:protocol:method:obfs:base64pass/?group=base64group)
            var serverInfo = $"{server.Host}:{server.Port}:{server.Protocol}:{server.Method}:{server.Obfuscation}";

            var otherInfos = new List<string>() {
                "remarks=" + WebSafeBase64Encode(Encoding.UTF8.GetBytes(server.Name))
            };
            if (!String.IsNullOrEmpty(server.ObfuscationParameter))
                otherInfos.Add("obfsparam=" + WebSafeBase64Encode(Encoding.UTF8.GetBytes(server.ObfuscationParameter)));
            if (!String.IsNullOrEmpty(server.ProtocolParameter))
                otherInfos.Add("protoparam=" + WebSafeBase64Encode(Encoding.UTF8.GetBytes(server.ProtocolParameter)));
            if (server.UDPPort > 0)
                otherInfos.Add($"udpport={server.UDPPort}");
            if (server.UDPOverTCP)
                otherInfos.Add("uot=1");
            if (!String.IsNullOrEmpty(groupName))
                otherInfos.Add("group=" + WebSafeBase64Encode(Encoding.UTF8.GetBytes(groupName)));

            return "ssr://" + WebSafeBase64Encode(Encoding.UTF8.GetBytes(serverInfo + "/?" + String.Join("&", otherInfos)));
        }

        public string Encode(Server[] servers, EncodeOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"REMARKS={options.ProfileName}");
            sb.AppendLine("");

            foreach (var server in servers)
            {
                switch (server)
                {
                    case ShadowsocksServer ss:
                        sb.AppendLine(EncodeShadowsocksServer(ss, options.ProfileName));
                        break;
                    case ShadowsocksRServer ssr:
                        sb.AppendLine(EncodeShadowsocksRServer(ssr, options.ProfileName));
                        break;
                }
            }
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(sb.ToString()));
        }

        private static string WebSafeBase64Encode(byte[] plainText)
        {
            return Convert.ToBase64String(plainText)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] WebSafeBase64Decode(string encodedText)
        {
            string replacedText = encodedText.Replace('_', '/').Replace('-', '+');
            switch (encodedText.Length % 4)
            {
                case 2: replacedText += "=="; break;
                case 3: replacedText += "="; break;
            }
            return Convert.FromBase64String(replacedText);
        }

        public string ExtName()
        {
            return ".conf";
        }
    }
}