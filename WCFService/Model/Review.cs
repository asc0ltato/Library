using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("Review")]
    public class Review
    {
        [Key]
        public int Id { get; set; }
        
        [StringLength(255)]
        public string Content { get; set; }

        [Required]
        public int Rating { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Date { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int UserId { get; set; }
        public Users User { get; set; }
    }
}