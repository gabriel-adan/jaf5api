using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Infrastructure.NHibernateMaps
{
    public class TeamMap : IAutoMappingOverride<Team>
    {
        public void Override(AutoMapping<Team> mapping)
        {
            mapping.HasMany(x => x.Players).Cascade.All().Inverse();
        }
    }
}
