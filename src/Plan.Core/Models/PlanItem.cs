using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    public class PlanItem : IdentityDbEntityBase
    {
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        [Required]
        public PlanItemTypes NowType { get; set; }

        /// <summary>
        /// 重要程度
        /// </summary>
        public short Level { get; set; }

        /// <summary>
        /// 计划开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 计划结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [DataType(DataType.Date)]
        public byte[] Content { get; set; }

        [Required]
        public PlanItemContentTypes ContentType { get; set; }

        [Required]
        [ForeignKey(nameof(Group))]
        public long GroupId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public virtual PlanUser User { get; set; }

        public virtual PlanGroup Group { get; set; }

        public virtual ICollection<PlanItemUser> Users { get; set; }

        public virtual ICollection<PlanItemAnnex> Annices { get; set; }

        public virtual ICollection<PlanDelay> Delays { get; set; }

        public virtual ICollection<PlanNotifyJob> NotifyJobs { get; set; }
        
    }
}
