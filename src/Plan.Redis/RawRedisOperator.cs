using Plan.Redis.Converters;
using StackExchange.Redis;
using System;

namespace Plan.Redis
{
    public class RawRedisOperator : IRedisOperator
    {
        public static readonly RedisValue defaultName = new RedisValue("Default");

        private IRedisValueConverter converter;

        public Type Target { get; }

        public RawRedisOperator(Type target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public HashEntry[] As(object value)
        {
            var val = RedisValue.EmptyString;
            if (value != null&& converter != null)
            {
                val = converter.Convert(null, value, null);
            }
            return new HashEntry[]
            {
                new HashEntry(defaultName, val)
            };
        }

        public void Build()
        {
            converter = KnowsRedisValueConverter.GetConverter(Target);
        }

        public void Write(ref object instance, HashEntry[] entries)
        {
            if (entries.Length != 0)
            {
                instance = converter.ConvertBack(entries[0].Value, null);
            }
        }
    }
}
