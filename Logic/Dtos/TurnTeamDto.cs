using System;

namespace Logic.Dtos
{
    public class TurnTeamDto : EntityDto
    {
        public string TeamName { get; set; }
        public string CampName { get; set; }
        public string Address { get; set; }
        public DateTime Timestamp { get; set; }
        public int PlayersAmount { get; set; }

        public TurnTeamDto(int id, string teamName, string campName, string address, DateTime timestamp, int playersAmount) : base (id)
        {
            TeamName = teamName;
            CampName = campName;
            Address = address;
            Timestamp = timestamp;
            PlayersAmount = playersAmount;
        }
    }
}
