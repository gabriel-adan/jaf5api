using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using NHibernate.Spatial.Type;

namespace Infrastructure.NHibernateMaps
{
    public class CampMap : IAutoMappingOverride<Camp>
    {
        public void Override(AutoMapping<Camp> mapping)
        {
            mapping.Map(x => x.Location).CustomType<GeometryType>();
        }
    }
}
