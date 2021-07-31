using System;

namespace Logic.Dtos
{
    public class TurnResultDto : EntityDto
    {
        public DateTime DateTime { get; set; }
        public string Name { get; set; }
        public string Field { get; set; }

        public TurnResultDto(int id, DateTime dateTime, string name, string field) : base (id)
        {
            DateTime = dateTime;
            Name = name;
            Field = field;
        }
    }
}
