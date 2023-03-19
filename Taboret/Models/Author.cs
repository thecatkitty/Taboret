using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Author")]
    public class Author : Feature
    {
        public int Id { get; set; }

        [Display(Name = "Designed covers")]
        public ICollection<Issue> Covers { get; set; }
    }
}
