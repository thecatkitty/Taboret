using System.Collections.Generic;

namespace Taboret.Models
{
    public class Author : Feature
    {
        public int Id { get; set; }

        public ICollection<Issue> Covers { get; set; }
    }
}
