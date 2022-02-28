using Plan.Core.Models;
using System.IO.Compression;

namespace Plan.Services
{
    public class PlanItemOptions
    {
        public PlanItemContentTypes ContentType { get; set; } = PlanItemContentTypes.Gzip;

        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;
    }
}
