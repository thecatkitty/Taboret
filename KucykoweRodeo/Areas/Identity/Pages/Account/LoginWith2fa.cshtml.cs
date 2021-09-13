using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KucykoweRodeo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("/");
        }

        public IActionResult OnPost()
        {
            return Redirect("/");
        }
    }
}
