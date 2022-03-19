using Plan.Core.Models;
using System.Collections.Generic;

namespace Plan.Services.Models
{
    public class PlanItemPlanNotifyJobRequest
    {
        public string Content { get; set; }

        public PlanNotifyContentTypes ContentType { get; set; }

        public List<PlanItemPlanNotifyJobTriggerRequest> Triggers { get; set; }
    }
}
