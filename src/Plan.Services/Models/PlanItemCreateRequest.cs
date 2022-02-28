using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Models
{
    public class PlanItemCreateRequest
    {
        public long GroupId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public short Level { get; set; }

        public bool BeginNow { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public List<PlanItemUserRequest> Users { get; set; }

        public List<PlanItemAnnexRequest> Annexs { get; set; }

        public List<PlanItemPlanNotifyJobRequest> NotifyJobs { get; set; }
    }
}
