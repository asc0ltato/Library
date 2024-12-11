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
        IRepository<Role> Roles { get; }
        int Save();
    }
}