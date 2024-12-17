using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFService.DTO
{
    public class TakenBookDTO
    {
        public int SampleId { get; set; }
        public string BookName { get; set; }
        public int Year { get; set; }
        public string UserName { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime? DateReturn { get; set; }
    }

}