using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Infrastructure;
using Org.BouncyCastle.X509;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using static Gosocket.Dian.Logger.Logger;

namespace Gosocket.Dian.Application.Managers
{
    public class CertificateManager
    {
        private static readonly object _lock = new object();

        //private static readonly Lazy<ConnectionMultiplexer> _cacheConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigurationManager.GetValue("GlobalRedis")));

        public static IDatabase cache;
        //= (_cacheConnection.Value).GetDatabase();

        private static CertificateManager _instance = null;

        public CertificateManager()
        {
            InitializeRedis();
        }

        private static void InitializeRedis()
        {
            if (cache == null)
            {
                cache = RedisConnectorManager.Connection.GetDatabase();
            }
        }

        public static CertificateManager Instance => _instance ?? (_instance = new CertificateManager());

        public X509Certificate[] GetRootCertificates(string container, string directory)
        {
            var lastUpdate = cache.GetOrSet("crtLastUpdate", () => DateTime.UtcNow);

            var data = (CacheData<X509Certificate[]>)MemoryCache.Default.Get("Crts");
            if (data == null || data.LastUpdate < lastUpdate)
            {
                var buffers = GetBytesFromStorage(container, directory);
                var parser = new X509CertificateParser();
                var certificates = buffers.Select(b => parser.ReadCertificate(b)).ToArray();

                data = new CacheData<X509Certificate[]>(lastUpdate, certificates);
                MemoryCache.Default.Set("Crts", data, new CacheItemPolicy());
            }
            return data.Data;
        }

        public X509Certificate[] OldGetRootCertificate(string container, string directory)
        {
            var buffers = cache.GetOrSet("crtBuffers", () => GetBytesFromStorage(container, directory));
            var parser = new X509CertificateParser();
            return buffers.Select(b => parser.ReadCertificate(b)).ToArray();
        }

        public X509Crl[] GetCrls(string container, string directory)
        {
            var buffers = cache.GetOrSet("crlBuffers", () => GetBytesFromStorage(container, directory));
            var parser = new X509CrlParser();
            return buffers.Select(b => parser.ReadCrl(b)).ToArray();
        }

        public static IEnumerable<byte[]> GetBytesFromStorage(string container, string directory)
        {
            var blobs = FileManager.Instance.GetFilesDirectory(container, directory);

            foreach (var blob in blobs)
            {
                var fileName = blob.Uri.Segments[blob.Uri.Segments.Length - 1];
                var bytes = FileManager.Instance.GetBytes(container, $"{directory}{fileName}");
                if (bytes != null)
                    yield return bytes;
            }
        }

        public class CacheData<T>
        {
            public CacheData(DateTime lastUpdate, T data)
            {
                LastUpdate = lastUpdate;
                Data = data;
            }
            public DateTime LastUpdate { get; }

            public T Data { get; }
        }
    }
}
