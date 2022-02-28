using Plan.Core.Models;
using System;

namespace Plan.Services.Models
{
    public class PlanItemPlanNotifyJobRequest
    {
        public string Cron { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Content { get; set; }

        public PlanNotifyContentTypes ContentType { get; set; }
    }
}
