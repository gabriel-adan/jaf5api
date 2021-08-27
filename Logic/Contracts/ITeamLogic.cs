using Logic.Dtos;
using System.Collections.Generic;

namespace Logic.Contracts
{
    public interface ITeamLogic
    {
        bool RequestJoinToTeam(int turnId, int perfilId);

        TurnResultDto ResponseJoinToTeam(int turnId, int playerId, bool isAccepted);

        IList<TeamResumeDto> GetList(string email);

        IList<TeamResumeDto> GetJoinedList(string email);
    }
}
