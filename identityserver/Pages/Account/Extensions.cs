using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace identityserver.Pages.Account;

public static class Extensions
{
    /// <summary>
    /// Renders a loading page that is used to redirect back to the client.
    /// </summary>
    public static IActionResult LoadingPage(this PageModel page, string redirectUri)
    {
        page.HttpContext.Response.StatusCode = 200;
        page.HttpContext.Response.Headers["Location"] = "";
        
        return new RedirectResult(redirectUri);
    }
}