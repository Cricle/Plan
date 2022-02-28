using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services
{
    public class AppServiceOptions
    {
        public TimeSpan RequestTimestampOffset { get; set; } = TimeSpan.FromMinutes(5);
    }
}
