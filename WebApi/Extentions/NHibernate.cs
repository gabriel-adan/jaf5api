using System.IO;
using NHibernate.Cfg;
using NHibernate.Spatial.Dialect;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using SharpArch.Domain.DomainModel;
using SharpArch.Domain.PersistenceSupport;
using Microsoft.Extensions.DependencyInjection;
using Domain;
using Infrastructure.Repositories;
using Infrastructure.NHibernateMaps;
using Infrastructure.NHibernateMaps.Conventions;

namespace WebApi.Extentions
{
    public static class NHibernate
    {
        public static IServiceCollection AddNHibernate(this IServiceCollection services, string connectionString)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(".." + Path.DirectorySeparatorChar + "Infrastructure" + Path.DirectorySeparatorChar + "NHibernateMaps" + Path.DirectorySeparatorChar + "NamedQueries.hbm.xml");
                Configuration cfg = new Configuration().AddFile(fileInfo);
                var autoPersistenceModel = AutoMap.AssemblyOf<Item>(new AutomappingConfiguration()).UseOverridesFromAssemblyOf<AutomappingConfiguration>();
                autoPersistenceModel.Conventions.AddFromAssemblyOf<LowercaseTableNameConvention>();
                autoPersistenceModel.IgnoreBase<Entity>();
                autoPersistenceModel.IgnoreBase(typeof(EntityWithTypedId<>));
                var sessionFactory = Fluently.Configure(cfg)
                    .Database(MySQLConfiguration.Standard.ConnectionString(connectionString).ShowSql().Dialect<MySQL57SpatialDialect>())
                    .Mappings(m =>
                    {
                        m.AutoMappings.Add(autoPersistenceModel);
                    })
                    .BuildSessionFactory();

                services.AddSingleton(sessionFactory);
                services.AddScoped(factory => sessionFactory.OpenSession());
                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddScoped(typeof(ITransactionManager), typeof(TransactionManager));
            }
            catch
            {
                throw;
            }
            return services;
        }
    }
}
