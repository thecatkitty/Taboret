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
    public class AuthorsController : Controller
    {
        private readonly ArchiveContext _context;

        public AuthorsController(ArchiveContext context)
        {
            _context = context;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Authors
                .Include(a => a.Articles)
                .Include(a => a.Covers)
                .ToListAsync());
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Articles)
                .ThenInclude(article => article.Issue)
                .ThenInclude(issue => issue.Magazine)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Authors)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Category)
                .Include(a => a.Articles)
                .ThenInclude(article => article.Tags)
                .Include(a => a.Covers)
                .ThenInclude(cover => cover.Magazine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }
    }
}
