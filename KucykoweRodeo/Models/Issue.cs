using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KucykoweRodeo.Models
{
    public class Issue
    {
        public string MagazineSignature { get; set; }
        [Key]
        public string Signature { get; set; }
        public DateTime PublicationDate { get; set; }
        public string CoverSignature { get; set; }
        public string Url { get; set; }
        public uint PageCount { get; set; }
        public bool IsArchived { get; set; }
        public DateTime UpdateTime { get; set; }

        public Magazine Magazine { get; set; }
        public ICollection<Author> CoverAuthors { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
