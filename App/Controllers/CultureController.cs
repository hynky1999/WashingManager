using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Controller for changing a culture
/// We have to use MVC controller because we need Reponse to set a cookie
/// No way to do this with a Blazor component
/// </summary>
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    /// <summary>
    /// Sets culture as a cookie
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
            new CookieOptions {Expires = DateTimeOffset.UtcNow.AddYears(1)}
        );
        return LocalRedirect(returnUrl);
    }
}