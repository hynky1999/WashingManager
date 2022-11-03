#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace PrackyASusarny.Areas.Identity.Pages.Account.Admin
{
    public static class AdminNavPages
    {
        public static string Register => "Register";
        public static string RegisterNavClass(ViewContext viewContext) => PageNavClass(viewContext, Register);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            // Get cur page
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}