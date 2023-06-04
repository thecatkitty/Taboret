using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taboret.Models
{
    [Display(Name = "Issue")]
    public class Issue
    {
        public string MagazineSignature { get; set; }

        [Key]
        public string Signature { get; set; }

        [Display(Name = "Publishing date")]
        public DateTime PublicationDate { get; set; }

        [Display(Name = "Issue number")]
        [Required(AllowEmptyStrings = false)]
        public string CoverSignature { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Number of pages")]
        public uint PageCount { get; set; }

        [Display(Name = "Archiving status")]
        public bool IsArchived { get; set; }

        [Display(Name = "Last modified")]
        public DateTime UpdateTime { get; set; }

        [Display(Name = "Magazine")]
        public Magazine Magazine { get; set; }

        [Display(Name = "Cover artists")]
        public ICollection<Author> CoverAuthors { get; set; }

        [Display(Name = "Articles")]
        public ICollection<Article> Articles { get; set; }
    }
}
