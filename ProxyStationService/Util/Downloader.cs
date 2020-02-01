using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ProxyStation.Util
{
    public class Downloader : IDownloader
    {
        public async Task<string> Download(string url)
        {
            var request = WebRequest.Create(url);
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