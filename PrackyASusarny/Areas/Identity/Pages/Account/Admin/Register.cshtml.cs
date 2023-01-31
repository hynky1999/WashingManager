using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Auth.Utils;

namespace PrackyASusarny.Areas.Identity.Pages.Account.Admin;

#nullable disable
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty] public InputModel Input { get; set; }

    [TempData] public string StatusMessage { get; set; }

    public ActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
                {UserName = Input.UserName, Email = Input.Email, Cash = 0};
            var password = GeneratePassword(12);
            var result =
                await _userManager.CreateWithClaimAsync(user, password);
            if (result.Succeeded)
            {
                StatusMessage =
                    $"User's password is {password}. Please change it as soon as possible.";
                return Page();
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }

    private string GeneratePassword(int length)
    {
        string[] randomChars =
        {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase 
            "abcdefghijkmnopqrstuvwxyz", // lowercase
            "0123456789", // digits
            "!@$?_-" // non-alphanumeric
        };

        var rand = new Random(Environment.TickCount);
        return new string(Enumerable.Range(0, length).Select(_ =>
        {
            var chosenSet = randomChars[rand.Next(0, randomChars.Length)];
            return chosenSet[rand.Next(0, chosenSet.Length)];
        }).ToArray());
    }


    public class InputModel
    {
        [Microsoft.Build.Framework.Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Username")]

        public string UserName { get; set; } = "";
    }
}