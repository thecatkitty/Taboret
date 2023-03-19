using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Magazine")]
    public class Magazine
    {
        [Key]
        public string Signature { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        // ReSharper disable once UnusedMember.Global
        [Display(Name = "Issues")]
        public ICollection<Issue> Issues { get; set; }
    }
}
