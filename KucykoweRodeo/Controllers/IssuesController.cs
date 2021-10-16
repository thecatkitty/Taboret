using KucykoweRodeo.Data;
using KucykoweRodeo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KucykoweRodeo.Controllers
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

            SetCoverPath(issue);

            var viewerHost = new Uri(issue.Url).Host;
            ViewData["IssueViewer"] = viewerHost switch
            {
                "issuu.com" => PublisherHandler.Issuu,
                "newsstand.joomag.com" => PublisherHandler.Newsstand,
                _ => PublisherHandler.CreateDefaultHandler(viewerHost)
            };

            return View(issue);
        }

#if false
        // GET: Issues/Create
        public IActionResult Create()
        {
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, "Signature", "Signature");
            return View();
        }

        // POST: Issues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MagazineSignature,Signature,PublicationDate,CoverSignature,Url,PageCount,IsArchived,UpdateTime")] Issue issue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(issue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, "Signature", "Signature", issue.MagazineSignature);
            return View(issue);
        }
#endif

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

            SetCoverPath(issue);

            ViewData["CoverAuthors"] = string.Join(", ", issue.CoverAuthors
                .Select(author => author.Name));
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
                        .Include(i => i.CoverAuthors)
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

        private void SetCoverPath(Issue issue)
        {
            var coverPath = "/coth/" + issue.Signature + ".png";
            if (System.IO.File.Exists(Path.Combine(_environment.ContentRootPath, "assets") + coverPath))
            {
                ViewData["CoverPath"] = coverPath;
            }
        }
    }
}
