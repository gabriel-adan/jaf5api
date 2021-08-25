using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Infrastructure.NHibernateMaps
{
    public class CampMap : IAutoMappingOverride<Camp>
    {
        public void Override(AutoMapping<Camp> mapping)
        {
            //mapping.Map(x => x.Longitude).ReadOnly();
            //mapping.Map(x => x.Latitude).ReadOnly();
            mapping.IgnoreProperty(x => x.Longitude);
            mapping.IgnoreProperty(x => x.Latitude);
            //mapping.Map(x => x.Longitude).Not.Insert().Not.Update();
            //mapping.Map(x => x.Latitude).Not.Insert().Not.Update();
        }
    }
}
