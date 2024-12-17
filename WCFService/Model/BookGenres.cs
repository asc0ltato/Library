using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("BookGenres")]
    public class BookGenres
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}