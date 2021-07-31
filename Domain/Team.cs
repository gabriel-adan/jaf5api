using System.Collections.Generic;
using SharpArch.Domain.DomainModel;

namespace Domain
{
    public class Team : Entity
    {
        public virtual string Name { get; set; }
        public virtual bool IsPrivate { get; set; }

        public virtual Perfil Perfil { get; set; }
        public virtual IList<Player> Players { get; set; }
    }
}
