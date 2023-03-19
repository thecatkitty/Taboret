using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Category")]
    public class Category : Feature
    {
        public int Id { get; set; }
    }
}
