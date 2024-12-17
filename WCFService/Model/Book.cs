using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public int Year { get; set; }
        public string Image { get; set; }
      
        public ICollection<BookAuthors> BookAuthors { get; set; }
        public ICollection<BookGenres> BookGenres { get; set; }
        public ICollection<Sample> Samples { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}