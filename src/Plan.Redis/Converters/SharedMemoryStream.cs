using Microsoft.IO;

namespace Plan.Redis.Converters
{
    internal static class SharedMemoryStream
    {
        public static readonly RecyclableMemoryStreamManager StreamManager=new RecyclableMemoryStreamManager();
    }
}
