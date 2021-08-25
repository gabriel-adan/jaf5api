using SharpArch.Domain.DomainModel;

namespace Domain
{
    public class CampImage : Entity
    {
        public virtual string Name { get; set; }

        public virtual Camp Camp { get; set; }
    }
}
