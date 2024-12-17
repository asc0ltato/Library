using WCFService.Model;
using WCFService.Repository;
using System;

namespace WCFService.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Genre> Genres { get; }
        IRepository<Author> Authors { get; }
        IRepository<Book> Books { get; }
        IRepository<Users> Users { get; }
        IRepository<Review> Reviews { get; }
        IRepository<Listgetbooks> Listgetbooks { get; }
        IRepository<Role> Roles { get; }
        IRepository<BookAuthors> BookAuthors { get; } 
        IRepository<BookGenres> BookGenres { get; }
        IRepository<Sample> Samples { get; }
        int Save();
    }
}