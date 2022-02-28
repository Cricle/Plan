using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan.Core.Models
{
    [NotMapped]
    public abstract class DbEntityBase
    {
        public DateTime CreateTime { get; set; }
    }
}
