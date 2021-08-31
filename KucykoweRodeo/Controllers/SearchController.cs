using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KucykoweRodeo.Data;
using KucykoweRodeo.Models;
using Microsoft.AspNetCore.Hosting;
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
                return Json(GetDictionaryForJson(
                    _context.Authors
                        .Include(author => author.Articles),
                    _context.Categories
                        .Include(category => category.Articles),
                    _context.Tags
                        .Include(tag => tag.Articles)));
            }


            var lastComma = query.LastIndexOf(',');
            var rawTags = query.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var authors = lastComma != -1
                ? GetArticlesFromQuery(query[..lastComma])
                    .SelectMany(article => article.Authors)
                    .Distinct()
                    .Where(author => author.Name.ToLower().Contains(rawTags.Last()))
                    .Where(author => !rawTags.Contains("a:" + author.Name.ToLower()))
                : _context.Authors
                    .Include(author => author.Articles)
                    .Where(author => author.Name.ToLower().Contains(rawTags.Last()));

            var categories = lastComma != -1
                ? rawTags.Any(tag => tag.StartsWith("c:"))
                    ? new List<Category>()
                    : GetArticlesFromQuery(query[..lastComma])
                        .Select(article => article.Category)
                        .Distinct()
                        .Where(category => category.Name.ToLower().Contains(rawTags.Last()))
                : _context.Categories
                    .Include(category => category.Articles)
                    .Where(category => category.Name.ToLower().Contains(rawTags.Last()));

            var tags = lastComma != -1
                ? GetArticlesFromQuery(query[..lastComma])
                    .SelectMany(article => article.Tags)
                    .Where(tag => tag.Name.ToLower().Contains(rawTags.Last()))
                    .Where(tag => !rawTags.Contains(tag.Name.ToLower()))
                    .Distinct()
                : _context.Tags
                    .Include(tag => tag.Articles)
                    .Where(tag => tag.Name.ToLower()
                        .Contains(query));
            
            return Json(GetDictionaryForJson(authors, categories, tags));
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
                .Split(',', StringSplitOptions.TrimEntries)
                .Where(feature => feature.Length > 0)
                .Select(feature => feature.ToLower())
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
                var authorIds = features[FeatureType.Author]
                    .Select(feature => int.Parse(feature[2..]));
                articles = articles.Where(article =>
                    authorIds.All(id => article.Authors
                        .Select(author => author.Id)
                        .Contains(id)));
            }

            if (features.Contains(FeatureType.Category))
            {
                var categoryId = features[FeatureType.Category]
                    .Select(feature => int.Parse(feature[2..]))
                    .Last();
                articles = articles.Where(article => article.CategoryId == categoryId);
            }

            if (features.Contains(FeatureType.Issue))
            {
                var issueSignature = features[FeatureType.Issue]
                    .Select(feature => feature[2..])
                    .Last();
                articles = articles.Where(article => article.IssueSignature == issueSignature);
            }

            if (features.Contains(FeatureType.Tag))
            {
                var tags = features[FeatureType.Tag];
                articles = articles.Where(article =>
                    tags.All(tag => article.Tags
                        .Select(articleTag => articleTag.Name.ToLower())
                        .Contains(tag)));
            }

            return articles;
        }

        private static Dictionary<string, object> GetDictionaryForJson(IEnumerable<Author> authors, IEnumerable<Category> categories, IEnumerable<Tag> tags)
        {
            return new Dictionary<string, object>
            {
                {
                    "tags",
                    tags
                        .OrderByDescending(tag => tag.Articles.Count)
                        .Take(8)
                        .Select(tag => tag.Name)
                        .ToList()
                },
                {
                    "categories",
                    categories
                        .OrderByDescending(category => category.Articles.Count)
                        .Take(8)
                        .Select(category => new Dictionary<string, object>
                        {
                            { "id", category.Id },
                            { "name", category.Name }
                        })
                        .ToList()
                },
                {
                    "authors",
                    authors
                        .OrderByDescending(category => category.Articles.Count)
                        .Take(8)
                        .Select(author => new Dictionary<string, object>
                        {
                            { "id", author.Id },
                            { "name", author.Name }
                        })
                        .ToList()
                }
            };
        }
    }
}
