using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Plan.Core.Models
{
    public class PlanGroup : IdentityDbEntityBase
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        public string Descript { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public int ItemCount { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public PlanUser User { get; set; }

        public virtual ICollection<PlanItem> Items { get; set; }

        public virtual ICollection<PlanItemUser> Users { get; set; }

        public virtual ICollection<PlanItemAnnex> Annices { get; set; }
    }
}
