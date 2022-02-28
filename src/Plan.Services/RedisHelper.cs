using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    internal static class RedisHelper
    {
        public const int DefaultPageSize = 200;

        public static Task<int> DeleteScanKeysAsync(this IDatabase database, string pattern)
        {
            return DeleteScanKeysAsync(database, pattern, DefaultPageSize);
        }
        public static async Task<int> DeleteScanKeysAsync(this IDatabase database,string pattern,int pageSize)
        {
            var count = 0;
            await foreach (var item in ScanKeys(database,pattern,pageSize))
            {
                var keys = AsRedisKey(item);
                count += keys.Length;
                await database.KeyDeleteAsync(keys);
            }
            return count;
        }
        public static IAsyncEnumerable<string[]> ScanKeys(this IDatabase database, string pattern)
        {
            return ScanKeys(database,pattern,DefaultPageSize);
        }
        public static async IAsyncEnumerable<string[]> ScanKeys(this IDatabase database, string pattern, int pageSize)
        {
            var count = 0;
            do
            {
                var res = await database.ExecuteAsync($"scan {count} match {pattern} count {pageSize}");
                yield return ((string[])res);
            } while (count == 0);
        }
        private static RedisKey[] AsRedisKey(string[] keys)
        {
            var val = new RedisKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                val[i] = keys[i];
            }
            return val;
        }
    }
}
