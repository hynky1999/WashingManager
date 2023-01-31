#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrackyASusarny.Auth.Models;

namespace PrackyASusarny.Areas.Identity.Pages.Account.Login;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager,
        ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        if (returnUrl != null)
            return LocalRedirect(returnUrl);
        // This needs to be a redirect so that the browser performs a new
        // request and the identity for the user gets updated.
        return RedirectToPage();
    }
}