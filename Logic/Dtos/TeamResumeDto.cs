using System;

namespace Logic.Dtos
{
    public class TeamResumeDto : EntityDto
    {
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Players { get; set; }

        public TeamResumeDto(int id, string name, bool isPrivate, DateTime createdDate, int players) : base (id)
        {
            Name = name;
            IsPrivate = isPrivate;
            CreatedDate = createdDate;
            Players = players;
        }
    }
}
