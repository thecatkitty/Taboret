using System.Collections.Generic;

namespace Taboret.Models
{
    public abstract class Feature
    {
        public string Name { get; set; }
        public string ComparableName { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
