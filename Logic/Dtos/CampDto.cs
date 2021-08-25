using System.Collections.Generic;

namespace Logic.Dtos
{
    public class CampDto : EntityDto
    {
        public string Name { get; }
        public string Street { get; }
        public string Number { get; }
        public bool IsEnabled { get; }
        public double Longitude { get; }
        public double Latitude { get; }

        public CampDto(int id, string name, string street, string number, bool isEnabled, double lng, double lat) : base (id)
        {
            Name = name;
            Street = street;
            Number = number;
            IsEnabled = isEnabled;
            Longitude = lng;
            Latitude = lat;
            Images = new List<CampImageDto>();
        }

        public IList<CampImageDto> Images { get; }
    }
}
