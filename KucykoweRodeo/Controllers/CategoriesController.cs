using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KucykoweRodeo.Data;

namespace KucykoweRodeo.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ArchiveContext _context;

        public CategoriesController(ArchiveContext context)
        {
            _context = context;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
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

            var tag = await _context.Categories
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
    }
}
