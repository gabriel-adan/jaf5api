using System;
using SharpArch.Domain.DomainModel;

namespace Domain
{
    public class Player : Entity
    {
        public virtual DateTime RequestDate { get; set; }
        public virtual DateTime? ConfirmDate { get; set; }

        public virtual Perfil Perfil { get; set; }
        public virtual Team Team { get; set; }
    }
}
