﻿using KucykoweRodeo.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KucykoweRodeo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, ArchiveContext context)
        {
            _logger = logger;
            Context = context;
        }

        public ArchiveContext Context { get; }

        public void OnGet()
        {

        }
    }
}
