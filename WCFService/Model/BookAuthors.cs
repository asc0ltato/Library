using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("BookAuthors")]
    public class BookAuthors
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}