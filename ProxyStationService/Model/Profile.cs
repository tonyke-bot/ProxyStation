using System.Collections.Generic;
using System.Threading.Tasks;
using ProxyStation.ServerFilter;
using ProxyStation.Util;

namespace ProxyStation.Model
{
    public class Profile
    {
        public string Source { get; set; }

        public ProfileType Type { get; set; }

        public string Name { get; set; }

        public bool AllowDirectAccess { get; set; }

        public List<BaseFilter> Filters { get; set; } = new List<BaseFilter>();

        public async Task<string> Download(IDownloader downloader)
        {
            return await downloader.Download(this.Source);
        }
    }
}