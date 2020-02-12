using System.Collections.Generic;

namespace ProxyStation.Model
{
    public abstract class SSPluginOptions { }

    public enum SimpleObfsPluginMode
    {
        HTTP,

        TLS,
    }

    public class SimpleObfsPluginOptions : SSPluginOptions
    {
        public SimpleObfsPluginMode Mode { get; set; }

        public string Host { get; set; }

        public string Uri { get; set; }

        public static bool TryParseMode(string value, out SimpleObfsPluginMode mode)
        {
            mode = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLower())
            {
                case "tls":
                    mode = SimpleObfsPluginMode.TLS;
                    break;
                case "http":
                    mode = SimpleObfsPluginMode.HTTP;
                    break;
                default:
                    return false;
            };
            return true;
        }
    }

    public enum V2RayPluginMode
    {
        WebSocket,

        QUIC,
    }

    public class V2RayPluginOptions : SSPluginOptions
    {
        public V2RayPluginMode Mode { get; set; }

        public string Host { get; set; }

        public string Path { get; set; }

        public bool EnableTLS { get; set; }

        public bool SkipCertVerification { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public static bool TryParseMode(string value, out V2RayPluginMode mode)
        {
            mode = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLower())
            {
                case "ws":
                    mode = V2RayPluginMode.WebSocket;
                    break;
                case "websocket":
                    mode = V2RayPluginMode.WebSocket;
                    break;
                case "quic":
                    mode = V2RayPluginMode.QUIC;
                    break;
                default:
                    return false;

            };
            return true;
        }
    }

    public class ShadowsocksServer : Server
    {
        public string Password { get; set; }

        public string Method { get; set; }

        public bool UDPRelay { get; set; }

        public bool FastOpen { get; set; }

        public SSPluginOptions PluginOptions { get; set; }

        public override string ToString() => $"Shadowsocks<{this.Host}:{this.Port}>";
    }
}