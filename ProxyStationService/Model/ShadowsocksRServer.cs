namespace ProxyStation.Model
{
    public class ShadowsocksRServer : Server
    {
        public string Password { get; set; }

        public string Method { get; set; }

        public string Obfuscation { get; set; }

        public string ObfuscationParameter { get; set; }

        public string Protocol { get; set; }

        public string ProtocolParameter { get; set; }

        public int UDPPort { get; set; }

        public bool UDPOverTCP { get; set; }

        public ShadowsocksRServer()
        {
            this.Type = ProxyType.ShadowsocksR;
        }
    }
}