using System;
using System.Collections.Generic;
using System.Globalization;
using Domain;
using Domain.RepositoryInterfaces;
using NHibernate;
using NHibernate.Criterion;
using SharpArch.Domain.PersistenceSupport;

namespace Infrastructure.Repositories
{
    public class TurnRepository : Repository<Turn>, ITurnRepository
    {
        public TurnRepository(ISession session, ITransactionManager transactionManager) : base(session, transactionManager)
        {

        }

        public IList<Turn> GetRequests(DateTime date, Hour hour, Field field)
        {
            try
            {
                return Session.CreateCriteria<Turn>()
                    .Add(Restrictions.Where<Turn>(t => t.Field == field && t.Date == date && t.Hour == hour && t.State == EState.REQUESTED))
                    .List<Turn>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Turn> ListByBufferZone(double longitude, double latitude, float radius, DateTime dateTime)
        {
            try
            {
                NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
                numberFormatInfo.NumberDecimalSeparator = ".";
                var query = Session.CreateSQLQuery(string.Format("CALL SP_TURNS_IN_BUFFER_ZONE({0}, {1}, {2}, '{3}');", longitude.ToString(numberFormatInfo), latitude.ToString(numberFormatInfo), radius.ToString(numberFormatInfo), dateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                query.AddEntity(typeof(Turn));
                return query.List<Turn>();
            }
            catch
            {
                throw;
            }
        }

        public IList<HourTurn> List(int campId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var query = Session.CreateSQLQuery(string.Format("CALL SP_HOUR_TURNS_STATUS('{0}', '{1}', {2});", fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), campId));
                query.AddEntity(typeof(HourTurn));
                return query.List<HourTurn>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Turn> ReserveList(DateTime date, int hourId, int campId)
        {
            try
            {
                return Session.CreateCriteria<Turn>("t")
                    .CreateCriteria("Hour", "h")
                    .CreateCriteria("Camp", "c")
                    .Add(Restrictions.Eq("t.Date", date))
                    .Add(Restrictions.Eq("h.Id", hourId))
                    ////.Add(Restrictions.Eq("t.Team", null))
                    .Add(Restrictions.Eq("t.Success", false))
                    .Add(Restrictions.Eq("t.State", EState.RESERVED))
                    .Add(Restrictions.Eq("c.Id", campId))
                    .List<Turn>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Turn> RequestList(DateTime date, int hourId, int campId)
        {
            try
            {
                return Session.CreateCriteria<Turn>("t")
                    .CreateCriteria("Hour", "h")
                    .CreateCriteria("Camp", "c")
                    .Add(Restrictions.Eq("t.Date", date))
                    .Add(Restrictions.Eq("h.Id", hourId))
                    .Add(Restrictions.Not(Restrictions.Eq("t.Team", null)))
                    .Add(Restrictions.Eq("t.Success", false))
                    .Add(Restrictions.Eq("t.State", EState.REQUESTED))
                    .Add(Restrictions.Eq("c.Id", campId))
                    .List<Turn>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Turn> PublicIncompletedRequestList(int campId, int perfilId, DateTime dateTime)
        {
            try
            {
                var query = Session.CreateSQLQuery(string.Format("CALL SP_PUBLIC_INCOMPLETED_TEAMS_IN_CAMP({0}, {1}, '{2}');", campId, perfilId, dateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                query.AddEntity(typeof(Turn));
                return query.List<Turn>();
            }
            catch
            {
                throw;
            }
        }
    }
}
