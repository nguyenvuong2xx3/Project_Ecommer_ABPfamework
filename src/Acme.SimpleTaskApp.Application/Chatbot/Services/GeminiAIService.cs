using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Acme.SimpleTaskApp.Chatbot.Services
{
	/// <summary>
	/// Service to interact with Google Gemini AI API
	/// </summary>
	public interface IGeminiAIService
	{
		Task<string> GenerateResponse(string prompt, List<ChatMessage> conversationHistory = null);
		Task<string> GenerateProductRecommendation(string userQuery, string productsContext);
	}

	public class GeminiAIService : IGeminiAIService, ITransientDependency
	{
		private readonly IConfiguration _configuration;
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;
		private readonly string _baseUrl;

		public GeminiAIService(IConfiguration configuration)
		{
			_configuration = configuration;
			_apiKey = _configuration["GeminiAI:ApiKey"];
			_model = _configuration["GeminiAI:Model"] ?? "gemini-pro";
			_baseUrl = _configuration["GeminiAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
			
			_httpClient = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(30)
			};
		}

		/// <summary>
		/// Generate response from Gemini AI
		/// </summary>
		public async Task<string> GenerateResponse(string prompt, List<ChatMessage> conversationHistory = null)
		{
			try
			{
				var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

				// Build conversation context
				var contents = new List<object>();

				// Add conversation history if exists
				if (conversationHistory != null && conversationHistory.Count > 0)
				{
					foreach (var msg in conversationHistory)
					{
						contents.Add(new
						{
							role = msg.Role,
							parts = new[]
							{
								new { text = msg.Content }
							}
						});
					}
				}

				// Add current prompt
				contents.Add(new
				{
					role = "user",
					parts = new[]
					{
						new { text = prompt }
					}
				});

				var requestBody = new
				{
					contents = contents,
					generationConfig = new
					{
						temperature = 0.7,
						topK = 40,
						topP = 0.95,
						maxOutputTokens = 2048,
					}
				};

				var json = JsonConvert.SerializeObject(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(url, content);
				var responseString = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Gemini API error: {responseString}");
				}

				var result = JObject.Parse(responseString);
				var text = result["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

				return text ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
			}
			catch (Exception ex)
			{
				throw new Exception($"Error calling Gemini API: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Generate product recommendation based on user query
		/// </summary>
		public async Task<string> GenerateProductRecommendation(string userQuery, string productsContext)
		{
			var prompt = $@"
Bạn là trợ lý tư vấn sản phẩm chuyên nghiệp cho cửa hàng điện thoại và phụ kiện.

THÔNG TIN SẢN PHẨM HIỆN CÓ:
{productsContext}

YÊU CẦU CỦA KHÁCH HÀNG:
{userQuery}

Hãy phân tích yêu cầu và đưa ra lời khuyên phù hợp:
1. Hiểu rõ nhu cầu của khách (giá cả, tính năng, thương hiệu...)
2. Đề xuất 2-3 sản phẩm phù hợp nhất từ danh sách trên
3. Giải thích tại sao sản phẩm đó phù hợp
4. So sánh điểm mạnh/yếu giữa các lựa chọn
5. Đưa ra kết luận và gợi ý

Trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp.
Chỉ đề xuất sản phẩm có trong danh sách được cung cấp.
Nếu không có sản phẩm phù hợp, hãy giải thích và hỏi thêm thông tin.
";

			return await GenerateResponse(prompt);
		}
	}

	/// <summary>
	/// Chat message model
	/// </summary>
	public class ChatMessage
	{
		public string Role { get; set; } // "user" or "model"
		public string Content { get; set; }
	}
}
