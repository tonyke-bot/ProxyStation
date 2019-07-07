using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ProxyStation.Model
{
    public class Profile
    {
        public string Source { get; set; }

        public ProfileType Type { get; set; }

        public string Name { get; set; }

        public bool AllowDirectAccess { get; set; }

        public async Task<string> Download()
        {
            var request = WebRequest.Create(Source);
            request.Headers.Set("User-Agent", "proxy-station/1.0.0");
            request.Timeout = 10 * 1000;  // 10s

            using (var response = await request.GetResponseAsync())
            using (var dataStream = response.GetResponseStream())
            {
                var reader = new StreamReader(dataStream);
                return reader.ReadToEnd();
            }
        }
    }
}