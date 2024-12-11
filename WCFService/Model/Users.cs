using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Login { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
       
        public string PasswordHash { get; set; }
        public string ProfilePhotoPath { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime DateCreate { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<Listgetbooks> Listgetbooks { get; set; }
    }
}