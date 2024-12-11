using System.Collections.Generic;

namespace WCFService.DTO
{
    public class BookDTO
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Genres { get; set; }
        public string Image { get; set; }
    }
}