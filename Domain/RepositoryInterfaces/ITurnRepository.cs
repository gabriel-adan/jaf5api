using System;
using System.Collections.Generic;
using SharpArch.Domain.PersistenceSupport;

namespace Domain.RepositoryInterfaces
{
    public interface ITurnRepository : IRepository<Turn>
    {
        IList<Turn> GetRequests(DateTime date, Hour hour, Field field);

        IList<Turn> ListByBufferZone(double longitude, double latitude, float radius, DateTime dateTime);

        IList<HourTurn> List(int campId, DateTime fromDate, DateTime toDate);

        IList<Turn> ReserveList(DateTime date, int hourId, int campId);

        IList<Turn> RequestList(DateTime date, int hourId, int campId);

        IList<Turn> PublicIncompletedRequestList(int campId, int perfilId, DateTime dateTime);
    }
}
