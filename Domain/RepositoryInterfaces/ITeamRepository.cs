using System.Collections.Generic;
using SharpArch.Domain.PersistenceSupport;

namespace Domain.RepositoryInterfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        IList<Team> GetList(string email);

        IList<Team> GetJoinedList(string email);
    }
}
