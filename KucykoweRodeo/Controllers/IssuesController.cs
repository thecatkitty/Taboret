using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KucykoweRodeo.Data;
using Microsoft.AspNetCore.Hosting;

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

            var coverPath = "/coth/" + issue.Signature + ".png";
            if (System.IO.File.Exists(Path.Combine(_environment.ContentRootPath, "assets") + coverPath))
            {
                ViewData["CoverPath"] = coverPath;
            }

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

        // GET: Issues/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, "Signature", "Signature", issue.MagazineSignature);
            return View(issue);
        }

        // POST: Issues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MagazineSignature,Signature,PublicationDate,CoverSignature,Url,PageCount,IsArchived,UpdateTime")] Issue issue)
        {
            if (id != issue.Signature)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(issue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IssueExists(issue.Signature))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MagazineSignature"] = new SelectList(_context.Magazines, "Signature", "Signature", issue.MagazineSignature);
            return View(issue);
        }

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

        private bool IssueExists(string id)
        {
            return _context.Issues.Any(e => e.Signature == id);
        }
#endif
    }
}
