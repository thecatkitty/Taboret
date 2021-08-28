using System;
using KucykoweRodeo.Models;

namespace KucykoweRodeo.Controllers
{
    public class PublisherHandler
    {
        public delegate string ArticleConverter(Article article);

        public string Name { get; init; }
        public ArticleConverter GetPageUrl { get; init; }

        public static PublisherHandler GetFromUrl(string url)
        {
            var viewerHost = new Uri(url).Host;
            return viewerHost switch
            {
                "issuu.com" => Issuu,
                "newsstand.joomag.com" => Newsstand,
                _ => CreateDefaultHandler(viewerHost)
            };
        }

        public static PublisherHandler Issuu = new()
        {
            Name = "Issuu",
            GetPageUrl = article => $"{article.Issue.Url}/{article.Page}"
        };

        public static PublisherHandler Newsstand = new()
        {
            Name = "Newsstand",
            GetPageUrl = article => $"{article.Issue.Url}/p{article.Page}"
        };

        public static PublisherHandler CreateDefaultHandler(string url) =>
            new()
            {
                Name = new Uri(url).Host,
                GetPageUrl = article => article.Issue.Url
            };
    }
}
