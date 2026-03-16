using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email (HTML hoặc plain text).
        /// </summary>
        /// <param name="toEmail">Địa chỉ người nhận.</param>
        /// <param name="subject">Tiêu đề email.</param>
        /// <param name="htmlBody">Nội dung HTML (có thể chèn inline CSS).</param>
        /// <param name="plainTextBody">Nội dung plain text (tùy chọn, dùng khi client không hỗ trợ HTML).</param>
        Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null);
    }
}
