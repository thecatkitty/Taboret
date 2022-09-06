using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Taboret.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IConfiguration _configuration;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }
        
        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }
        
        
        public IActionResult OnGet(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("~/", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("~/", new { ReturnUrl = returnUrl });
            }
            
            var accessToken = info.AuthenticationTokens.First(token => token.Name == "access_token");
            var discord = await DiscordHelper.GetClientAsync(accessToken.Value);
            
            var allowedGuild = await discord.GetGuildSummariesAsync()
                .Flatten()
                .AnyAsync(guild => guild.Id == ulong.Parse(_configuration["Discord:AllowGuild"]));
            if (!allowedGuild)
            {
                return RedirectToPage("./AccessDenied");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider,
                    info.ProviderKey);

                var props = new AuthenticationProperties();
                props.StoreTokens(info.AuthenticationTokens);

                await _signInManager.SignInAsync(user, props, info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("~/", new { ReturnUrl = returnUrl });
            }

            var user = new IdentityUser
            {
                UserName = info.Principal.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var props = new AuthenticationProperties();
                    props.StoreTokens(info.AuthenticationTokens);
                    props.IsPersistent = false;

                    await _signInManager.SignInAsync(user, props, info.LoginProvider);

                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
