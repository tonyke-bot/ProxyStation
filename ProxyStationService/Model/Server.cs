namespace ProxyStation.Model
{
    public abstract class Server
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }

    public enum SnellObfuscationMode
    {
        None,
        HTTP,
        TLS,
    }

    public class SnellServer : Server
    {
        public string Password { get; set; }

        public SnellObfuscationMode ObfuscationMode { get; set; }

        public string ObfuscationHost { get; set; }

        public bool FastOpen { get; set; }
    }

}