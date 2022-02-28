using System;

namespace Plan.Core.Models
{
    [Flags]
    public enum PlanItemUserTypes
    {
        /// <summary>
        /// 等待
        /// </summary>
        StandBy = 0,
        /// <summary>
        /// 已确认
        /// </summary>
        Acked = 1,
        /// <summary>
        /// 已接受
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// 已拒绝
        /// </summary>
        Refused = 3,
        /// <summary>
        /// 不确定
        /// </summary>
        Uncertain = 4
    }
}
