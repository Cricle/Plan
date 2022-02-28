using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Models
{
    public class UpdateGroupRequest
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string Name { get; set; }

        public string Descript { get; set; }
    }
}
