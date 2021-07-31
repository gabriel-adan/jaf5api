namespace WebApi.Models
{
    public class PlayerJoinForm
    {
        public int TurnId { get; set; }
        public int PerfilId { get; set; }

        public int PlayerId { get; set; }
        public bool IsAccepted { get; set; }
    }
}
