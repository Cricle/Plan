using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Jobs
{
    public static class JobConstKeys
    {

    }
    public class NotifyJobManager : List<INotifyJobCreator>
    {
        public INotifyJobCreator GetJobCreator(INotifyContext context)
        {
            return GetJobCreators(context).FirstOrDefault();
        }
        public IEnumerable<INotifyJobCreator> GetJobCreators(INotifyContext context)
        {
            foreach (var item in this)
            {
                if (item.Condition(context))
                {
                    yield return item;
                }
            }
        }
    }

    public interface INotifyJobCreator
    {
        string NotifyName { get; }

        Type GetJobType();

        void ConfigKey(JobBuilder builder);

        bool Condition(INotifyContext context);
    }
}
