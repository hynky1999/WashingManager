#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Areas.Manage.Pages.Account.Admin;

/// <summary>
/// This class is used to sets the active tag in the navigation bar.
/// </summary>
public static class AdminNavPages
{
    /// <summary>
    /// Register tag
    /// </summary>
    public static string Register => "Register";

    /// <summary>
    /// Register page method
    /// </summary>
    /// <param name="viewContext"></param>
    /// <returns></returns>
    public static string RegisterNavClass(ViewContext viewContext)
    {
        return PageNavClass(viewContext, Register);
    }

    /// <summary>
    /// returns active if the page
    /// </summary>
    /// <param name="viewContext">current viewContext</param>
    /// <param name="page">checked page</param>
    /// <returns>active if the current page is the same as navbar else nothing</returns>
    public static string PageNavClass(ViewContext viewContext, string page)
    {
        // Get cur page
        var activePage = viewContext.ViewData["ActivePage"] as string
                         ?? Path.GetFileNameWithoutExtension(viewContext
                             .ActionDescriptor.DisplayName);
        return string.Equals(activePage, page,
            StringComparison.OrdinalIgnoreCase)
            ? "active"
            : null;
    }
}