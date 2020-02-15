using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ProxyStation.Util
{
    public class Downloader : IDownloader
    {
        readonly string azureStorageConnectionString;

        public Downloader() : this(null)
        {
        }

        public Downloader(string azureStorageConnectionString)
        {
            this.azureStorageConnectionString = azureStorageConnectionString;
        }

        public async Task<string> Download(ILogger logger, string url)
        {
            try
            {
                var content = await this.DownloadRaw(logger, url);
                if (azureStorageConnectionString != null)
                {
                    try
                    {
                        await this.UpdateCache(logger, url, Encoding.UTF8.GetBytes(content));
                        logger.LogInformation($"Updated cache for url {url}");

                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Fail to update cache. Exception: {ex.Message}.\n    StackTrace: {ex.StackTrace}");
                    }
                }
                return content;
            }
            catch (Exception ex)
            {
                if (this.azureStorageConnectionString != null)
                {
                    logger.LogError($"Fail to download resource. Will try to use cache. Exception: {ex.Message}.\n    StackTrace: {ex.StackTrace}");
                    var cache = await this.GetCache(logger, url);

                    if (cache == null || cache.Length == 0)
                    {
                        logger.LogError($"Get null cache for url {url}");
                        return null;
                    }
                    logger.LogInformation($"Load cache for url {url}");
                    return Encoding.UTF8.GetString(cache);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<string> DownloadRaw(ILogger logger, string url)
        {
            var request = WebRequest.Create(url);
            request.Headers.Set("User-Agent", "proxy-station/1.0.0");
            request.Timeout = (int)Constant.DownloadTimeout.TotalMilliseconds;

            using var response = (await request.GetResponseAsync()) as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Fail to download {url}。 Status code: {response.StatusCode}");
            }
            using var dataStream = response.GetResponseStream();
            return await new StreamReader(dataStream).ReadToEndAsync();
        }

        CancellationToken GetCacheTimeoutToken() => new CancellationTokenSource(Constant.CacheOperationTimeout).Token;

        string GetCacheKey(string plainText)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(plainText));
            return string.Concat(hash.Select(b => b.ToString("X2")));
        }

        async Task<byte[]> GetCache(ILogger logger, string url)
        {
            string connectionString = azureStorageConnectionString;
            string containerName = Constant.DownloaderCacheBlobContainer;
            string blobName = this.GetCacheKey(url);

            var container = new BlobContainerClient(connectionString, containerName);
            var blob = container.GetBlobClient(blobName);
            var exist = await blob.ExistsAsync(this.GetCacheTimeoutToken());
            if (!exist)
            {
                logger.LogDebug($"Cache for url `{url}` is not missing");
                return null;
            }

            var response = await blob.DownloadAsync(this.GetCacheTimeoutToken());
            using var memoryStream = new MemoryStream();
            response.Value.Content.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        async Task<bool> UpdateCache(ILogger logger, string url, byte[] data)
        {
            string connectionString = azureStorageConnectionString;
            string containerName = Constant.DownloaderCacheBlobContainer;
            string blobName = this.GetCacheKey(url);

            var container = new BlobContainerClient(connectionString, containerName);
            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
            await blob.UploadAsync(new MemoryStream(data));
            return true;
        }
    }
}