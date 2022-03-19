using Plan.Services;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plan.Services.Models;
using Plan.Identity;
using Microsoft.Extensions.Options;
using RedLockNet;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Plan.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using SecurityLogin;
using SecurityLogin.Redis;

namespace Plan.Services
{
    public class AppService
    {

        /// <summary>
        /// <see cref="AppLoginResult"/>
        /// </summary>
        private static readonly string AppInfoKey = "Plan.Services.AppService.AppInfo";
        private static readonly string AppInfoNotExistKey = "Plan.Services.AppService.AppInfoNotExist";
        private static readonly string AppInfoSessionKey = "Plan.Services.AppService.AppInfoSession";
        private static readonly TimeSpan NotExistsCacheTime = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan AppInfoCacheTime = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan AppSessionTime = TimeSpan.FromDays(3);
        private static readonly Random random = new Random();

        private readonly ILogger<AppService> logger;
        private readonly IDatabase database;
        private readonly PlanDbContext planDbContext;
        private readonly IOptions<AppServiceOptions> options;

        public AppService(IDatabase database,
            IOptions<AppServiceOptions> options,
            ILogger<AppService> logger,
            PlanDbContext planDbContext)
        {
            this.database = database;
            this.options = options;
            this.logger = logger;
            this.planDbContext = planDbContext;
        }
        private Task<bool> DeleteAppCacheAsync(string appKey)
        {
            var redisKey = KeyGenerator.Concat(AppInfoKey, appKey);
            return database.KeyDeleteAsync(redisKey);
        }
        private async Task<AppInfoSnapshot> GetAppAsync(string appKey)
        {
            var notExistsKey = KeyGenerator.Concat(AppInfoNotExistKey, appKey);
            var isNotExists = await database.KeyExistsAsync(notExistsKey);
            if (isNotExists)
            {
                return null;
            }
            var redisKey = KeyGenerator.Concat(AppInfoKey, appKey);
            var hashs = await database.HashGetAllAsync(redisKey);
            if (hashs.Length == 0)
            {
                var data = await planDbContext.PlanApps.AsNoTracking()
                    .Where(x => x.AppKey == appKey)
                    .Select(x => new AppInfoSnapshot
                    {
                        AppKey = x.AppKey,
                        AppSecret = x.AppSecret,
                        EndTime = x.EndTime,
                        UserId = x.UserId,
                    }).FirstOrDefaultAsync();
                if (data == null)
                {
                    await database.StringSetAsync(notExistsKey, false, NotExistsCacheTime);
                    return null;
                }
                else
                {
                    var d = DefaultRedisOperator.GetRedisOperator(typeof(AppInfoSnapshot));
                    hashs = d.As(data);
                    await database.HashSetAsync(redisKey, hashs);
                    await database.KeyExpireAsync(redisKey, AppInfoCacheTime);
                    return data;
                }
            }
            else
            {
                var d = DefaultRedisOperator.GetRedisOperator(typeof(AppInfoSnapshot));
                var data = new AppInfoSnapshot();
                object dataObj = data;
                d.Write(ref dataObj, hashs);
                return data;
            }
        }
        public Task<bool> HasSessionAsync(string appKey, string session)
        {
            var redisKey = KeyGenerator.Concat(AppInfoSessionKey, appKey, session);
            return database.KeyExistsAsync(redisKey);
        }
        public async Task<DateTime?> GetSessionExpireTimeAsync(string appKey, string session)
        {
            var redisKey = KeyGenerator.Concat(AppInfoSessionKey, appKey, session);
            var val = await database.StringGetAsync(redisKey);
            if (val.HasValue && val.TryParse(out long ts))
            {
                return new DateTime(ts);
            }
            return null;
        }
        public async Task<AppInfoSnapshot> RegenSecAsync(string appKey)
        {
            //验证
            var appInfo = await GetAppAsync(appKey);
            if (appInfo != null)
            {
                var secret = GenSecretKey(appKey, DateTime.Now);
                await planDbContext.PlanApps.AsNoTracking()
                    .Where(x => x.AppKey == appKey)
                    .Take(1)
                    .UpdateFromQueryAsync(x => new PlanApp { AppSecret = secret });
                await DeleteSessionsAsync(appKey);
                await DeleteAppCacheAsync(appKey);
                appInfo.AppSecret = secret;
                return appInfo;
            }
            return null;
        }
        public async Task<int> DeleteAppAsync(string appKey)
        {
            var res = await planDbContext.PlanApps.Where(x => x.AppKey == appKey)
                .Take(1)
                .DeleteFromQueryAsync();
            if (res > 0)
            {
                await DeleteAppCacheAsync(appKey);
                await DeleteSessionsAsync(appKey);
            }
            //先不移除键了
            return res;
        }
        public Task<long> DeleteSessionsAsync(string appKey)
        {
            var key = KeyGenerator.Concat(AppInfoSessionKey, appKey);
            return database.DeleteScanKeysAsync(key + "*", 400);
        }
        
        
        private static string GenSecretKey(string appKey, DateTime time)
        {
            var appSecretKey = Guid.NewGuid().ToString() + appKey + time.Ticks + random.Next(1111, 9999);
            return Md5Helper.ComputeHashToString(appSecretKey);
        }
        public async Task<AppInfoSnapshot> CreateAppAsync(long userId, DateTime? endTime)
        {
            var now = DateTime.Now;
            var appKey = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var appSecret = GenSecretKey(appKey, now);
            var app = new PlanApp
            {
                AppKey = appKey,
                AppSecret = appSecret,
                CreateTime = now,
                Enable = true,
                EndTime = endTime,
                UserId = userId,
            };
            await planDbContext.PlanApps.SingleInsertAsync(app);
            return new AppInfoSnapshot
            {
                AppKey = appKey,
                UserId = userId,
                AppSecret = appSecret,
                EndTime = endTime
            };
        }
        public Task<TimeSpan?> GetSessionTimeAsync(string appKey, string session)
        {
            var redisKey = KeyGenerator.Concat(AppInfoSessionKey, appKey, session);
            return database.KeyIdleTimeAsync(redisKey);
        }
        public Task<bool> DeleteSessionAsync(string appKey, string session)
        {
            var redisKey = KeyGenerator.Concat(AppInfoSessionKey, appKey, session);
            return database.KeyDeleteAsync(redisKey);
        }

        public async Task<AppLoginResult> LoginAsync(string appKey, long timestamp, string sign)
        {
            var sn = await GetAppAsync(appKey);
            if (sn == null)
            {
                return new AppLoginResult { Code = AppLoginCode.NoSuchAppKey };
            }
            var now = DateTime.Now;
            if (sn.EndTime != null && sn.EndTime <= now)
            {
                return new AppLoginResult { Code = AppLoginCode.AppEndOfTime };
            }
            var csTime = TimeHelper.GetCsTime(timestamp);
            var subTime = Math.Abs((now - csTime).TotalMilliseconds);
            if (subTime > options.Value.RequestTimestampOffset.TotalMilliseconds)
            {
                return new AppLoginResult { Code = AppLoginCode.TimestampOutOfTime };
            }
            var mysignStr = appKey + timestamp + sn.AppSecret;
            var mysign = Md5Helper.ComputeHashToString(mysignStr);
            if (!mysign.Equals(sign, StringComparison.OrdinalIgnoreCase))
            {
                return new AppLoginResult { Code = AppLoginCode.SignError };
            }
            var accessToken = Guid.NewGuid().ToString();
            var tokenTime = AppSessionTime;
            if (sn.EndTime != null)
            {
                var minTime = sn.EndTime.Value - now;
                tokenTime = TimeSpan.FromMilliseconds(Math.Min(minTime.TotalMilliseconds, tokenTime.TotalMilliseconds));
            }
            var session = new AppLoginResult
            {
                CreateAt = now,
                AccessToken = accessToken,
                ExpireTime = (int)tokenTime.TotalMilliseconds
            };
            var redisKey = KeyGenerator.Concat(AppInfoSessionKey, appKey, accessToken);
            await database.StringSetAsync(redisKey, now.Add(tokenTime).Ticks, tokenTime);
            return session;
        }

    }
}
