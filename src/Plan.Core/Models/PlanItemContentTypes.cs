using System;

namespace Plan.Core.Models
{
    [Flags]
    public enum PlanItemContentTypes
    {
        Raw = 0,
        Gzip = 1,
        Deflate = 2,
    }
}
