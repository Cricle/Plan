using System;

namespace Plan.Services.Models
{
    public class PlanItemPlanNotifyJobTriggerRequest
    {
        public string Cron { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

    }
}
