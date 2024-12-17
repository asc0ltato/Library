using WCFService.Model;
using System.Collections.Generic;
using System.ServiceModel;
using WCFService.DTO;
using System.Threading.Tasks;

namespace WCFService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string RegisterUser(string name, string login, string email, string password, byte[] profilePhoto);
        [OperationContract]
        int GetUserIdByLoginOrEmail(string loginOrEmail);
        [OperationContract]
        string LoginUser(string loginOrEmail, string password);

        [OperationContract]
        List<string> GetGenres();
        [OperationContract]
        List<BookDTO> GetBooks(string authorName, string genreName, string title);
        [OperationContract]
        List<BookDTO> GetAllBooks();
        [OperationContract]
        void AddBook(BookDTO book);
        [OperationContract]
        void UpdateBook(BookDTO bookDto);
        [OperationContract]
        string DeleteBook(int bookId);
        [OperationContract]
        BookDTO GetBookById(int bookId);

        [OperationContract]
        List<UserDTO> GetUsers();
        [OperationContract]
        Users GetUserInfo(int userId);
        [OperationContract]
        void UpdateUser(int userId, string name, string login, string email, string password, byte[] photo);
        [OperationContract]
        string DeleteUser(int userId);

        [OperationContract]
        List<TakenBookDTO> GetTakenBooksByUserId(int userId);
        [OperationContract]
        List<TakenBookDTO> GetAllTakenBooks();
        [OperationContract]
        string TakeBook(int userId, int bookId);
        [OperationContract]
        string ReturnBook(int userId, int sampleId);
        [OperationContract]
        bool HasUserTakenBook(int userId, int sampleId);

        [OperationContract]
        List<ReviewDTO> GetAllReviews(int currentUserId);
        [OperationContract]
        List<ReviewDTO> GetReviews();
        [OperationContract]
        ReviewDTO GetReviewById(int reviewId, int _currentUserId);
        [OperationContract]
        string AddBookReview(int userId, int bookId, string review, int rating);
        [OperationContract]
        void UpdateReview(int reviewId, string content, int rating);
        [OperationContract]
        void DeleteReview(int reviewId, int userId);
        [OperationContract]
        void AdminDeleteReview(int reviewId); 
    }
}