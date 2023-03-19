using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Tag")]
    public class Tag : Feature
    {
        public int Id { get; set; }
    }
}
