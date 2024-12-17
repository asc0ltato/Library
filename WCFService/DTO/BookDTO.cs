using System.Collections.Generic;

namespace WCFService.DTO
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Genres { get; set; }
        public string Image { get; set; }
        public int SampleId { get; set; }
        public int SampleCount { get; set; }
        public bool Presence { get; set; }
    }
}