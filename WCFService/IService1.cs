using WCFService.Model;
using System.Collections.Generic;
using System.ServiceModel;
using WCFService.DTO;

namespace WCFService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string RegisterUser(string name, string login, string email, string password, byte[] profilePhoto);

        [OperationContract]
        string LoginUser(string loginOrEmail, string password);

        [OperationContract]
        List<BookDTO> GetBooks(string authorName, string genreName, string title);

        [OperationContract]
        Users GetUserInfo(int userId);
        [OperationContract]
        int GetUserIdByLoginOrEmail(string loginOrEmail);

        [OperationContract]
        void UpdateUser(int userId, string name, string login, string email, string password, byte[] photo);
        [OperationContract]
        List<UserDTO> GetUsers();
        [OperationContract]
        void DeleteUser(int userId);

        /*[OperationContract]
        IEnumerable<Users> GetAllUsers();

        [OperationContract]
        string AddUser(string name, string login, string email, string password);

        [OperationContract]
        string UpdateUser(int userId, string name, string email, string password);

        [OperationContract]
        string DeleteUser(int userId);*/
    }
}
