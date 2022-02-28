using Microsoft.Extensions.Logging;
using Plan.Core.Models;
using Plan.Identity;
using Plan.Services.Jobs;
using Quartz;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services
{
    public class NotifyEngine
    {
        private readonly ILogger<NotifyEngine> logger;
        private readonly IDatabase database;
        private readonly PlanDbContext planDbContext;

        public INotifyJobCreator GetJobType(NotifyTypes type)
        {

        }
        public async Task ScheduleAsync(IScheduler scheduler,PlanNotifyJob job)
        {

        }
    }
    public enum NotifyTypes:int
    {
        Pushy = 0,
        Gandi = 1,
    }
}
