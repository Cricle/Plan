using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Plan.Core.Models
{
    /// <summary>
    /// 好友
    /// </summary>
    /// <remarks>
    /// 设计是有可以生成密匙/二维码，扫描即加好友
    /// </remarks>
    public class PlanUserFriend
    {
        [Required]
        [ForeignKey(nameof(OwnerUser))]
        public long OwnerUserId { get; set; }

        [Required]
        [ForeignKey(nameof(TargetUser))]
        public long TargetUserId { get; set; }

        public PlanUser TargetUser { get; set; }

        public PlanUser OwnerUser { get; set; }
    }
    public class PlanFriendKey:IdentityDbEntityBase
    {

        public bool Limit { get; set; }

        public short? UseableCount { get; set; }

        public short? CurrentCount { get; set; }

        [Required]
        [MaxLength(36)]
        public string Key { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public PlanUser User { get; set; }
    }
}
