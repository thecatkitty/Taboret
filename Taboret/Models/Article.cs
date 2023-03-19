using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Article")]
    public class Article
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        public string IssueSignature { get; set; }

        [Display(Name = "No.")]
        public uint OrdinalNumber { get; set; }

        [Display(Name = "Page")]
        public uint Page { get; set; }

        [Display(Name = "Subject")]
        public string Subject { get; set; }

        public int CategoryId { get; set; }

        [Display(Name = "Lead")]
        public string Lead { get; set; }

        [Display(Name = "Word count")]
        public uint WordCount { get; set; }

        [Display(Name = "Issue")]
        public Issue Issue { get; set; }

        [Display(Name = "Category")]
        public Category Category { get; set; }

        [Display(Name = "Authors")]
        public ICollection<Author> Authors { get; set; }

        [Display(Name = "Tags")]
        public ICollection<Tag> Tags { get; set; }
    }
}
