using Plan.Services.Jobs;
using System;

namespace Plan.Services.Annotations
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class NotifyJobTypeAttribute:Attribute
    {
        public static readonly string NotifyJobCreatorTypeName = typeof(INotifyJobCreator).FullName;

        public NotifyJobTypeAttribute(Type notifyJobType)
        {
            NotifyJobType = notifyJobType ?? throw new ArgumentNullException(nameof(notifyJobType));
            if (notifyJobType.GetInterface(NotifyJobCreatorTypeName)==null)
            {
                throw new ArgumentException($"Type {notifyJobType} not implement {NotifyJobCreatorTypeName}");
            }
        }

        public Type NotifyJobType { get; }

    }
}
