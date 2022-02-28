﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    //做成saas那种，可以有自己的服务
    public class PlanNotifyJob
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(255)]
        public string JobKey { get; set; }

        [MaxLength(255)]
        public string TriggerKey { get; set; }

        [Required]
        public string Cron { get; set; }

        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        /// <summary>
        /// 是否已经计划了任务
        /// </summary>
        public bool Scheduled { get; set; }

        [Required]
        public PlanNotifyContentTypes ContentType { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        [ForeignKey(nameof(Item))]
        public long ItemId { get; set; }

        public virtual PlanItem Item { get; set; }
    }
}
