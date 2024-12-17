using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFService.DTO
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime Date { get; set; }
        public string BookName { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public bool CanEdit { get; set; }
    }
}