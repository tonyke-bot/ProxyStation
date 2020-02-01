using System.Threading.Tasks;

namespace ProxyStation.Util {
    public interface IDownloader {
        Task<string> Download(string url);
    }
}