using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KucykoweRodeo.Models
{
    public class Magazine
    {
        [Key]
        public string Signature { get; set; }
        public string Name { get; set; }

        public ICollection<Issue> Issues { get; set; }
    }
}
