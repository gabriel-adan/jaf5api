using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Infrastructure.NHibernateMaps.Conventions
{
    public class TimeSpanPropertyConvention : IPropertyConvention
    {
        public void Apply(IPropertyInstance instance)
        {
            if (instance.Property.PropertyType == typeof(TimeSpan))
                instance.CustomType("TimeAsTimeSpan");
        }
    }
}
