using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RickAndMortyPersonInfoWebAPI
{
    public static class CacheManager
    {
        private static ObjectCache cache = MemoryCache.Default;
        //ten minutes
        private static readonly int expirationTime = 10;

        public static void Put<T>(string key, T data)
        {
            cache.Set(key, data, DateTimeOffset.Now.AddMinutes(expirationTime));
        }

        public static object Get<T>(string key) where T: class
        {
            return cache.Get(key) as T;
        }
    }
}
