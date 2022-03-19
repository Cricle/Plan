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
        private readonly NotifyEngine notifyEngine;

        public async Task CreateAsync(long userId, PlanItemCreateRequest request)
        {
            using var transaction = await planDbContext.Database.BeginTransactionAsync();
            try
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
                if (request.Users != null)
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
                if (request.NotifyJobs != null)
                {
                    var jobs = new List<PlanNotifyJob>();
                    foreach (var j in request.NotifyJobs)
                    {
                        var u = new PlanNotifyJob
                        {
                            JobKey = CreateKey(jobIdentity),
                            ContentType = j.ContentType,
                            Content = j.Content,
                            Triggers = j.Triggers.Select(x => new PlanNotifyJobTrigger
                            {
                                Cron = x.Cron,
                                StartTime = x.StartTime,
                                EndTime = x.EndTime,
                                TriggerKey = CreateKey(triggerIdentity),
                            }).ToList()
                        };
                        jobs.Add(u);
                    }
                    item.NotifyJobs = jobs;
                }
                planDbContext.Add(item);
                await planDbContext.SaveChangesAsync();

            }
            catch(Exception ex)
            {
                logger.LogError("Fail to create item",ex);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }
        public async Task StartAsync(long userId,long itemId)
        {

        }
        private async Task StartCoreAsync(PlanItem item,IEnumerable<PlanNotifyJob> jobs)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var scheduledId = new List<long>();
            var ctx = new NotifyContext { Item = item, Engine = notifyEngine };
            var creators = notifyEngine.NotifyJobManager.GetJobCreators(ctx).ToList();
            foreach (var job in jobs)
            {
                var jobKeyBuilder = JobBuilder.Create()
                    .WithIdentity(job.JobKey)
                    .UsingJobData(new JobDataMap
                    {

                    })
                    .RequestRecovery();
                foreach (var creator in creators)
                {
                    creator.ConfigKey(jobKeyBuilder);
                }
                var jobKey = jobKeyBuilder.Build();
                var exists = await scheduler.CheckExists(jobKey.Key);
                if (exists)
                {
                    scheduledId.Add(item.Id);
                }
                else
                {
                    await scheduler.AddJob(jobKey, false);
                }
                var triggerBuilder = TriggerBuilder.Create()
                    .WithIdentity(job.TriggerKey)
                    .ForJob(jobKey.Key);
                if (job.StartTime == null)
                {
                    triggerBuilder.StartNow();
                }
                else
                {
                    triggerBuilder.StartAt(job.StartTime.Value);
                }
                triggerBuilder.WithCronSchedule(job.Cron);
                if (job.EndTime != null)
                {
                    triggerBuilder.EndAt(job.EndTime.Value);
                }
                var trigger = triggerBuilder.Build();
                var foundTrigger = await scheduler.GetTrigger(trigger.Key);
                if (foundTrigger == null)
                {
                    await scheduler.ScheduleJob(jobKey, trigger);
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
