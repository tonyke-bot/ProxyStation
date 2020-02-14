namespace ProxyStation.Model
{
    public abstract class Server
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }
}