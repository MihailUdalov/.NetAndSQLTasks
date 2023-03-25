using RickAndMortyPersonInfoWebAPI.Models;
using System.Runtime.Caching;

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

        public static T Get<T>(string key) where T : class
        {
            return cache.Get(key) as T;
        }

        public static string GetKey(APIs api, string name)
        {
            switch (api)
            {
                case APIs.CharacterAPI:
                    return $"{APIs.CharacterAPI}_{name}";
                case APIs.EpisodeAPI:
                    return $"{APIs.EpisodeAPI}_{name}";
                default:
                    throw new Exception();
            }
        }
    }
}
