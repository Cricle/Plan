using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Plan.Core.Models
{
    public class PlanApp:IdentityDbEntityBase
    {
        [Required]
        [MaxLength(38)]
        public string AppKey { get; set; }

        [Required]
        [MaxLength(64)]
        public string AppSecret { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public bool Enable { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public virtual PlanUser User { get; set; }

        public virtual ICollection<PlanAppKey> Keys { get; set; }
    }
}
