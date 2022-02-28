using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Models
{
    public class GroupSnapsnot
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Descript { get; set; }

        public DateTime CreateTime { get; set; }

        public int ItemCount { get; set; }
    }
}
