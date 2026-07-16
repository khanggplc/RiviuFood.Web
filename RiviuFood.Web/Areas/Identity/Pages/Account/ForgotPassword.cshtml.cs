using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using RiviuFood.Web.Models.Entities;

namespace RiviuFood.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Không tiết lộ email có tồn tại hay không (bảo mật)
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // Tạo token reset password
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code, email = Input.Email },
            protocol: Request.Scheme);

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Đặt lại mật khẩu - RiviuFood",
            $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #f97316; font-size: 28px;'>RiviuFood</h1>
                </div>
                <div style='background-color: #fff7ed; border-radius: 12px; padding: 30px; text-align: center;'>
                    <h2 style='color: #333; margin-bottom: 15px;'>Đặt lại mật khẩu</h2>
                    <p style='color: #666; margin-bottom: 25px;'>
                        Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản RiviuFood. 
                        Nhấn vào nút bên dưới để tạo mật khẩu mới.
                    </p>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}' 
                       style='display: inline-block; background-color: #f97316; color: white; padding: 14px 32px; 
                              text-decoration: none; border-radius: 25px; font-weight: bold; font-size: 16px;'>
                        Đặt lại mật khẩu
                    </a>
                    <p style='color: #999; font-size: 12px; margin-top: 25px;'>
                        Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                    </p>
                </div>
                <p style='color: #bbb; font-size: 11px; text-align: center; margin-top: 20px;'>
                    © {DateTime.Now.Year} RiviuFood. All rights reserved.
                </p>
            </div>");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
