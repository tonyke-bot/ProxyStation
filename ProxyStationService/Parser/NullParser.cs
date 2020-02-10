using System;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public class NullParser : IProfileParser
    {
        public string Encode(Server[] servers, EncodeOptions options)
        {
            throw new NotImplementedException();
        }

        public string ExtName()
        {
            throw new NotImplementedException();
        }

        public Server[] Parse(string profile)
        {
            throw new NotImplementedException();
        }
    }
}