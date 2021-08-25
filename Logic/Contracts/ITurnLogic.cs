using Domain;
using Logic.Dtos;
using System;
using System.Collections.Generic;

namespace Logic.Contracts
{
    public interface ITurnLogic
    {
        TurnResultDto Request(DateTime date, int hourId, int teamId, int perfilId);

        TurnResultDto Reserve(DateTime date, int hourId, string name);

        TurnResultDto CreateTeamTurn(DateTime date, int hourId, string name, bool isPrivate, int perfilId);

        IList<TurnTeamDto> ListByBufferZone(double longitude, double latitude, float radius);

        IList<HourTurn> List(int campId, DateTime fromDate, DateTime toDate);

        IList<TurnDto> GetReserveList(DateTime date, int hourId, int campId);

        void Confirm(int id);

        IList<TurnDto> GetRequestList(DateTime date, int hourId, int campId);

        IList<TurnTeamDto> GetPublicIncompletedReserveList(int campId, string email);
    }
}
