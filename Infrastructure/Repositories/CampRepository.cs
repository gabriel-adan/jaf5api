using Domain;
using Domain.RepositoryInterfaces;
using SharpArch.Domain.PersistenceSupport;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Spatial.Criterion;
using NetTopologySuite.Geometries;

namespace Infrastructure.Repositories
{
    public class CampRepository : Repository<Camp>, ICampRepository
    {
        public CampRepository(ISession session, ITransactionManager transactionManager) : base(session, transactionManager)
        {

        }

        public IList<Camp> ListByBufferZone(double longitude, double latitude, float radius, int spatialReference)
        {
            try
            {
                Coordinate coordinate = new Coordinate(longitude, latitude);
                Point point = new Point(coordinate);
                point.SRID = spatialReference;
                Geometry buffer = point.Buffer(radius);
                SpatialRelationCriterion spatialCriterion = SpatialRestrictions.Intersects("Location", buffer);
                return Session.CreateCriteria<Camp>()
                    .Add(Restrictions.And(spatialCriterion, Restrictions.Where<Camp>(c => c.IsEnabled)))
                        .List<Camp>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Camp> List(string userName)
        {
            try
            {
                var query = Session.GetNamedQuery("GetCampListByEmail");
                query.SetParameter("email", userName);
                return query.List<Camp>();
            }
            catch
            {
                throw;
            }
        }
    }
}
