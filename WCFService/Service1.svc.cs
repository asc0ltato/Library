using WCFService.Model;
using WCFService.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WCFService.DTO;
using System.Data.Entity;
using System.Xml.Linq;
using System.ServiceModel;

namespace WCFService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class Service1 : IService1
    {
        private readonly IUnitOfWork _unitOfWork;
        public Service1()
        {
            var context = new LibraryContext();
            _unitOfWork = new UnitOfWork(context);
        }

        public Service1(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public string RegisterUser(string name, string login, string email, string password, byte[] profilePhoto)
        {
            try
            {
                var existingUserByEmail = _unitOfWork.Users.Find(u => u.Email == email).FirstOrDefault();
                if (existingUserByEmail != null)
                {
                    return "Пользователь с таким Email уже существует.";
                }

                var existingUserByLogin = _unitOfWork.Users.Find(u => u.Login == login).FirstOrDefault();
                if (existingUserByLogin != null)
                {
                    return "Пользователь с таким Login уже существует.";
                }

                var adminRole = _unitOfWork.Roles.Find(r => r.Id == 1 && r.Name == "Admin").FirstOrDefault();
                var userRole = _unitOfWork.Roles.Find(r => r.Id == 2 && r.Name == "User").FirstOrDefault();

                if (adminRole == null)
                {
                    adminRole = new Role { Id = 1, Name = "Admin" };
                    _unitOfWork.Roles.Add(adminRole);
                }

                if (userRole == null)
                {
                    userRole = new Role { Id = 2, Name = "User" };
                    _unitOfWork.Roles.Add(userRole);
                }

                _unitOfWork.Save();

                Role assignedRole = userRole;
                if (name == "admin" && email == "admin@gmail.com" && login == "admin" && password == "admin")
                {
                    assignedRole = adminRole;
                }

                var newUser = new Users
                {
                    Name = name,
                    Login = login,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleId = assignedRole.Id,
                    DateCreate = DateTime.Now
                };

                if (profilePhoto != null)
                {
                    string photoDirectory = @"C:\Users\super\Desktop\Library\UserPhotos";

                    if (!Directory.Exists(photoDirectory))
                    {
                        Directory.CreateDirectory(photoDirectory);
                    }

                    string photoPath = Path.Combine(photoDirectory, $"{Guid.NewGuid()}.jpg");

                    File.WriteAllBytes(photoPath, profilePhoto);

                    newUser.ProfilePhotoPath = photoPath;
                }

                _unitOfWork.Users.Add(newUser);
                _unitOfWork.Save();

                return "Регистрация успешно завершена!";
            }
            catch (Exception ex)
            {
                return $"Ошибка при регистрации: {ex.Message}\nВнутренняя ошибка: {ex.InnerException?.Message}";
            }
        }

        public string LoginUser(string loginOrEmail, string password)
        {
            try
            {
                var existingUser = _unitOfWork.Users
                .Find(u => u.Login == loginOrEmail || u.Email == loginOrEmail)
                .FirstOrDefault();

                if (existingUser != null)
                {
                    existingUser.Role = _unitOfWork.Roles
                        .Find(r => r.Id == existingUser.RoleId)
                        .FirstOrDefault();
                }

                if (existingUser == null)
                {
                    return "Пользователь с таким логином или email не существует.";
                }

                bool passwordMatches = BCrypt.Net.BCrypt.Verify(password, existingUser.PasswordHash);
                if (!passwordMatches)
                {
                    return "Неверный пароль.";
                }

                return existingUser.Role?.Name;
            }
            catch (Exception ex)
            {
                return $"Ошибка при авторизации: {ex.Message}\nВнутренняя ошибка: {ex.InnerException?.Message}";
            }
        }

        public List<BookDTO> GetBooks(string authorName, string genreName, string title)
        {
            using (var context = new LibraryContext())
            {
                var booksQuery = _unitOfWork.Books
                .Include(b => b.BookAuthors.Select(ba => ba.Author))
                .Include(b => b.BookGenres.Select(bg => bg.Genre))
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(authorName))
                {
                    booksQuery = booksQuery.Where(b => b.BookAuthors.Any(ba => ba.Author.Name.Contains(authorName)));
                }

                if (!string.IsNullOrWhiteSpace(genreName))
                {
                    booksQuery = booksQuery.Where(b => b.BookGenres.Any(bg => bg.Genre.Name.Contains(genreName)));
                }

                if (!string.IsNullOrWhiteSpace(title))
                {
                    booksQuery = booksQuery.Where(b => b.Name.Contains(title));
                }

                var books = booksQuery.ToList();

                var bookDTOs = books.Select(b => new BookDTO
                {
                    Name = b.Name,
                    Year = b.Year,
                    Image = b.Image,
                    Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
                }).ToList();

                return bookDTOs;
            }
        }

        public Users GetUserInfo(int userId)
        {
            return _unitOfWork.Users.GetById(userId);
        }

        public int GetUserIdByLoginOrEmail(string loginOrEmail)
        {
            var user = _unitOfWork.Users.Find(u => u.Login == loginOrEmail || u.Email == loginOrEmail).FirstOrDefault();
            return user.Id;
        }

        public void UpdateUser(int userId, string name, string login, string email, string password, byte[] photo)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user != null)
            {
                user.Name = name;
                user.Login = login;
                user.Email = email;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                }

                if (photo != null)
                {
                    string photoDirectory = @"C:\Users\super\Desktop\Library\UserPhotos";

                    if (!Directory.Exists(photoDirectory))
                    {
                        Directory.CreateDirectory(photoDirectory);
                    }

                    string photoPath = Path.Combine(photoDirectory, $"{Guid.NewGuid()}.jpg");
                    File.WriteAllBytes(photoPath, photo);

                    user.ProfilePhotoPath = photoPath;
                }

                _unitOfWork.Save();
            }
        }

        public List<UserDTO> GetUsers()
        {
            try
            {
                var users = _unitOfWork.Users.GetAll();

                var userDTOs = users.Select(u => new UserDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Login = u.Login,
                    Email = u.Email,
                    Image = u.ProfilePhotoPath,
                    DateCreate = u.DateCreate
                }).ToList();

                return userDTOs;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка получения пользователей: {ex.Message}");
            }
        }

        public void DeleteUser(int userId)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user != null)
            {
                _unitOfWork.Users.Remove(user);
                _unitOfWork.Save();
            }
            else
            {
                throw new Exception("Пользователь не найден.");
            }
        }
    }
}