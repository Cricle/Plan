using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plan.ChannelModel.KeyGenerator;
using Plan.ChannelModel.Results;
using Plan.Core.Models;
using Plan.Identity;
using Plan.Services.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services
{
    public class PlanGroupService
    {
        private static readonly string GroupsKey = "Plan.Services.PlanGroupService.Groups";
        private static readonly TimeSpan GroupsCacheTime = TimeSpan.FromSeconds(1);

        private readonly ILogger<PlanGroupService> logger;
        private readonly IDatabase database;
        private readonly PlanDbContext planDbContext;

        public PlanGroupService(ILogger<PlanGroupService> logger, IDatabase database, PlanDbContext planDbContext)
        {
            this.logger = logger;
            this.database = database;
            this.planDbContext = planDbContext;
        }

        public async Task<EntityResult<bool>> CreateAsync(string name, string descript, long userId)
        {
            var group = new PlanGroup
            {
                CreateTime = DateTime.Now,
                Name = name,
                Descript = descript,
                UserId = userId
            };
            try
            {
                await planDbContext.SingleInsertAsync(group);
                await DeleteGroupsCacheAsync(userId);
                logger.LogInformation("{0} is create group with name {1}", userId, name);
                return new EntityResult<bool> { Data = true };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail to create group with userId {0}, name {1}", userId, name);

                return new EntityResult<bool> { Data = false };
            }
        }
        public async Task<EntityResult<bool>> DeleteAsync(long groupId, long userId)
        {
            var del = await planDbContext.PlanGroups.AsNoTracking()
                .Where(x => x.Id == groupId && x.UserId == userId)
                .Take(1)
                .DeleteFromQueryAsync();
            if (del>0)
            {
                await DeleteGroupsCacheAsync(userId);
            }
            return new EntityResult<bool> { Data = del > 0 };
        }
        public async Task<EntityResult<bool>> UpdateAsync(UpdateGroupRequest request)
        {
            var res = await planDbContext.PlanGroups.AsNoTracking()
                .Where(x => x.Id == request.Id && x.UserId == request.Id)
                .UpdateFromQueryAsync(x => new PlanGroup { Name = request.Name, Descript = request.Descript });
            if (res > 0)
            {
                await DeleteGroupsCacheAsync(request.UserId);
            }
            return new EntityResult<bool> { Data = res > 0 };
        }
        private Task<int> DeleteGroupsCacheAsync(long userId)
        {
            var key = RedisKeyGenerator.Concat(GroupsKey, userId);
            return database.DeleteScanKeysAsync(key + "*");
        }
        public async Task<SetResult<GroupSnapsnot>> GetGroupAsyncs(string keywork,long? userId,int? skip,int? take)
        {
            var key = RedisKeyGenerator.Concat(GroupsKey, userId, keywork, skip, take);
            var rs = await database.JsonGetAsync<SetResult<GroupSnapsnot>>(key);
            if (rs != null)
            {
                return rs;
            }
            var query = planDbContext.PlanGroups.AsNoTracking();
            if (userId!=null)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }
            if (string.IsNullOrEmpty(keywork))
            {
                query = query.Where(x => EF.Functions.Like($"%{keywork}%",x.Name)|| EF.Functions.Like($"%{keywork}%", x.Descript));
            }
            var count = await query.LongCountAsync();
            if (skip!=null)
            {
                query = query.Skip(skip.Value);
            }
            if (take != null)
            {
                query = query.Take(take.Value);
            }
            var datas = await query.Select(x => new GroupSnapsnot
            {
                CreateTime = x.CreateTime,
                Descript = x.Descript,
                Id = x.Id,
                ItemCount = x.ItemCount,
                Name = x.Name
            }).ToListAsync();
            var res = new SetResult<GroupSnapsnot>
            {
                Datas = datas,
                Skip = skip,
                Take = take,
                Total = count
            };
            await database.JsonSetAsync(res, key, GroupsCacheTime);
            return res;
        }
    }
}
