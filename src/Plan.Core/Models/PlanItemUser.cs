using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    /// <summary>
    /// 目标用户
    /// </summary>
    public class PlanItemUser
    {
        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        [Required]
        [ForeignKey(nameof(Item))]
        public long ItemId { get; set; }

        [Required]
        [ForeignKey(nameof(Group))]
        public long GroupId { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        [Required]
        public PlanItemUserTypes CurrentType { get; set; }

        /// <summary>
        /// 评价
        /// </summary>
        public PlanItemUserTypes? Appraise { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public virtual PlanItem Item { get; set; }

        public virtual PlanUser User { get; set; }

        public virtual PlanGroup Group { get; set; }
    }
}
