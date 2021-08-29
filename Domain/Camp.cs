using System.Collections.Generic;
using SharpArch.Domain.DomainModel;
using NetTopologySuite.Geometries;

namespace Domain
{
    public class Camp : Entity
    {
        public virtual string Name { get; set; }
        public virtual string Street { get; set; }
        public virtual string Number { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual Point Location { get; set; }

        public virtual IList<Hour> Hours { get; set; }
        public virtual IList<Field> Fields { get; set; }
        public virtual IList<CampImage> Images { get; set; }
    }
}
