using Plan.Core.Models;

namespace Plan.Services
{
    internal class NotifyContext: INotifyContext
    {
        public NotifyEngine Engine { get; set; }

        public PlanItem Item { get; set; }
    }
}
