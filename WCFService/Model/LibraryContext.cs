using System.Data.Entity;

namespace WCFService.Model
{
    public class LibraryContext : DbContext
    {
        public LibraryContext() : base("name=LibraryDB")
        {
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthors> BookAuthors { get; set; }
        public DbSet<BookGenres> BookGenres { get; set; }
        public DbSet<Listgetbooks> Listgetbooks { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Связь многие-ко-многим для Books и Authors
            modelBuilder.Entity<BookAuthors>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookAuthors>()
                .HasRequired(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthors>()
                .HasRequired(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Связь многие-ко-многим для Books и Genres
            modelBuilder.Entity<BookGenres>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            modelBuilder.Entity<BookGenres>()
                .HasRequired(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            modelBuilder.Entity<BookGenres>()
                .HasRequired(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Связь один-ко-многим для Reviews
            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}