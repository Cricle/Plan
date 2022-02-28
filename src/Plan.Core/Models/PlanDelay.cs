using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    public class PlanDelay : IdentityDbEntityBase
    {
        /// <summary>
        /// 计划开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 计划结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string Descript { get; set; }

        [Required]
        [ForeignKey(nameof(Item))]
        public long ItemId { get; set; }

        public virtual PlanItem Item { get; set; }
    }
}
