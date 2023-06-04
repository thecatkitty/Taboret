using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taboret.Data;
using Taboret.Models;

namespace Taboret.Controllers
{
    public class IssuesController : Controller
    {
        private readonly ArchiveContext _context;
        private readonly IWebHostEnvironment _environment;

        public IssuesController(ArchiveContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Issues
        public async Task<IActionResult> Index()
        {
            var issues = _context.Issues
                .Include(i => i.Magazine)
                .Include(i => i.Articles);
            return View(await issues.ToListAsync());
        }

        // GET: Issues/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .Include(i => i.Magazine)
                .Include(i => i.Articles)
                .Include(i => i.CoverAuthors)
                .Include("Articles.Authors")
                .Include("Articles.Category")
                .Include("Articles.Tags")
                .FirstOrDefaultAsync(m => m.Signature == id);
            if (issue == null)
            {
                return NotFound();
            }

            ViewData["IssueViewer"] = PublisherHandler.GetFromUrl(issue.Url);

            return View(issue);
        }

        // GET: Issues/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, nameof(Magazine.Signature), nameof(Magazine.Name), _context.Magazines.ToList().Last().Signature);
            return View();
        }

        // POST: Issues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("MagazineSignature,PublicationDate,CoverSignature,Url,PageCount")] Issue issue)
        {
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, nameof(Magazine.Signature), nameof(Magazine.Name), issue.MagazineSignature);

            if (!ModelState.IsValid)
            {
                return View(issue);
            }

            var signatureYear = Regex.Match(issue.CoverSignature, "\\d{4}");
            if (!signatureYear.Success)
            {
                ModelState.AddModelError(nameof(Issue.CoverSignature), "Cover signature doesn't contain a year");
                return View(issue);
            }

            var signatureOrdinal = Regex.Match(issue.CoverSignature.Replace(signatureYear.Value, string.Empty), "\\d+");
            if (!signatureOrdinal.Success)
            {
                issue.Signature = issue.MagazineSignature + signatureYear.Value;
            }
            else
            {
                issue.Signature = issue.MagazineSignature + signatureYear.Value[^2..] + signatureOrdinal.Value;
            }

            issue.UpdateTime = DateTime.UtcNow;
            _context.Add(issue);
            await _context.SaveChangesAsync();

            var id = issue.Signature;
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Issues/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .Include(i => i.Magazine)
                .Include(i => i.CoverAuthors)
                .Include(i => i.Articles).ThenInclude(a => a.Authors)
                .Include(i => i.Articles).ThenInclude(a => a.Category)
                .Include(i => i.Articles).ThenInclude(a => a.Tags)
                .FirstAsync(issue => issue.Signature == id);
            if (issue == null)
            {
                return NotFound();
            }

            ViewData["CoverAuthors"] = string.Join(", ", issue.CoverAuthors
                .Select(author => author.Name));
            ViewData["Categories"] = _context.Categories
                .AsQueryable()
                .OrderBy(category => category.ComparableName)
                .ToList();
            return View(issue);
        }

        // POST: Issues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("Signature,PublicationDate,CoverSignature,Url,PageCount,IsArchived")] Issue input, string coverAuthors)
        {
            if (id != input.Signature)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var issue = _context.Issues
                        .Include(i => i.Magazine)
                        .Include(i => i.CoverAuthors)
                        .Include(i => i.Articles).ThenInclude(a => a.Authors)
                        .Include(i => i.Articles).ThenInclude(a => a.Category)
                        .Include(i => i.Articles).ThenInclude(a => a.Tags)
                        .First(i => i.Signature == id);
                    issue.PublicationDate = input.PublicationDate;
                    issue.CoverSignature = input.CoverSignature;
                    issue.Url = input.Url;
                    issue.PageCount = input.PageCount;
                    issue.IsArchived = input.IsArchived;
                    issue.UpdateTime = DateTime.Now;

                    var (authors, unknownAuthors) = _context.GetAuthors(coverAuthors);
                    _context.Authors.AddRange(unknownAuthors);
                    unknownAuthors.ForEach(author => authors.Add(author));

                    issue.CoverAuthors
                        .Where(author => !authors.Contains(author))
                        .ToList()
                        .ForEach(author => issue.CoverAuthors.Remove(author));
                    authors
                        .Where(author => !issue.CoverAuthors.Contains(author))
                        .ToList()
                        .ForEach(author => issue.CoverAuthors.Add(author));
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IssueExists(input.Signature))
                    {
                        return NotFound();
                    }

                    throw;
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, "Signature", "Signature", input.MagazineSignature);
            return View(input);
        }

#if false
        // GET: Issues/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .Include(i => i.Magazine)
                .FirstOrDefaultAsync(m => m.Signature == id);
            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }

        // POST: Issues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var issue = await _context.Issues.FindAsync(id);
            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
#endif

        private bool IssueExists(string id)
        {
            return _context.Issues.Any(e => e.Signature == id);
        }
    }
}
