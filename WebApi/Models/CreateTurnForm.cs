using System;

namespace WebApi.Models
{
    public class CreateTurnForm
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public int HourId { get; set; }
        public int PerfilId { get; set; }
    }
}
