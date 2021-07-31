using System;

namespace WebApi.Models
{
    public class RequestTurnForm
    {
        public DateTime Date { get; set; }
        public int HourId { get; set; }
        public int TeamId { get; set; }
        public int PerfilId { get; set; }
    }
}
