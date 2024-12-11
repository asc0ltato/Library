using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFService.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public DateTime DateCreate { get; set; }
    }
}