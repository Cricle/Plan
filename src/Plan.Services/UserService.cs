using Plan.ChannelModel;
using Plan.ChannelModel.Helpers;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Plan.Core.Models;
using Microsoft.Extensions.Options;
using RedLockNet;

namespace Plan.Services
{

    public class UserService: RSALoginService
    {
        private static readonly string RSAKey = "Plan.Services.UserService.RSAKey";
        private static readonly string SharedRSAIdentityKey = "Plan.Services.UserService.SharedRSAIdentity";
        private static readonly string SharedRSAKey = "Plan.Services.UserService.SharedRSAKey";
        private static readonly string SharedRSALockKey = "Red.Plan.Services.UserService.SharedRSAKey";

        private readonly UserIdentityService userIdentityService;
        private readonly UserManager<PlanUser> userManager;
        private readonly IOptions<UserServiceOptions> options;

        public UserService(IDatabase database,
            UserIdentityService userIdentityService,
            UserManager<PlanUser> userManager,
            IOptions<UserServiceOptions> options,
            IDistributedLockFactory lockFactory)
            :base(database,lockFactory)
        {
            this.options = options;
            this.userIdentityService = userIdentityService;
            this.userManager = userManager;
        }

        
        public Task<string> GenerateResetTokenAsync(string userName)
        {
            var user = new PlanUser { UserName = userName };
            return userManager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task<bool> RestPasswordAsync(string connectId, string userName, string resetToken, string @new)
        {
            var pwdNew = await DecryptAsync(connectId, @new);
            if (string.IsNullOrEmpty(pwdNew))
            {
                return false;
            }
            var user = new PlanUser { UserName = userName };
            var ok = await userManager.ResetPasswordAsync(user, resetToken, pwdNew);
            return ok.Succeeded;
        }
        public async Task<bool> RestPasswordWithOldAsync(string connectId, string userName, string old, string @new)
        {
            var header = GetHeader();
            var privateKey = await GetPrivateKeyAsync(header, connectId);
            if (privateKey is null)
            {
                return false;
            }
            var pwdOld = RSAHelper.RSADecrypt(privateKey, old);
            if (string.IsNullOrEmpty(pwdOld))
            {
                return false;
            }
            var pwdNew = RSAHelper.RSADecrypt(privateKey, @new);
            if (string.IsNullOrEmpty(pwdNew))
            {
                return false;
            }
            var user = new PlanUser { UserName = userName };
            var ok = await userManager.ChangePasswordAsync(user, pwdOld, pwdNew);
            return ok.Succeeded;
        }
        public async Task<bool> RegisteAsync(string connectId, string userName, string passwordHash)
        {
            var pwd = await DecryptAsync(connectId, passwordHash);
            if (string.IsNullOrEmpty(pwd))
            {
                return false;
            }
            var user = new PlanUser { UserName = userName };
            var identity = await userManager.CreateAsync(user, pwd);
            return identity.Succeeded;
        }
        protected override string GetRSAGenLockKey()
        {
            return SharedRSALockKey;
        }
        protected override string GetHeader()
        {
            return options.Value.SharedRSAKey ? SharedRSAKey : RSAKey;
        }
        protected override bool IsShared()
        {
            return options.Value.SharedRSAKey;
        }
        public async Task<string> LoginAsync(string connectId, string userName, string passwordHash)
        {
            var val = await DecryptAsync(connectId, passwordHash);
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            var u = await userManager.FindByNameAsync(userName);
            var ok = await userManager.CheckPasswordAsync(u, val);
            if (ok)
            {
                if (!options.Value.SharedRSAKey)
                {
                    await DeleteKeyAsync(connectId);
                }
                var identity = new UserSnapshot
                {
                    Email = u.Email,
                    Id = u.Id,
                    Name = u.NormalizedUserName
                };
                var tk = await userIdentityService.SetIdentityAsync(identity);
                return tk;
            }
            return null;
        }

        protected override string GetSharedIdentityKey()
        {
            return SharedRSAIdentityKey;
        }
    }
}
