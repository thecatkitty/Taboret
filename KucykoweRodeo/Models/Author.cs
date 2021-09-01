using System.Collections.Generic;

namespace KucykoweRodeo.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ComparableName { get; set; }

        public ICollection<Issue> Covers { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
