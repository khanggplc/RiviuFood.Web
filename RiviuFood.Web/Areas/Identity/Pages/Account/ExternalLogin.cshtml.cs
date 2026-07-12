using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiviuFood.Web.Models.Entities;

namespace RiviuFood.Web.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnPost(string provider, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return RedirectToPage("./Login");
            }

            returnUrl ??= Url.Content("~/");

            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!string.IsNullOrEmpty(remoteError))
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return LocalRedirect(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return LocalRedirect(returnUrl);
        }
    }
}
