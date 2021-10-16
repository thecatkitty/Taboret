using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KucykoweRodeo.Data;
using System;
using System.Collections.Generic;
using KucykoweRodeo.Models;

namespace KucykoweRodeo.Controllers
{
    public class TagsController : Controller
    {
        private readonly ArchiveContext _context;

        public TagsController(ArchiveContext context)
        {
            _context = context;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tags
                .Include(t => t.Articles)
                .ToListAsync());
        }

        // GET: Tags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .Include(t => t.Articles)
                .ThenInclude(article => article.Issue)
                .ThenInclude(issue => issue.Magazine)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Authors)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Category)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            ViewData["ArticleListColumn"] = "issue";
            return View(tag);
        }

        [HttpGet]
        public IActionResult Suggest(string query)
        {
            query = (query ?? "").ToLower();
            
            var rawTags = query.Split(',', StringSplitOptions.TrimEntries);
            var term = rawTags.Last();

            IQueryable<Tag> tags = _context.Tags
                .AsQueryable()
                .OrderByDescending(tag => tag.Articles.Count);

            if (rawTags.Length > 1)
            {
                var excluded = rawTags[..^1];
                tags = tags.Where(tag => !excluded.Contains(tag.ComparableName));
            }

            if (term.Length != 0)
            {
                tags = tags.Where(tag => tag.ComparableName.Contains(rawTags.Last()));
            }

            return Json(tags
                .Select(tag => tag.Name)
                .Take(20)
                .ToList());
        }
    }
}
