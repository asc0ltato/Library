using System.Web.Security;
using WCFService.Model;
using WCFService.Repository;

namespace WCFService.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryContext _context;

        public UnitOfWork(LibraryContext context)
        {
            _context = context;
            Genres = new Repository<Genre>(_context);
            Authors = new Repository<Author>(_context);
            Books = new Repository<Book>(_context);
            Users = new Repository<Users>(_context);
            Reviews = new Repository<Review>(_context);
            Roles = new Repository<Role>(_context);
            Samples = new Repository<Sample>(_context);
            Listgetbooks = new Repository<Listgetbooks>(_context);
            BookAuthors = new Repository<BookAuthors>(_context);
            BookGenres = new Repository<BookGenres>(_context);
        }

        public IRepository<Genre> Genres { get; private set; }
        public IRepository<Author> Authors { get; private set; }
        public IRepository<Book> Books { get; private set; }
        public IRepository<Users> Users { get; private set; }
        public IRepository<Review> Reviews { get; private set; }
        public IRepository<Role> Roles { get; private set; }
        public IRepository<Sample> Samples { get; private set; }
        public IRepository<Listgetbooks> Listgetbooks { get; private set; }
        public IRepository<BookAuthors> BookAuthors { get; private set; } 
        public IRepository<BookGenres> BookGenres { get; private set; }
        public int Save()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}