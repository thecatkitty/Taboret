using KucykoweRodeo.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KucykoweRodeo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ArchiveContext _context;

        public IndexModel(ILogger<IndexModel> logger, ArchiveContext context)
        {
            _logger = logger;
            _context = context;
        }

        public ArchiveContext Context => _context;

        public void OnGet()
        {

        }
    }
}
