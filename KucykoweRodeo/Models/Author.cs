using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KucykoweRodeo.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Issue> Covers { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
