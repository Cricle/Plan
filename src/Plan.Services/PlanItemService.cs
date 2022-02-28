using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Plan.Core.Models;
using Plan.Identity;
using Plan.Services.Models;
using Quartz;
using Quartz.Impl.Matchers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services
{
    public class PlanItemService
    {
        private static readonly string jobIdentity = "job";
        private static readonly string triggerIdentity = "trigger";

        private static readonly string StartJobKeyLock = "Red.Plan.Services.PlanItemService.StartJobKey";

        private static readonly Random random = new Random();
        private readonly ILogger<PlanGroupService> logger;
        private readonly IDatabase database;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly PlanDbContext planDbContext;
        private readonly IOptions<PlanItemOptions> options;
        private readonly RecyclableMemoryStreamManager streamManager;

        public async Task CreateAsync(long userId, PlanItemCreateRequest request)
        {
            var contentType = options.Value.ContentType;
            var item = new PlanItem
            {
                UserId = userId,
                GroupId = request.GroupId,
                Title = request.Title,
                CreateTime = DateTime.Now,
                BeginTime = request.BeginTime,
                EndTime = request.EndTime,
                Level = request.Level,
                NowType = PlanItemTypes.StandBy,
                ContentType = contentType,
                Content = Encoding.UTF8.GetBytes(request.Content)
            };
            if (contentType != PlanItemContentTypes.Raw)
            {
                using var stream = streamManager.GetStream();
                Stream zipStream = null;
                if (contentType == PlanItemContentTypes.Gzip)
                {
                    zipStream = new GZipStream(stream, options.Value.CompressionLevel);
                }
                else if (contentType == PlanItemContentTypes.Deflate)
                {
                    zipStream = new DeflateStream(stream, options.Value.CompressionLevel);
                }
                zipStream.Write(item.Content);
                item.Content = stream.ToArray();
                zipStream.Dispose();
            }
            if (request.Users!=null)
            {
                var usrs = new List<PlanItemUser>();
                foreach (var user in request.Users)
                {
                    var u = new PlanItemUser
                    {
                        UserId = user.UserId,
                        CurrentType = PlanItemUserTypes.StandBy,
                        GroupId = request.GroupId,
                        Note = user.Note
                    };
                    usrs.Add(u);
                }
                item.Users = usrs;
            }
            if (request.Annexs != null)
            {
                var annices = new List<PlanItemAnnex>();
                foreach (var annex in request.Annexs)
                {
                    var u = new PlanItemAnnex
                    {
                        GroupId = request.GroupId,
                        Name = annex.Name,
                        FileUri = annex.Uri
                    };
                    annices.Add(u);
                }
                item.Annices = annices;
            }
            if (request.Annexs != null)
            {
                var annices = new List<PlanItemAnnex>();
                foreach (var annex in request.Annexs)
                {
                    var u = new PlanItemAnnex
                    {
                        GroupId = request.GroupId,
                        Name = annex.Name,
                        FileUri = annex.Uri
                    };
                    annices.Add(u);
                }
                item.Annices = annices;
            }
            if (request.NotifyJobs!=null)
            {
                var jobs = new List<PlanNotifyJob>();
                foreach (var j in request.NotifyJobs)
                {
                    var u = new PlanNotifyJob
                    {
                        JobKey = CreateKey(jobIdentity),
                        TriggerKey = CreateKey(triggerIdentity),
                        StartTime = j.StartTime,
                        EndTime = j.EndTime,
                        ContentType = j.ContentType,
                        Content = j.Content,
                        Cron = j.Cron,
                    };
                    jobs.Add(u);
                }
                item.NotifyJobs = jobs;
            }
            planDbContext.Add(item);
            await planDbContext.SaveChangesAsync();
        }
        public async Task StartAsync(long userId,long itemId)
        {

        }
        private async Task StartCoreAsync(IEnumerable<PlanNotifyJob> jobs)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var scheduledId = new List<long>();
            foreach (var item in jobs)
            {
                var jobKey = JobKey.Create(item.JobKey);
                var exists = await scheduler.CheckExists(jobKey);
                if (exists)
                {
                    
                    scheduledId.Add(item.Id);
                }
                else
                {
                }
            }
        }
        private async Task StartItemAsync()
        {

        }
        private static string CreateKey(string identity)
        {
            return Guid.NewGuid() + $"-{identity}-" + random.Next(10000, 99999);
        }
    }
}
