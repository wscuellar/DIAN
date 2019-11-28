using System.Runtime.Caching;

namespace Gosocket.Dian.Services.Cache
{
    public class InstanceCache
    {
        public static MemoryCache AuthorizationsInstanceCache = MemoryCache.Default;
    }
}
