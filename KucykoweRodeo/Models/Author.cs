using System.Collections.Generic;

namespace KucykoweRodeo.Models
{
    public class Author : Feature
    {
        public int Id { get; set; }

        public ICollection<Issue> Covers { get; set; }
    }
}
