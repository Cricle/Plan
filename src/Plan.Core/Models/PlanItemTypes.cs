using System;

namespace Plan.Core.Models
{
    [Flags]
    public enum PlanItemTypes
    {
        /// <summary>
        /// 等待中
        /// </summary>
        StandBy = 0,
        /// <summary>
        /// 已开始
        /// </summary>
        Started = 1,
        /// <summary>
        /// 已取消
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// 已完成
        /// </summary>
        Complated = 3
    }
}
