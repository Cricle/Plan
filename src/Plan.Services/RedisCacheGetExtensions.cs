using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    internal static class RedisCacheGetExtensions
    {
        public static Task JsonSetAsync<T>(this IDatabase database,T val, string key,TimeSpan? cacheTime)
            where T : class
        {
            var bf = JsonSerializer.Serialize(val);
            return database.StringSetAsync(bf, key,cacheTime);
        }

        public static async Task<T> JsonGetAsync<T>(this IDatabase database,string key)
            where T : class
        {
            var val = await database.StringGetAsync(key);
            if (val.HasValue)
            {
                return JsonSerializer.Deserialize<T>(val);
            }
            return null;
        }
    }
}
