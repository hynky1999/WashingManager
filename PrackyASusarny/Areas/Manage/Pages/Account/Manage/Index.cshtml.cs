// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrackyASusarny.Auth.Models;

namespace PrackyASusarny.Areas.Manage.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
    }

    public string Username { get; set; }

    [TempData] public string StatusMessage { get; set; }

    private async Task LoadAsync(ApplicationUser applicationUser)
    {
        var userName = await _userManager.GetUserNameAsync(applicationUser);
        Username = userName;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(
                $"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        await LoadAsync(user);
        return Page();
    }
}