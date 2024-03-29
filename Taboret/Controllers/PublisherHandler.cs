﻿using System;
using Taboret.Models;

namespace Taboret.Controllers
{
    public class PublisherHandler
    {
        public delegate string ArticleConverter(Article article);

        public string Name { get; init; }
        public ArticleConverter GetPageUrl { get; init; }

        public static PublisherHandler GetFromUrl(string url)
        {
            if (url.EndsWith(".pdf"))
            {
                return Pdf;
            }

            var viewerHost = new Uri(url).Host;
            return viewerHost switch
            {
                "issuu.com" => Issuu,
                "newsstand.joomag.com" => Newsstand,
                _ => CreateDefaultHandler(url)
            };
        }

        public static PublisherHandler Pdf = new()
        {
            Name = "PDF",
            GetPageUrl = article => $"{article.Issue.Url}#page={article.Page}"
        };

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
