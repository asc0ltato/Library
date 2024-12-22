using WCFService.Model;
using WCFService.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WCFService.DTO;
using System.Data.Entity;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;

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
                if (name == "admin" && email == "admin@gmail.com" && login == "admin")
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
                return $"Ошибка при регистрации: {ex.Message}";
            }
        }

        public int GetUserIdByLoginOrEmail(string loginOrEmail)
        {
            var user = _unitOfWork.Users.Find(u => u.Login == loginOrEmail || u.Email == loginOrEmail).FirstOrDefault();
            return user.Id;
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
                return $"Ошибка при авторизации: {ex.Message}";
            }
        }
        //-----------------------------------------------------------------------------------------------------
        public List<string> GetGenres()
        {
            var genres = _unitOfWork.Genres.GetAll().Select(g => g.Name).ToList();
            return genres;
        }

        public List<BookDTO> GetBooks(string authorName, string genreName, string title)
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

            return books.Select(b => new BookDTO
            {
                Name = b.Name,
                Year = b.Year,
                Image = b.Image,
                Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
            }).ToList();
        }

        public List<BookDTO> GetAllBooks()
        {
            var books = _unitOfWork.Books
                .Include(b => b.BookAuthors.Select(ba => ba.Author))
                .Include(b => b.BookGenres.Select(bg => bg.Genre))
                .Include(b => b.Samples)
                .ToList();

            var bookDTOs = books.Select(b => new BookDTO
            {
                Id = b.Id,
                Name = b.Name,
                Year = b.Year,
                Image = b.Image,
                Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                SampleId = b.Samples.FirstOrDefault()?.Id ?? 0,
                SampleCount = b.Samples.Count, 
                Presence = b.Samples.Any(s => s.Presence) 
            }).ToList();

            return bookDTOs;
        }

        public List<BookDTO> GetBooksByPage(int page)
        {
            var books = _unitOfWork.Books
                .Include(b => b.BookAuthors.Select(ba => ba.Author))
                .Include(b => b.BookGenres.Select(bg => bg.Genre))
                .Include(b => b.Samples)
                .ToList();

            var bookDTOs = books.Select(b => new BookDTO
            {
                Id = b.Id,
                Name = b.Name,
                Year = b.Year,
                Image = b.Image,
                Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                SampleId = b.Samples.FirstOrDefault()?.Id ?? 0,
                SampleCount = b.Samples.Count,
                Presence = b.Samples.Any(s => s.Presence)
            })
                .Skip((page - 1) * 4)
                .Take(4)
                .ToList();

            return bookDTOs;
        }

        public List<BookDTO> GetFilteredBooksByPage(string authorName, string genreName, string title, int page)
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

            return books.Select(b => new BookDTO
            {
                Name = b.Name,
                Year = b.Year,
                Image = b.Image,
                Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
            })
                .Skip((page - 1) * 4)
                .Take(4)
                .ToList();
        }

        public void AddBook(BookDTO bookDTO)
        {
            var existingAuthors = _unitOfWork.Authors
                .Find(a => bookDTO.Authors.Contains(a.Name))
                .ToList();

            var newAuthors = bookDTO.Authors
                .Where(authorName => !existingAuthors.Any(ea => ea.Name == authorName))
                .Select(authorName => new Author { Name = authorName })
                .ToList();

            foreach (var newAuthor in newAuthors)
            {
                _unitOfWork.Authors.Add(newAuthor);
            }

            var existingGenres = _unitOfWork.Genres
                .Find(g => bookDTO.Genres.Contains(g.Name))
                .ToList();

            var newGenres = bookDTO.Genres
                .Where(genreName => !existingGenres.Any(eg => eg.Name == genreName))
                .Select(genreName => new Genre { Name = genreName })
                .ToList();

            foreach (var newGenre in newGenres)
            {
                _unitOfWork.Genres.Add(newGenre);
            }

            var newBook = new Book
            {
                Name = bookDTO.Name,
                Year = bookDTO.Year,
                Image = bookDTO.Image,

                BookAuthors = existingAuthors
                    .Concat(newAuthors)
                    .Select(author => new BookAuthors { Author = author })
                    .ToList(),

                BookGenres = existingGenres
                    .Concat(newGenres)
                    .Select(genre => new BookGenres { Genre = genre })
                    .ToList(),

                Samples = Enumerable.Range(0, bookDTO.SampleCount).Select(i => new Sample
                {
                    Presence = true
                }).ToList()
            };

            _unitOfWork.Books.Add(newBook);
            _unitOfWork.Save();
        }

        public void UpdateBook(BookDTO bookDto)
        {
            try
            {
                var book = _unitOfWork.Books.GetById(bookDto.Id);

                if (book == null)
                {
                    throw new FaultException("Книга с указанным ID не найдена.");
                }

                book.Name = bookDto.Name;
                book.Year = bookDto.Year;

                if (!string.IsNullOrWhiteSpace(bookDto.Image))
                {
                    book.Image = bookDto.Image;
                }

                if (bookDto.Authors != null && bookDto.Authors.Any())
                {
                    var existingBookAuthors = _unitOfWork.BookAuthors.Find(ba => ba.BookId == book.Id).ToList();
                    foreach (var bookAuthor in existingBookAuthors)
                    {
                        _unitOfWork.BookAuthors.Remove(bookAuthor);
                    }

                    foreach (var authorName in bookDto.Authors)
                    {
                        var author = _unitOfWork.Authors.Find(a => a.Name == authorName).FirstOrDefault();
                        if (author == null)
                        {
                            author = new Author { Name = authorName };
                            _unitOfWork.Authors.Add(author);
                            _unitOfWork.Save();
                        }

                        _unitOfWork.BookAuthors.Add(new BookAuthors
                        {
                            BookId = book.Id,
                            AuthorId = author.Id
                        });
                    }
                }

                if (bookDto.Genres != null && bookDto.Genres.Any())
                {
                    var existingBookGenres = _unitOfWork.BookGenres.Find(bg => bg.BookId == book.Id).ToList();
                    foreach (var bookGenre in existingBookGenres)
                    {
                        _unitOfWork.BookGenres.Remove(bookGenre);
                    }

                    foreach (var genreName in bookDto.Genres)
                    {
                        var genre = _unitOfWork.Genres.Find(g => g.Name == genreName).FirstOrDefault();
                        if (genre == null)
                        {
                            genre = new Genre { Name = genreName };
                            _unitOfWork.Genres.Add(genre);
                            _unitOfWork.Save();
                        }

                        _unitOfWork.BookGenres.Add(new BookGenres
                        {
                            BookId = book.Id,
                            GenreId = genre.Id
                        });
                    }
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при обновлении книги: {ex.Message}");
            }
        }

        public string DeleteBook(int bookId)
        {
            try
            {
                var book = _unitOfWork.Books.GetById(bookId);

                if (book == null)
                {
                    return "Книга с указанным ID не найдена.";
                }

                var hasSamplesOnLoan = _unitOfWork.Listgetbooks
                .Include(l => l.Sample)
                .Any(l => l.Sample.BookId == bookId && l.DateReturn == null);

                if (hasSamplesOnLoan)
                {
                    return "Невозможно удалить книгу, так как она находится на руках у пользователей.";
                }

                var bookAuthors = _unitOfWork.BookAuthors.Find(ba => ba.BookId == bookId).ToList();
                foreach (var bookAuthor in bookAuthors)
                {
                    _unitOfWork.BookAuthors.Remove(bookAuthor);

                    var isAuthorUsed = _unitOfWork.BookAuthors.Any(ba => ba.AuthorId == bookAuthor.AuthorId);
                    if (!isAuthorUsed)
                    {
                        var author = _unitOfWork.Authors.GetById(bookAuthor.AuthorId);
                        if (author != null)
                        {
                            _unitOfWork.Authors.Remove(author);
                        }
                    }
                }

                var bookGenres = _unitOfWork.BookGenres.Find(bg => bg.BookId == bookId).ToList();
                foreach (var bookGenre in bookGenres)
                {
                    _unitOfWork.BookGenres.Remove(bookGenre);

                    var isGenreUsed = _unitOfWork.BookGenres.Any(bg => bg.GenreId == bookGenre.GenreId);
                    if (!isGenreUsed)
                    {
                        var genre = _unitOfWork.Genres.GetById(bookGenre.GenreId);
                        if (genre != null)
                        {
                            _unitOfWork.Genres.Remove(genre);
                        }
                    }
                }

                var samples = _unitOfWork.Samples.Find(s => s.BookId == bookId).ToList();
                foreach (var sample in samples)
                {
                    _unitOfWork.Samples.Remove(sample);
                }

                _unitOfWork.Books.Remove(book);
                _unitOfWork.Save();
                return "Книга успешно удалена.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при удалении книги: {ex.Message}";
            }
        }


        public BookDTO GetBookById(int bookId)
        {
            var book = _unitOfWork.Books
                .Include(b => b.BookAuthors.Select(ba => ba.Author))
                .Include(b => b.BookGenres.Select(bg => bg.Genre))
                .FirstOrDefault(b => b.Id == bookId);

            return new BookDTO
            {
                Id = book.Id,
                Name = book.Name,
                Year = book.Year,
                Image = book.Image,
                Authors = book.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                Genres = book.BookGenres.Select(bg => bg.Genre.Name).ToList()
            };
        }
        //-----------------------------------------------------------------------------------------------------
        public List<UserDTO> GetUsers()
        {
            try
            {
                var users = _unitOfWork.Users.Find(u => u.RoleId == 2);

                if (users == null || !users.Any())
                {
                    throw new Exception("Таблица пользователей пуста или данные не загружены.");
                }

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

        public Users GetUserInfo(int userId)
        {
            return _unitOfWork.Users.GetById(userId);
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

        public string DeleteUser(int userId)
        {
            try
            {
                bool hasBooksOnHand = _unitOfWork.Listgetbooks
                    .Find(l => l.UsersId == userId && l.DateReturn == null)
                    .Any();

                if (hasBooksOnHand)
                {
                    return "Невозможно удалить профиль, так как у пользователя есть взятые книги.";
                }

                var user = _unitOfWork.Users.GetById(userId);
                if (user != null)
                {
                    _unitOfWork.Users.Remove(user);
                    _unitOfWork.Save();
                    return "Профиль пользователя успешно удален.";
                }

                return "Пользователь не найден.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при удалении пользователя: {ex.Message}";
            }
        }

        //-----------------------------------------------------------------------------------------------------

        public List<TakenBookDTO> GetTakenBooksByUserId(int userId)
        {
            try
            {
                var takenBooks = _unitOfWork.Listgetbooks
                    .Include(l => l.Sample)
                    .Include(l => l.Sample.Book)
                    .Include(l => l.Users) 
                    .Where(l => l.UsersId == userId)
                    .ToList();

                return takenBooks.Select(l => new TakenBookDTO
                {
                    SampleId = l.SampleId,
                    BookName = l.Sample.Book.Name,
                    Year = l.Sample.Book.Year,
                    UserName = l.Users.Login, 
                    DateTaken = l.DateTaken,
                    DateReturn = l.DateReturn 
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при получении списка книг: {ex.Message}");
            }
        }


        public List<TakenBookDTO> GetAllTakenBooks()
        {
            try
            {
                var takenBooks = _unitOfWork.Listgetbooks
                    .Include(l => l.Sample)
                    .Include(l => l.Sample.Book)
                    .Include(l => l.Users)
                    .ToList();

                return takenBooks.Select(l => new TakenBookDTO
                {
                    SampleId = l.SampleId,
                    BookName = l.Sample.Book.Name,
                    Year = l.Sample.Book.Year,
                    UserName = l.Users.Login,
                    DateTaken = l.DateTaken
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при получении списка взятых книг: {ex.Message}");
            }
        }


        public string TakeBook(int userId, int bookId)
        {
            try
            {
                var availableSample = _unitOfWork.Samples
                    .Find(s => s.BookId == bookId && s.Presence == true)
                    .FirstOrDefault();
               
                if (availableSample == null)
                {
                    return "Нет доступных экземпляров книги.";
                }

                var newRecord = new Listgetbooks
                {
                    UsersId = userId,
                    SampleId = availableSample.Id,
                    DateTaken = DateTime.Now, 
                    DateReturn = null
                };

                availableSample.Presence = false;

                _unitOfWork.Listgetbooks.Add(newRecord);
                _unitOfWork.Save();

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при взятии книги: {ex.Message}");
            }
        }

        public string ReturnBook(int userId, int sampleId)
        {
            try
            {
                var listEntry = _unitOfWork.Listgetbooks
                    .Find(l => l.UsersId == userId && l.SampleId == sampleId && l.DateReturn == null)
                    .FirstOrDefault();

                if (listEntry == null)
                {
                    return "Вы не брали этот экземпляр книги.";
                }

                var sample = _unitOfWork.Samples
                    .Find(s => s.Id == sampleId)
                    .FirstOrDefault();

                if (sample == null)
                {
                    return "Экземпляр книги не найден.";
                }

                sample.Presence = true;
                _unitOfWork.Listgetbooks.Remove(listEntry);
                _unitOfWork.Save();
                return "Книга успешно возвращена.";
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при возвращении книги: {ex.Message}");
            }
        }

        public bool HasUserTakenBook(int userId, int sampleId)
        {
            return _unitOfWork.Listgetbooks.Any(l => l.UsersId == userId && l.SampleId == sampleId);
        }
        //-----------------------------------------------------------------------------------------------------
        public List<ReviewDTO> GetAllReviews(int currentUserId)
        {
            var reviews = _unitOfWork.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToList();

            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                BookName = r.Book.Name,
                UserName = r.User.Login,
                UserId = r.UserId,
                Rating = r.Rating,
                Content = r.Content,
                Date = r.Date,
                CanEdit = r.UserId == currentUserId
            }).ToList();
        }

        public List<ReviewDTO> GetReviews()
        {
            var reviews = _unitOfWork.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToList();

            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                BookName = r.Book.Name,
                UserName = r.User.Login,
                UserId = r.UserId,
                Rating = r.Rating,
                Content = r.Content,
                Date = r.Date
            }).ToList();
        }

        public ReviewDTO GetReviewById(int reviewId, int _currentUserId)
        {
            var review = _unitOfWork.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == reviewId);

            if (review == null)
            {
                throw new Exception("Отзыв не найден");
            }

            return new ReviewDTO
            {
                Id = review.Id,
                Content = review.Content,
                Rating = review.Rating,
                Date = review.Date,
                UserId = review.UserId,
                BookName = review.Book?.Name,
                UserName = review.User?.Login,
                CanEdit = review.UserId == _currentUserId
            };
        }

        public string AddBookReview(int userId, int bookId, string review, int rating)
        {
            try
            {
                var hasTakenBook = _unitOfWork.Listgetbooks
                    .Include(l => l.Sample)
                    .Any(l => l.UsersId == userId && l.Sample.BookId == bookId);

                if (!hasTakenBook)
                {
                    return "Вы не можете оставить отзыв, так как вы не брали эту книгу.";
                }

                var bookReview = new Review
                {
                    UserId = userId,
                    BookId = bookId,
                    Content = review,
                    Rating = rating,
                    Date = DateTime.Now
                };

                _unitOfWork.Reviews.Add(bookReview);
                _unitOfWork.Save();

                return string.Empty; 
            }
            catch (Exception ex)
            {
                return $"Ошибка при добавлении отзыва: {ex.Message}";
            }
        }

        public void UpdateReview(int reviewId, string content, int rating)
        {
            var review = _unitOfWork.Reviews.Find(r => r.Id == reviewId).FirstOrDefault();

            if (review == null)
                throw new FaultException("Отзыв не найден.");

            review.Content = content;
            review.Rating = rating;
            review.Date = DateTime.Now; 

            _unitOfWork.Save();
        }

        public void DeleteReview(int reviewId, int userId)
        {
            var review = _unitOfWork.Reviews.Find(r => r.Id == reviewId && r.UserId == userId).FirstOrDefault();

            if (review == null)
                throw new FaultException("Отзыв не найден или у вас нет прав для его удаления.");

            _unitOfWork.Reviews.Remove(review);
            _unitOfWork.Save();
        }

        public void AdminDeleteReview(int reviewId)
        {
            try
            {
                var review = _unitOfWork.Reviews.GetById(reviewId);

                if (review == null)
                {
                    throw new FaultException("Отзыв с указанным ID не найден.");
                }

                _unitOfWork.Reviews.Remove(review);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Ошибка при удалении отзыва: {ex.Message}");
            }
        }
    }
}