using System;
using ProxyStation.Model;

namespace ProxyStation.ProfileParser
{
    public interface IEncodeOptions { }

    public interface IProfileParser
    {
        Server[] Parse(string profile);

        string Encode(Server[] servers, IEncodeOptions options);

        string Encode(Server[] servers);

        string ExtName();
    }

    public class ParseException : Exception
    {
        private readonly string reason;
        private readonly string rawURI;

        public ParseException(string reason, string rawURI)
        {
            this.reason = reason;
            this.rawURI = rawURI;
        }
    }
}