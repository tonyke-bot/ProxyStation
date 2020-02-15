using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxyStation.ServerFilter;
using ProxyStation.Util;

namespace ProxyStation.Model
{
    public class Profile : IEquatable<Profile>
    {
        public string Source { get; set; }

        public ProfileType Type { get; set; }

        public string Name { get; set; }

        public bool AllowDirectAccess { get; set; }

        public List<BaseFilter> Filters { get; set; } = new List<BaseFilter>();

        public async Task<string> Download(ILogger logger, IDownloader downloader)
        {
            return await downloader.Download(logger, this.Source);
        }

        public bool Equals([AllowNull] Profile other)
        {
            return other != null && other.Name == this.Name;
        }
    }
}