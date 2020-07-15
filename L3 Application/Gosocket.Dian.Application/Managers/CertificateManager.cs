using Gosocket.Dian.Infrastructure;
using Org.BouncyCastle.X509;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Gosocket.Dian.Application.Managers
{
    public class CertificateManager
    {
        private static readonly object _lock = new object();

        private static readonly string container = $"dian";
        private static readonly string crtFilesFolder = $"certificates/crts/";
        private static readonly string crlFilesFolder = $"certificates/crls/";

        public static IDatabase cache;

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

        public X509Certificate[] GetRootCertificates()
        {
            X509Certificate[] certificates = null;

            var data = InstanceCache.CertificatesInstanceCache.GetCacheItem("Crts");

            if (data == null)
            {
                var buffers = GetBytesFromStorage(container, crtFilesFolder);
                var parser = new X509CertificateParser();
                certificates = buffers.Select(b => parser.ReadCertificate(b)).ToArray();

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24) };
                InstanceCache.CertificatesInstanceCache.Set(new CacheItem("Crts", certificates), policy);
            }
            else
                certificates = (X509Certificate[])data.Value;

            return certificates;
        }

        public X509Crl[] GetCrls()
        {
            X509Crl[] crls = null;

            var data = InstanceCache.CertificatesInstanceCache.GetCacheItem("Crls");

            if (data == null)
            {
                var buffers = GetBytesFromStorage(container, crlFilesFolder);
                var parser = new X509CrlParser();
                crls = buffers.Select(b => parser.ReadCrl(b)).ToArray();

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24) };
                InstanceCache.CertificatesInstanceCache.Set(new CacheItem("Crls", crls), policy);

            }
            else
                crls = (X509Crl[])data.Value;

            return crls;
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
    }

    public static class InstanceCache
    {
        public static MemoryCache CertificatesInstanceCache = MemoryCache.Default;
    }
}