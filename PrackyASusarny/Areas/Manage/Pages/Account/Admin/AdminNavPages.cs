#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace PrackyASusarny.Areas.Manage.Pages.Account.Admin;

public static class AdminNavPages
{
    public static string Register => "Register";

    public static string RegisterNavClass(ViewContext viewContext)
    {
        return PageNavClass(viewContext, Register);
    }

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