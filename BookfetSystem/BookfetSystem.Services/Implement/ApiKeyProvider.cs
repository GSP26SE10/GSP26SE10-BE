using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        private readonly List<string> _apiKeys;
        private readonly Random _random;

        public ApiKeyProvider(IConfiguration configuration)
        {
            // Lấy danh sách keys từ appsettings
            _apiKeys = configuration.GetSection("Gemini:ApiKeys").Get<List<string>>()
                       ?? new List<string>();

            if (!_apiKeys.Any())
                throw new Exception("Không tìm thấy ApiKeys trong cấu hình!");

            _random = new Random();
        }

        public string GetRandomKey()
        {
            // Chọn ngẫu nhiên 1 index trong mảng
            int index = _random.Next(_apiKeys.Count);
            return _apiKeys[index];
        }
    }
}
