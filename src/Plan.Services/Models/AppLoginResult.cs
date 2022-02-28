using System;
using System.Collections.Generic;
using System.Text;

namespace Plan.Services.Models
{
    public class AppLoginResult
    {
        public AppLoginCode Code { get; set; }

        public string AccessToken { get; set; }

        public int ExpireTime { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
