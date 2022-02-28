using Plan.ChannelModel.Helpers;
using Plan.ChannelModel.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using RedLockNet;
using System.Text.Json;

namespace Plan.Services
{
    public abstract class RSALoginService
    {
        protected readonly IDatabase database;
        protected readonly IDistributedLockFactory lockFactory;

        private static readonly TimeSpan RSAKeyCacheTime = TimeSpan.FromMinutes(3);

        public RSALoginService(IDatabase database,
            IDistributedLockFactory lockFactory)
        {
            this.lockFactory = lockFactory;
            this.database = database;
        }

        public async Task<RSAKeyIdentity> FlushRSAKey()
        {
            var header = GetHeader();
            if (IsShared())
            {
                var identityKey = GetSharedIdentityKey();
                var identity=await database.StringGetAsync(identityKey);
                if (identity.HasValue)
                {
                    var redisKey = RedisKeyGenerator.Concat(header, identity.ToString());
                    var val = await database.StringGetAsync(redisKey);
                    if (val.HasValue)
                    {
                        var fullKey = val.Get<RSAFullKey>();
                        return new RSAKeyIdentity { Identity = fullKey.Identity, Key = fullKey.PublicKey };
                    }
                }
                using var locker = lockFactory.CreateLock(GetRSAGenLockKey(), RedisKeyGenerator.LockerWaitTime);
                if (locker.IsAcquired)
                {
                    var rsaIdentity = RSAHelper.GenerateRSASecretKey();
                    await database.StringSetAsync(identityKey, rsaIdentity.Identity.ToString(), GetCacheTime());
                    await SetRSAIdentityAsync(header, rsaIdentity);
                    return new RSAKeyIdentity { Identity = rsaIdentity.Identity, Key = rsaIdentity.PublicKey };
                }
                return null;//获取失败
            }
            else
            {
                var rsaIdentity = RSAHelper.GenerateRSASecretKey();
                await SetRSAIdentityAsync(header, rsaIdentity);
                return new RSAKeyIdentity { Identity = rsaIdentity.Identity, Key = rsaIdentity.PublicKey };
            }
        }
        protected abstract string GetRSAGenLockKey();
        protected virtual TimeSpan GetCacheTime()
        {
            return RSAKeyCacheTime;
        }
        protected abstract string GetSharedIdentityKey();

        protected virtual bool IsShared()
        {
            return true;
        }
        private Task SetRSAIdentityAsync(string header, RSAFullKey fullKey)
        {
            var redisKey = RedisKeyGenerator.Concat(header, fullKey.Identity);
            var bs = JsonSerializer.Serialize(fullKey);
            return database.StringSetAsync(redisKey, bs, GetCacheTime());
        }

        protected async Task<RSAFullKey> GetFullKeyAsync(string header, string connectId)
        {
            var key = RedisKeyGenerator.Concat(header, connectId);
            var fullKey = await database.StringGetAsync(key);
            if (!fullKey.HasValue)
            {
                return null;
            }
            var obj = JsonSerializer.Deserialize<RSAFullKey>(fullKey);
            return obj;
        }
        protected async Task<string> GetPrivateKeyAsync(string header, string connectId)
        {
            var fullKey = await GetFullKeyAsync(header, connectId);
            return fullKey?.PrivateKey;
        }
        protected abstract string GetHeader();

        protected async Task<string> DecryptAsync(string connectId, string passwordHash)
        {
            var header = GetHeader();
            var privateKey = await GetPrivateKeyAsync(header, connectId);
            if (privateKey is null)
            {
                return null;
            }
            return RSAHelper.RSADecrypt(privateKey, passwordHash);
        }
        protected Task DeleteKeyAsync(string connectId)
        {
            var header = GetHeader();
            var key = RedisKeyGenerator.Concat(header, connectId);
            return database.KeyDeleteAsync(key);
        }
    }
}
