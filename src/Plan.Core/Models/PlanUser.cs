using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Plan.Core.Models
{
    public class PlanUser : IdentityUser<long>
    {
        [MaxLength(512)]
        public string Avatar { get; set; }

        public PlanUserStates States { get; set; }

        public ICollection<PlanApp> Apps { get; set; }

        public ICollection<PlanItemUser> ItemUsers { get; set; }
    }
    [Flags]
    public enum PlanUserStates
    {
        /// <summary>
        /// 没什么
        /// </summary>
        None=0,
        /// <summary>
        /// 不要约我
        /// </summary>
        DontAskMe=1
    }
}
