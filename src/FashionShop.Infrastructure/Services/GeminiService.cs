using FashionShop.Application.Common.Interfaces;
using FashionShop.Infrastructure.Services.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Services
{
    public class GeminiService : IAIGenerationService
    {
        private readonly SystemConfigService _systemConfigService
            ;
        private readonly HttpClient _httpClient;
       
        public GeminiService(SystemConfigService systemConfigService, HttpClient httpClient)
        {
            
            _httpClient = httpClient;
            _systemConfigService = systemConfigService;
        }
        public async Task<string> AnalyzeTryOnAsync(string userImageBase64, string clothingImageBase64, string clothingType)
        {
            var _apiKey = _systemConfigService.GetSystemConfigValueAsync("GeminiApi");
            var _baseUrl = _systemConfigService.GetSystemConfigValueAsync("GeminiApiUrl");
            var prompt = _systemConfigService.GetSystemConfigValueAsync("GeminiTryOnPromptTemplate").ToString();

            // --- CODE MỚI: DÙNG CLASS ---
            var requestPayload = new GeminiRequest
            {
                Contents = new List<Content>
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = prompt! },
                    new Part
                    {
                        InlineData = new InlineData { MimeType = "image/jpeg", Data = userImageBase64 }
                    },
                    new Part
                    {
                        InlineData = new InlineData { MimeType = "image/jpeg", Data = clothingImageBase64 }
                    }
                }
            }
        }
            };
            // ----------------------------

            // Serialize object thành JSON
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var urlWithKey = $"{_baseUrl}?key={_apiKey}";

            var response = await _httpClient.PostAsync(urlWithKey, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {error}");
            }

            // 5. Xử lý kết quả
            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);

            // Safe navigation để tránh lỗi null nếu JSON trả về khác cấu trúc
            try
            {
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? "Không có phản hồi text.";
            }
            catch
            {
                return "Gemini không trả về kết quả đúng định dạng.";
            }
        }
    }
}
