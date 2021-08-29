using System.Linq;
using System.Collections.Generic;
using Domain;
using Domain.RepositoryInterfaces;
using Logic.Dtos;
using Logic.Contracts;
using SharpArch.Domain.PersistenceSupport;
using NetTopologySuite.Geometries;

namespace Logic.Commons
{
    public class CampLogic : ICampLogic
    {
        private readonly ICustomerRepository customerRepository;
        private readonly ICampRepository campRepository;
        private readonly IRepository<Account> accountRepository;
        private readonly IFieldRepository fieldRepository;
        private readonly IUserRepository userRepository;
        private readonly IRepository<UserCamp> userCampRepository;

        public CampLogic(ICustomerRepository customerRepository, ICampRepository campRepository, IRepository<Account> accountRepository, IFieldRepository fieldRepository, IUserRepository userRepository, IRepository<UserCamp> userCampRepository)
        {
            this.customerRepository = customerRepository;
            this.campRepository = campRepository;
            this.accountRepository = accountRepository;
            this.fieldRepository = fieldRepository;
            this.userRepository = userRepository;
            this.userCampRepository = userCampRepository;
        }

        public CampDto Create(string email, string name, string street, string number, double longitude, double latitude, IList<string> fieldNames, int fieldCount, string imagesPath, int spatialReference)
        {
            try
            {
                campRepository.TransactionManager.BeginTransaction();
                Customer customer = customerRepository.Exists(email);
                Helper.ThrowIfNull(customer, "Cliente no registrado");
                Helper.ThrowIfIsNullOrEmpty(name, "Debe ingresar un nombre para la cancha");
                Helper.ThrowIfIsNullOrEmpty(street, "Debe ingresar una dirección");
                Helper.ThrowIf(fieldCount == 0, "Debe indicar la cantidad de canhas que tiene el predio");
                Helper.ThrowIfIsNullOrEmpty<string>(fieldNames, "Ocurrió un error al intentar registrar las canchas.");
                int countMax = fieldNames.Count;
                Helper.ThrowIf(fieldCount > countMax, "Por el momento solo es posible registrar hasta [" + countMax + "] canchas.");

                fieldNames = fieldNames.Take(fieldCount).ToList();

                Camp camp = new Camp();
                camp.Name = name;
                camp.Street = street;
                camp.Number = number;
                Coordinate coordinate = new Coordinate(longitude, latitude);
                Point location = new Point(coordinate);
                location.SRID = spatialReference;
                camp.Location = location;
                camp.IsEnabled = true;
                camp = campRepository.Save(camp);

                Account account = new Account();
                account.CreatedDate = Helper.GetDateTimeZone();
                account.IsEnabled = true;
                account.Customer = customer;
                account.Camp = camp;

                accountRepository.Save(account);

                int count = fieldNames.Count;
                for (int i = 0; i < count; i++)
                {
                    Field field = new Field();
                    field.Name = fieldNames[i];
                    field.IsEnabled = true;
                    field.Camp = camp;
                    fieldRepository.Save(field);
                }

                User user = userRepository.Exists(customer.Email);
                Helper.ThrowIfNull(user, "La cuenta del cliente no tiene usuario asignado");

                UserCamp userCamp = new UserCamp();
                userCamp.IsEnabled = true;
                userCamp.Account = account;
                userCamp.User = user;

                userCampRepository.Save(userCamp);
                campRepository.TransactionManager.CommitTransaction();

                return new CampDto(camp.Id, camp.Name, camp.Street, camp.Number, camp.IsEnabled, camp.Location.X, camp.Location.Y);
            }
            catch
            {
                campRepository.TransactionManager.RollbackTransaction();
                throw;
            }
        }

        public IList<FieldDto> GetFields(int campId)
        {
            try
            {
                IList<FieldDto> fieldDtos = new List<FieldDto>();
                IList<Field> fields = fieldRepository.List(campId);
                foreach (Field field in fields)
                {
                    FieldDto fieldDto = new FieldDto(field.Id, field.IsEnabled, campId);
                    fieldDtos.Add(fieldDto);
                }
                return fieldDtos;
            }
            catch
            {
                throw;
            }
        }

        public void EditFieldState(int fieldId, int campId, bool isEnabled)
        {
            try
            {
                Field field = fieldRepository.Get(fieldId);
                Helper.ThrowIfNull(field, "Datos de cancha inválidos");
                Helper.ThrowIf(field.Camp.Id != campId, "Datos de cancha inválidos");
                field.IsEnabled = isEnabled;
                fieldRepository.SaveOrUpdate(field);
            }
            catch
            {
                throw;
            }
        }

        public IList<CampDto> ListByBufferZone(double longitude, double latitude, float radius, int srId, string imagesPath)
        {
            try
            {
                var camps = campRepository.ListByBufferZone(longitude, latitude, radius, srId);
                return BuildCampList(camps, imagesPath);
            }
            catch
            {
                throw;
            }
        }

        public IList<CampDto> List(string userName, string imagesPath)
        {
            try
            {
                var camps = campRepository.List(userName);
                return BuildCampList(camps, imagesPath);
            }
            catch
            {
                throw;
            }
        }

        private IList<CampDto> BuildCampList(IList<Camp> camps, string imagesPath)
        {
            IList<CampDto> campDtos = new List<CampDto>();
            foreach (Camp camp in camps)
            {
                CampDto campDto = BuildCamp(camp, imagesPath);
                campDtos.Add(campDto);
            }
            return campDtos;
        }

        private CampDto BuildCamp(Camp camp, string imagesPath)
        {
            CampDto campDto = new CampDto(camp.Id, camp.Name, camp.Street, camp.Number, camp.IsEnabled, camp.Location.X, camp.Location.Y);
            foreach (CampImage campImage in camp.Images)
            {
                string fileUrl = string.Format("{0}/{1}", imagesPath, campImage.Name);
                CampImageDto campImageDto = new CampImageDto(campImage.Id, fileUrl);
                campDto.Images.Add(campImageDto);
            }
            return campDto;
        }
    }
}
