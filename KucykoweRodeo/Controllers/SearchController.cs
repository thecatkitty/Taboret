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

        [HttpPost]
        public IActionResult Index(string query)
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

            return View(articles.ToList());
        }
    }
}
