using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.Common
{
    public static class RedisExtensions
    {
        private static readonly JsonSerializer m_serializer = new JsonSerializer();
        private static readonly SemaphoreSlim m_mutex = new SemaphoreSlim(1, 1);

        public async static Task<T> GetAsync<T>(this IDatabase cache, string key)
        {
            var value = await cache.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default(T);
            }

            return DeserializeJson<T>(value);
        }

        public static T Get<T>(this IDatabase cache, string key)
        {
            var value = cache.StringGet(key);
            if (!value.HasValue)
            {
                return default(T);
            }

            return DeserializeJson<T>(value);
        }

        public static async Task<T> GetOrSetAsync<T>(this IDatabase cache, string key, Func<T> func)
        {
            var value = await cache.GetAsync<T>(key);
            if (value == null)
            {
                await m_mutex.WaitAsync();
                try
                {
                    value = await cache.GetAsync<T>(key);
                    if (value == null)
                    {
                        value = func();
                        await cache.SetAsync(key, value);
                    }
                }
                finally
                {
                    m_mutex.Release();
                }
            }

            return value;
        }

        public static T GetOrSet<T>(this IDatabase cache, string key, Func<T> func)
        {
            var value = cache.Get<T>(key);
            if (value == null)
            {
                lock (m_mutex)
                {
                    value = cache.Get<T>(key);
                    if (value == null)
                    {
                        value = func();
                        cache.Set(key, value);
                    }
                }
            }

            return value;
        }

        public static async Task SetAsync(this IDatabase cache, string key, object value)
        {
            await cache.StringSetAsync(key, SerializeJson(value));
        }

        public static void Set(this IDatabase cache, string key, object value)
        {
            cache.StringSet(key, SerializeJson(value));
        }

        private static byte[] SerializeJson(object value)
        {
            if (value == null)
            {
                return null;
            }

            using (var stream = new MemoryStream())
            using (var zip = new GZipStream(stream, CompressionMode.Compress))
            using (var textWriter = new StreamWriter(zip))
            using (var writer = new JsonTextWriter(textWriter))
            {
                m_serializer.Serialize(writer, value);
                writer.Close();
                return stream.ToArray();
            }
        }

        private static T DeserializeJson<T>(byte[] buffer)
        {
            if (buffer == null)
            {
                return default(T);
            }

            using (var stream = new MemoryStream(buffer))
            using (var zip = new GZipStream(stream, CompressionMode.Decompress))
            using (var textReader = new StreamReader(zip))
            using (var reader = new JsonTextReader(textReader))
            {
                return m_serializer.Deserialize<T>(reader);
            }
        }
    }
}
