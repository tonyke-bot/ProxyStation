namespace ProxyStation.Model
{

    public abstract class Server
    {
        public ProxyType Type { get; protected set; }

        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }
}