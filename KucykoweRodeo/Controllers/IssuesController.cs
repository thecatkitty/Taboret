using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KucykoweRodeo.Data;
using KucykoweRodeo.Models;

namespace KucykoweRodeo.Controllers
{
    public class IssuesController : Controller
    {
        private readonly ArchiveContext _context;

        public IssuesController(ArchiveContext context)
        {
            _context = context;
        }

        // GET: Issues
        public async Task<IActionResult> Index()
        {
            var archiveContext = _context.Issues
                .Include(i => i.Magazine)
                .Include(i => i.Articles);
            return View(await archiveContext.ToListAsync());
        }

#if false
        // GET: Issues/Details/5
        public async Task<IActionResult> Details(string id)
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
