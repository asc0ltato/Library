using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("Sample")]
    public class Sample
    {
        [Key]
        public int Id { get; set; }
        public bool Presence { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public ICollection<Listgetbooks> Listgetbooks { get; set; }
    }
}