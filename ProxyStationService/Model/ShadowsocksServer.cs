using System.Collections.Generic;

namespace ProxyStation.Model
{
    public enum PluginType
    {
        None,
        SimpleObfs,
        V2Ray,
    }

    public abstract class PluginOptions { }

    public class SimpleObfsPluginOptions : PluginOptions
    {
        public string Mode { get; set; }
        public string Host { get; set; }
    }

    public class V2RayPluginOptions : PluginOptions
    {
        public string Mode { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public bool EnableTLS { get; set; }
        public bool SkipCertVerification { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class ShadowsocksServer : Server
    {
        public bool UDPRelay { get; set; }
        public string Method { get; set; }
        public PluginType PluginType { get; set; }
        public PluginOptions PluginOptions { get; set; }

        public ShadowsocksServer()
        {
            Type = ProxyType.Shadowsocks;
        }
    }
}