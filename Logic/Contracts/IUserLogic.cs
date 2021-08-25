using Domain;

namespace Logic.Contracts
{
    public interface IUserLogic
    {
        Perfil PerfilExists(string email);

        Perfil CreatePerfil(string firstName, string lastName, string email);

        Customer CustomerExists(string email);

        Customer CreateCustomer(string firstName, string lastName, string email);

        User CreateUser(string firstName, string lastName, string email);
    }
}
