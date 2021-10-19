using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KucykoweRodeo.Data;
using KucykoweRodeo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace KucykoweRodeo.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ArchiveContext _context;

        public ArticlesController(ArchiveContext context)
        {
            _context = context;
        }

        // POST: ArticlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,IssueSignature,Page,Subject,CategoryId,Lead,WordCount")] Article article, string authors, string tags)
        {
            if (!ModelState.IsValid) return Redirect("/Issues");

            article.OrdinalNumber = 1;

            if (_context.Articles.Any(a => a.IssueSignature == article.IssueSignature))
            {
                article.OrdinalNumber = _context.Articles
                    .AsQueryable()
                    .Where(a => a.IssueSignature == article.IssueSignature)
                    .Max(a => a.OrdinalNumber) + 1;
            }

            article.Authors ??= new HashSet<Author>();
            article.Tags ??= new HashSet<Tag>();

            var (knownAuthors, unknownAuthors) = _context.GetAuthors(authors);
            _context.Authors.AddRange(unknownAuthors);
            knownAuthors.ForEach(author => article.Authors.Add(author));
            unknownAuthors.ForEach(author => article.Authors.Add(author));

            var (knownTags, unknownTags) = _context.GetTags(tags);
            _context.Tags.AddRange(unknownTags);
            knownTags.ForEach(tag => article.Tags.Add(tag));
            unknownTags.ForEach(tag => article.Tags.Add(tag));

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(IssuesController.Edit),
                "Issues",
                new { id = article.IssueSignature },
                article.Id.ToString());
        }

        // POST: ArticlesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Page,Subject,CategoryId,Lead,WordCount")] Article input, string authors, string tags)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return Redirect("/Issues");

            try
            {
                var article = _context.Articles
                    .Include(a => a.Authors)
                    .Include(a => a.Tags)
                    .First(a => a.Id == id);

                article.Title = input.Title ?? article.Title;
                article.Subject = input.Subject ?? article.Subject;
                article.Lead = input.Lead ?? article.Lead;
                    
                if (input.Page != 0) article.Page = input.Page;
                if (input.CategoryId != 0) article.CategoryId = input.CategoryId;
                if (input.WordCount != 0) article.WordCount = input.WordCount;
                    
                var (knownAuthors, unknownAuthors) = _context.GetAuthors(authors);
                _context.Authors.AddRange(unknownAuthors);
                unknownAuthors.ForEach(author => knownAuthors.Add(author));

                article.Authors
                    .Where(author => !knownAuthors.Contains(author))
                    .ToList()
                    .ForEach(author => article.Authors.Remove(author));
                knownAuthors
                    .Where(author => !article.Authors.Contains(author))
                    .ToList()
                    .ForEach(author => article.Authors.Add(author));

                var (knownTags, unknownTags) = _context.GetTags(tags);
                _context.Tags.AddRange(unknownTags);
                unknownTags.ForEach(tag => knownTags.Add(tag));

                article.Tags
                    .Where(tag => !knownTags.Contains(tag))
                    .ToList()
                    .ForEach(tag => article.Tags.Remove(tag));
                knownTags
                    .Where(tag => !article.Tags.Contains(tag))
                    .ToList()
                    .ForEach(tag => article.Tags.Add(tag));

                await _context.SaveChangesAsync();
                return RedirectToAction(
                    nameof(IssuesController.Edit),
                    "Issues",
                    new { id = article.IssueSignature },
                    id.ToString());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(input.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }

#if false
        // GET: ArticlesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ArticlesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
#endif
        private bool ArticleExists(int id) => _context.Articles.Any(a => a.Id == id);
    }
}
