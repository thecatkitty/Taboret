using System.Collections.Generic;

namespace KucykoweRodeo.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string IssueSignature { get; set; }
        public uint OrdinalNumber { get; set; }
        public uint Page { get; set; }
        public string Subject { get; set; }
        public int CategoryId { get; set; }
        public string Lead { get; set; }
        public uint WordCount { get; set; }

        public Issue Issue { get; set; }
        public Category Category { get; set; }
        public ICollection<Author> Authors { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
