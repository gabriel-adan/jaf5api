using Domain;
using Domain.RepositoryInterfaces;
using Logic.Contracts;
using SharpArch.Domain.PersistenceSupport;

namespace Logic.Commons
{
    public class UserLogic : IUserLogic
    {
        private readonly IPerfilRepository perfilRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IRepository<User> userRepository;

        public UserLogic(IPerfilRepository perfilRepository, ICustomerRepository customerRepository, IRepository<User> userRepository)
        {
            this.perfilRepository = perfilRepository;
            this.customerRepository = customerRepository;
            this.userRepository = userRepository;
        }

        public Perfil PerfilExists(string email)
        {
            try
            {
                return perfilRepository.Exists(email);
            }
            catch
            {
                throw;
            }
        }

        public Perfil CreatePerfil(string firstName, string lastName, string email)
        {
            try
            {
                Perfil perfil = perfilRepository.Exists(email);
                Helper.ThrowIfExists(perfil, "Ya existe una cuenta con este correo electrónico");
                perfil = new Perfil();
                perfil.Name = string.Format("{0} {1}", firstName, lastName);
                perfil.Email = email;
                return perfilRepository.Save(perfil);
            }
            catch
            {
                throw;
            }
        }

        public Customer CustomerExists(string email)
        {
            try
            {
                return customerRepository.Exists(email);
            }
            catch
            {
                throw;
            }
        }

        public User CreateUser(string firstName, string lastName, string email)
        {
            try
            {
                User user = new User();
                user.Name = string.Format("{0} {1}", firstName, lastName);
                user.Email = email;
                return userRepository.Save(user);
            }
            catch
            {
                throw;
            }
        }

        public Customer CreateCustomer(string firstName, string lastName, string email)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(firstName, "Debe ingresar un nombre");
                Helper.ThrowIfIsNullOrEmpty(lastName, "Debe ingresar un apellido");
                Helper.ThrowIfIsNullOrEmpty(email, "Debe ingresar un correo electrónico");
                Customer customer = new Customer();
                customer.FirstName = firstName;
                customer.LastName = lastName;
                customer.Email = email;
                return customerRepository.Save(customer);
            }
            catch
            {
                throw;
            }
        }
    }
}
