using Ao.ObjectDesign;
using Plan.Redis.Annotations;
using Plan.Redis.Converters;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Redis
{
    public interface IRedisOperator
    {
        void Build();

        void Write(ref object instance, HashEntry[] entries);

        HashEntry[] As(object value);
    }
}
