using Logic.Dtos;

namespace Logic.Contracts
{
    public interface ITeamLogic
    {
        bool RequestJoinToTeam(int turnId, int perfilId);

        TurnResultDto ResponseJoinToTeam(int turnId, int playerId, bool isAccepted);
    }
}
