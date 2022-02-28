using System;

namespace Plan.Core.Models
{
    [Flags]
    public enum PlanNotifyContentTypes
    {
        /// <summary>
        /// 跟随<see cref="PlanItem.Content"/>
        /// </summary>
        ForrowItem = 0,
        /// <summary>
        /// 只使用<see cref="PlanNotifyJob.Content"/>
        /// </summary>
        OnlySelf = 1,
        /// <summary>
        /// 先<see cref="PlanItem.Content"/>再<see cref="PlanNotifyJob.Content"/>
        /// </summary>
        Both = 2,
        /// <summary>
        /// 先<see cref="PlanNotifyJob.Conten"/>再<see cref="PlanItem.Content"/>
        /// </summary>
        BothRev = 3
    }
}
