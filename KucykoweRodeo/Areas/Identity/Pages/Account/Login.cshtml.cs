using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KucykoweRodeo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("~/");
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("~/");
        }
    }
}
