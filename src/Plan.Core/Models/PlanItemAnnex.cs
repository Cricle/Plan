using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    /// <summary>
    /// 附件
    /// </summary>
    public class PlanItemAnnex
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(Item))]
        public long ItemId { get; set; }

        [Required]
        [ForeignKey(nameof(Group))]
        public long GroupId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        /// <summary>
        /// 资源文件路径
        /// </summary>
        [Required]
        public string FileUri { get; set; }

        public virtual PlanItem Item { get; set; }

        public virtual PlanGroup Group { get; set; }
    }
}
