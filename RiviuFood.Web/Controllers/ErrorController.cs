using Microsoft.AspNetCore.Mvc;

namespace RiviuFood.Web.Controllers
{
    public class ErrorController : Controller
    {
        // Điều hướng đến giao diện lỗi 403 (Từ chối truy cập)
        [HttpGet]
        [Route("Error/403")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}