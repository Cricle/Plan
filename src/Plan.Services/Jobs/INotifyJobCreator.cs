using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services.Jobs
{
    public interface INotifyJobCreator
    {
        string NotifyName { get; }

        Type GetJobType();

        void Config();
    }
}
