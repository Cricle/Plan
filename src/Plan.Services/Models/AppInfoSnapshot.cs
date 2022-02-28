using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Models
{
    public class AppInfoSnapshot
    {
        public string AppKey { get; set; }

        public string AppSecret { get; set; }

        public DateTime? EndTime { get; set; }

        public long UserId { get; set; }
    }
}
