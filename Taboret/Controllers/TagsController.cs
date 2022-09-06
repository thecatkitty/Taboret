using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taboret.Data;

namespace Taboret.Controllers
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
        public IActionResult Suggest(string query) =>
            Json(_context.SuggestTags(query)
                .Select(tag => tag.Name)
                .Take(20)
                .ToList());
    }
}
