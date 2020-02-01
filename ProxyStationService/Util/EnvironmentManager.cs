using System;

namespace ProxyStation.Util
{
    public class EnvironmentManager : IEnvironmentManager
    {
        public string Get(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}