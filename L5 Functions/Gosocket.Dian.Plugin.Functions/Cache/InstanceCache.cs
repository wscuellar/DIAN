using System.Runtime.Caching;

namespace Gosocket.Dian.Plugin.Functions.Cache
{
    public static class InstanceCache
    {
        public static MemoryCache ContributorInstanceCache = MemoryCache.Default;
        public static MemoryCache CrlsInstanceCache = MemoryCache.Default;
        public static MemoryCache NumberRangesInstanceCache = MemoryCache.Default;
        public static MemoryCache SoftwareInstanceCache = MemoryCache.Default;
        public static MemoryCache TypesListInstanceCache = MemoryCache.Default;
    }
}
