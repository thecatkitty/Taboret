using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    public abstract class Feature
    {
        [Display(Name = "Name")]
        public string Name { get; set; }

        public string ComparableName { get; set; }

        [Display(Name = "Articles")]
        public ICollection<Article> Articles { get; set; }
    }
}
