using Logic.Dtos;
using System.Collections.Generic;

namespace Logic.Contracts
{
    public interface ICampLogic
    {
        CampDto Create(string email, string name, string street, string number, double longitude, double latitude, IList<string> fieldNames, int fieldCount, string imagesPath, int spatialReference);

        IList<FieldDto> GetFields(int campId);

        void EditFieldState(int fieldId, int campId, bool isEnabled);

        IList<CampDto> ListByBufferZone(double longitude, double latitude, float radius, int srId, string imagesPath);

        IList<CampDto> List(string userName, string imagesPath);
    }
}
