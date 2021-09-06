using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using KucykoweRodeo.Data;
using KucykoweRodeo.Models;
using Microsoft.EntityFrameworkCore;

namespace KucykoweRodeo.Controllers
{
    enum FeatureType
    {
        Author, Category, Issue, Tag
    };

    public class SearchController : Controller
    {
        private readonly ArchiveContext _context;

        public SearchController(ArchiveContext context)
        {
            _context = context;
        }

        public IActionResult Index(string query)
        {
            ViewData["SearchQuery"] = query;
            return View(GetArticlesFromQuery(query).ToList());
        }

        [HttpGet]
        public IActionResult Suggest(string query)
        {
            query = (query ?? "").ToLower();

            if (query.Length == 0)
            {
                return Json(GetDictionaryForJson(GetFeatures()));
            }

            var lastComma = query.LastIndexOf(',');
            var rawTags = query.Split(',', StringSplitOptions.TrimEntries);

            var titleMatches = _context.Articles
                .Include(article => article.Authors)
                .Include(article => article.Issue)
                .ThenInclude(issue => issue.Magazine)
                .Where(article => article.Title.ToLower().Contains(query) || article.Subject.ToLower().Contains(query));

            if (lastComma == -1)
            {
                return Json(GetDictionaryForJson(GetFeatures()
                            .Where(feature => feature.ComparableName.Contains(rawTags[0])), titleMatches));
            }

            var articles = GetArticlesFromQuery(query[..lastComma]).ToList();
            var term = rawTags.Last();

            var features = articles
                .SelectMany(article => article.Authors).AsEnumerable<Feature>()
                .Concat(articles.SelectMany(article => article.Tags))
                .Distinct()
                .Where(feature => !rawTags.Contains(GetPrefixedValue(feature)));

            if (term != "")
            {
                features = features
                    .Where(feature => feature.ComparableName.Contains(term));
            }

            if (!rawTags.Any(tag => tag.StartsWith("c:")))
            {
                features = features.Concat(articles
                    .Select(article => article.Category)
                    .Distinct()
                    .Where(category => category.ComparableName.Contains(term)));
            }

            return Json(GetDictionaryForJson(features, titleMatches));
        }

        private IEnumerable<Article> GetArticlesFromQuery(string query)
        {
            IEnumerable<Article> articles = _context.Articles
                .OrderBy(article => article.Issue.PublicationDate)
                .ThenBy(article => article.OrdinalNumber)
                .Include(article => article.Category)
                .Include(article => article.Issue)
                .ThenInclude(issue => issue.Magazine)
                .Include(article => article.Authors)
                .Include(article => article.Tags);
            var features = query
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(feature => feature.Length > 0)
                .Distinct()
                .ToLookup(feature => feature.Split(":")[0] switch
                {
                    "a" => FeatureType.Author,
                    "c" => FeatureType.Category,
                    "i" => FeatureType.Issue,
                    _ => FeatureType.Tag
                });

            if (features.Contains(FeatureType.Author))
            {
                var authors = features[FeatureType.Author]
                    .Select(feature => feature[2..]);
                articles = articles.Where(article =>
                    authors.All(name => article.Authors
                        .Select(author => author.ComparableName)
                        .Contains(name)));
            }

            if (features.Contains(FeatureType.Category))
            {
                var category = features[FeatureType.Category]
                    .Select(feature => feature[2..])
                    .Last();
                articles = articles
                    .Where(article => article.Category.ComparableName == category);
            }

            if (features.Contains(FeatureType.Issue))
            {
                var issueSignature = features[FeatureType.Issue]
                    .Select(feature => feature[2..])
                    .Last()
                    .ToUpper();
                articles = articles.Where(article => article.IssueSignature == issueSignature);
            }

            if (features.Contains(FeatureType.Tag))
            {
                var tags = features[FeatureType.Tag];
                articles = articles.Where(article =>
                    tags.All(tag => article.Tags
                        .Select(articleTag => articleTag.ComparableName)
                        .Contains(tag)));
            }

            return articles;
        }

        private IEnumerable<Feature> GetFeatures() =>
            _context.Authors
                .Include(author => author.Articles).AsEnumerable<Feature>()
                .Concat(_context.Categories
                    .Include(category => category.Articles))
                .Concat(_context.Tags
                    .Include(tag => tag.Articles));

        private static string GetPrefixedValue(Feature feature) => feature switch
        {
            Author => $"a:{feature.ComparableName}",
            Category => $"c:{feature.ComparableName}",
            _ => feature.ComparableName
        };

        private static Dictionary<string, object> GetDictionaryForJson(IEnumerable<Feature> features, IEnumerable<Article> articles = null)
        {
            return new Dictionary<string, object>
            {
                {
                    "features",
                    features
                        .OrderByDescending(feature => feature.Articles.Count)
                        .Take(20)
                        .Select(feature => new Dictionary<string, string>
                        {
                            { "caption", feature.Name },
                            { "value", GetPrefixedValue(feature) },
                        })
                        .ToList()
                },
                {
                    "articles",
                    articles == null
                        ? new List<object>()
                        : articles
                            .OrderByDescending(article => article.Issue.PublicationDate)
                            .ThenBy(article => article.Title)
                            .Take(20)
                            .Select(article => new Dictionary<string, object>
                            {
                                { "title", article.Title },
                                { "subject", article.Subject },
                                {
                                    "authors",
                                    article.Authors
                                        .Select(author => author.Name)
                                        .ToList()
                                },
                                { "issue", $"{article.Issue.Magazine.Name} {article.Issue.CoverSignature}" },
                                { "url", PublisherHandler.GetFromUrl(article.Issue.Url).GetPageUrl(article) }
                            })
                            .ToList()
                }
            };
        }
    }
}
