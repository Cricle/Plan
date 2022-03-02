//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Plan.Services.Jobs
//{
//    public class NotifyArgs
//    {
//        public string AppKey { get; set; }

//        public long NotifyId { get; set; }

//        public long ItemId { get; set; }
//    }
//    public abstract class NotifyJob : IJob
//    {
//        /// <summary>
//        /// <see cref="NotifyArgs"/>
//        /// </summary>
//        public static readonly string NotifyArgsKey = "Plan.Services.Jobs.NotifyJob.NotifyArgs";

//        public Task Execute(IJobExecutionContext context)
//        {
//        }
//    }
//    public class MailjetNotifyJob : IJob
//    {
        
//    }
//}
