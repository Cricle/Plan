using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Plan.Core.Models
{
    public class PlanAppKey : IdentityDbEntityBase
    {
        [MaxLength(64)]
        public string Name { get; set; }
        
        [MaxLength(1024)]
        public string Key { get; set; }

        [MaxLength(1024)]
        public string Secret { get; set; }

        [Required]
        [ForeignKey(nameof(App))]
        public long AppId { get; set; }

        public PlanApp App { get; set; }
    }
}
