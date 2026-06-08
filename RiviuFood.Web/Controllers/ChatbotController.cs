using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RiviuFood.Web.Repositories;
using RiviuFood.Web.Models.Entities;

namespace RiviuFood.Web.Controllers
{
    public class ChatbotController(IGenericRepository<Post> postRepo, IConfiguration configuration) : Controller
    {
        private readonly IGenericRepository<Post> _postRepo = postRepo;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = new HttpClient();

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Json(new { success = false, response = "Tin nhắn trống mất rồi Boss ơi!" });
            }

            try
            {
                // 1. Lấy toàn bộ dữ liệu quán ăn/bài viết từ DB để làm "kiến thức" cho AI
                var posts = await _postRepo.GetAllAsync("Restaurant,User");

                var databaseContext = new StringBuilder();
                databaseContext.AppendLine("Dưới đây là danh sách các quán ăn có trong hệ thống RiviuFood của bạn:");
                foreach (var p in posts)
                {
                    databaseContext.AppendLine($"- Quán: {p.Restaurant?.Name}, Địa chỉ: {p.Restaurant?.Address}. Bài review: {p.Title}. Nội dung: {p.Content}");
                }

                // 2. Cấu hình API Key và Endpoint của Gemini AI (hoặc OpenAI tùy Boss chọn)
                string apiKey = _configuration["Gemini:ApiKey"] ?? "YOUR_GEMINI_API_KEY_HERE";
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

                // 3. Xây dựng Prompt nâng cao - Ép AI trả về ID bài viết liên quan theo ý tưởng của Boss
                var prompt = $@"
                Bạn là một trợ lý AI thông minh, thân thiện của website hệ thống RiviuFood do Boss Khang phát triển.
                Nhiệm vụ của bạn là dựa vào dữ liệu hệ thống được cung cấp dưới đây để gợi ý món ăn, quán ăn cho thực khách.

                {databaseContext}

                YÊU CẦU ĐẶC BIỆT KHI GỢI Ý:
                - Nếu bạn gợi ý hoặc nhắc đến một quán ăn nào có trong danh sách trên, bạn BẮT BUỘC phải chèn thêm mã định dạng ẩn phía sau tên quán đó theo cấu trúc chính xác là: {{RELATED_POST:ID_CỦA_BÀI_VIẾT}}
                Ví dụ: ""Bạn có thể ghé qua quán Phê La Cầu Giấy {{RELATED_POST:12}} để thử món Ô Long đậm vị...""
                - Tuyệt đối không tự chế ra ID nếu quán đó không có trong danh sách được cung cấp.

                Câu hỏi của người dùng: {message}
                Trả lời ngắn gọn, vui vẻ, dùng các icon món ăn cho sinh động.";

                // 4. Đóng gói dữ liệu theo cấu trúc JSON của Gemini API
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // 5. Gửi request lên Server Google AI
                var response = await _httpClient.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // 6. Bóc tách dữ liệu chữ trả về từ JSON của Gemini
                using var doc = JsonDocument.Parse(jsonResponse);
                var aiReply = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Json(new { success = true, response = aiReply });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = "AI đang bận nấu ăn chút rồi, Boss check lại API Key nhé! Lỗi: " + ex.Message });
            }
        }
    }
}