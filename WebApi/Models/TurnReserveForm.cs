using System;

namespace WebApi.Models
{
    public class TurnReserveForm
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int HourId { get; set; }
    }
}
