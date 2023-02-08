// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using App.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Areas.Manage.Pages.Account.Manage;

/// <summary>
/// Model for the change password page.
/// </summary>
public class ChangePasswordModel : PageModel
{
    private readonly ILogger<ChangePasswordModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    ///  Initializes a new instance of the <see cref="ChangePasswordModel"/> class.
    /// </summary>
    /// <param name="userManager">userManager for changing user setting</param>
    /// <param name="signInManager">signInManager for signIn</param>
    /// <param name="logger"></param>
    public ChangePasswordModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ChangePasswordModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Input model for the change password page.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    /// Status message for the change password page.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>
    /// Shows page with the change password form.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(
                $"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        return Page();
    }

    /// <summary>
    /// Handles the change password form.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(
                $"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var changePasswordResult =
            await _userManager.ChangePasswordAsync(user, Input.OldPassword,
                Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User changed their password successfully");
        StatusMessage = "Your password has been changed.";
        return RedirectToPage();
    }

    /// <summary>
    /// Input model for the change password page.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// Old password to check
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary>
        /// New users password
        /// </summary>
        [Required]
        [StringLength(20,
            ErrorMessage =
                "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Repeat new password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword",
            ErrorMessage =
                "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}