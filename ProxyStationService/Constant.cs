using System;

namespace ProxyStation
{
    public static class Constant
    {
        public const string ObfsucationHost = "update.windows.com";

        public const string DownloaderCacheBlobContainer = "downloader-cache";

        public readonly static TimeSpan CacheOperationTimeout = TimeSpan.FromSeconds(5);

        public readonly static TimeSpan DownloadTimeout = TimeSpan.FromSeconds(10);
    }
}