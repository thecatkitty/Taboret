using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KucykoweRodeo.Models;

namespace KucykoweRodeo.Controllers
{
    public class PublisherHandler
    {
        public delegate string ArticleConverter(Article article);

        public string Name { get; init; }
        public ArticleConverter GetPageUrl { get; init; }
    }
}
