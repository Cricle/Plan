using Ao.ObjectDesign;
using Plan.Redis.Converters;
using StackExchange.Redis;
using System.Reflection;

namespace Plan.Redis
{
    public interface IRedisColumn
    {
        IRedisValueConverter Converter { get; }

        PropertyGetter Getter { get; }

        PropertySetter Setter { get; }

        PropertyInfo Property { get; }

        string Name { get; }

        RedisValue NameRedis { get; }
    }
}
