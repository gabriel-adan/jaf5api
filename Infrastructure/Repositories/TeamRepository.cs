using Domain;
using Domain.RepositoryInterfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using SharpArch.Domain.PersistenceSupport;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        public TeamRepository(ISession session, ITransactionManager transactionManager) : base(session, transactionManager)
        {

        }

        public IList<Team> GetList(string email)
        {
            try
            {
                return Session.CreateCriteria<Team>()
                    .Add(Restrictions.Where<Team>(t => t.Perfil.Email == email)).List<Team>();
            }
            catch
            {
                throw;
            }
        }

        public IList<Team> GetJoinedList(string email)
        {
            try
            {
                return Session.CreateCriteria<Team>("t")
                    .CreateAlias("Players", "pl")
                    .CreateEntityAlias("p",
                        Restrictions.EqProperty("pl.Perfil.Id", "p.Id"),
                        JoinType.InnerJoin,
                        typeof(Perfil).FullName)
                        .Add(
                            Restrictions.And(
                                Restrictions.Eq("p.Email", email),
                                Restrictions.And(
                                    Restrictions.IsNotNull("pl.ConfirmDate"),
                                    Restrictions.NotEqProperty("p.Id", "t.Perfil.Id")
                                    )
                                )
                        )
                        .List<Team>();
            }
            catch
            {
                throw;
            }
        }
    }
}
