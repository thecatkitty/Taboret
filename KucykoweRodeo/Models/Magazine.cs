﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KucykoweRodeo.Models
{
    public class Magazine
    {
        [Key]
        public string Signature { get; set; }
        public string Name { get; set; }

        // ReSharper disable once UnusedMember.Global
        public ICollection<Issue> Issues { get; set; }
    }
}
