using System;

namespace Logic.Dtos
{
    public class TurnDto : EntityDto
    {
        public DateTime Date { get; set; }
        public string FullName { get; set; }
        public string FieldName { get; set; }
        public int? TeamId { get; set; }

        public TurnDto(int id, DateTime date, string fullName, string fieldName, int? teamId) : base (id)
        {
            Date = date;
            FullName = fullName;
            FieldName = fieldName;
            TeamId = teamId;
        }
    }
}
