using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProxyStation.Util
{
    public interface IDownloader
    {
        Task<string> Download(ILogger logger, string url);
    }
}