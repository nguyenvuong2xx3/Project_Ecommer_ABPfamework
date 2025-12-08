using Abp.Dependency;
using AutoGen.Core;
using AutoGen.Gemini;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Chatbot.Services
{
	/// <summary>
	/// Service to interact with Google Gemini AI API using AutoGen.Gemini
	/// </summary>
	public interface IGeminiAIService
	{
		Task<string> GenerateResponse(string prompt, List<ChatMessageDto> conversationHistory = null);
		Task<string> GenerateProductRecommendation(string userQuery, string productsContext);
	}

	public class GeminiAIService : IGeminiAIService, ITransientDependency
	{
		private readonly IConfiguration _configuration;
		private readonly string _apiKey;
		private readonly string _model;
		private IAgent _geminiAgent;

		public GeminiAIService(IConfiguration configuration)
		{
			_configuration = configuration;

			// ✅ Đọc API Key từ config
			_apiKey = _configuration["GeminiAI:ApiKey"];

			if (string.IsNullOrEmpty(_apiKey))
			{
				throw new Exception("❌ Chưa cấu hình GeminiAI:ApiKey trong appsettings.json");
			}

			// ✅ SỬ DỤNG MODEL ĐÚNG THEO GOOGLE API DOCS
			// Danh sách model hợp lệ (tháng 12/2024):
			// - "gemini-1.5-flash" ← Nhanh, ổn định cho free tier (15 RPM)
			// - "gemini-1.5-pro" ← Mạnh hơn nhưng rate limit thấp (2 RPM free)
			// - "gemini-pro" ← KHÔNG CÒN HỖ TRỢ, sẽ lỗi NotFound!
			
			_model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";

			// ⚠️ Validate model name
			//var validModels = new[] { "gemini-1.5-flash", "gemini-1.5-pro", "gemini-1.5-flash-latest", "gemini-1.5-pro-latest" };
			//if (!validModels.Contains(_model))
			//{
			//	throw new Exception($"❌ Model '{_model}' không hợp lệ. Dùng: gemini-1.5-flash hoặc gemini-1.5-pro");
			//}

			// Khởi tạo agent
			InitializeAgent();
		}

		/// <summary>
		/// Khởi tạo Gemini Chat Agent
		/// </summary>
		private void InitializeAgent()
		{
			var systemMessage = @"Bạn là trợ lý tư vấn sản phẩm AI thông minh cho cửa hàng điện thoại và phụ kiện.

NHIỆM VỤ:
1. Lắng nghe và hiểu nhu cầu khách hàng
2. Đề xuất sản phẩm phù hợp từ danh sách được cung cấp
3. Giải thích rõ ràng, súc tích
4. So sánh các lựa chọn khi cần

QUY TẮC:
- Luôn trả lời bằng tiếng Việt
- Thân thiện, chuyên nghiệp
- Chỉ đề xuất sản phẩm có trong danh sách context
- Không bịa đặt thông tin";

			try
			{
				_geminiAgent = new GeminiChatAgent(
						name: "product-advisor",
						model: _model,
						apiKey: _apiKey,
						systemMessage: systemMessage)
					.RegisterMessageConnector(); // ✅ Bắt buộc để xử lý message format
			}
			catch (Exception ex)
			{
				throw new Exception($"❌ Lỗi khởi tạo GeminiChatAgent: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Generate response với retry logic và error handling
		/// </summary>
		public async Task<string> GenerateResponse(string prompt, List<ChatMessageDto> conversationHistory = null)
		{
			// ✅ CƠ CHẾ RETRY CHO RATE LIMIT
			int maxRetries = 3;
			int baseDelayMs = 2000; // Chờ 2 giây giữa các lần retry

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				try
				{
					// ✅ Build full prompt với history
					var fullPrompt = BuildPromptWithHistory(prompt, conversationHistory);

					// ✅ Tạo message theo AutoGen format
					var userMessage = new TextMessage(Role.User, fullPrompt);

					// ✅ Gửi request qua AutoGen
					var response = await _geminiAgent.SendAsync(userMessage);

					// ✅ Extract text từ response
					return ExtractTextFromResponse(response);
				}
				catch (Exception ex)
				{
					// ✅ Xử lý các loại lỗi khác nhau
					var errorType = ClassifyError(ex);

					// Log lỗi (có thể thêm ILogger nếu cần)
					Console.WriteLine($"⚠️ Gemini Error (Attempt {attempt + 1}/{maxRetries}): {ex.Message}");

					// Nếu là lỗi không thể retry hoặc hết lượt thử
					if (errorType != ErrorType.RateLimit || attempt == maxRetries - 1)
					{
						return HandleFinalError(errorType, ex);
					}

					// ✅ Exponential backoff cho rate limit
					int delayMs = baseDelayMs * (int)Math.Pow(2, attempt);
					Console.WriteLine($"⏳ Chờ {delayMs}ms trước khi retry...");
					await Task.Delay(delayMs);
				}
			}

			return "❌ Hệ thống đang bận, vui lòng thử lại sau.";
		}

		/// <summary>
		/// Generate product recommendation (sử dụng lại GenerateResponse)
		/// </summary>
		public async Task<string> GenerateProductRecommendation(string userQuery, string productsContext)
		{
			var prompt = $@"
THÔNG TIN SẢN PHẨM HIỆN CÓ:
{productsContext}

YÊU CẦU CỦA KHÁCH HÀNG:
{userQuery}

Hãy tư vấn 2-3 sản phẩm phù hợp nhất. Giải thích ngắn gọn lý do.
";

			return await GenerateResponse(prompt);
		}

		// ==================== HELPER METHODS ====================

		/// <summary>
		/// Build prompt kèm history
		/// </summary>
		private string BuildPromptWithHistory(string prompt, List<ChatMessageDto> history)
		{
			if (history == null || !history.Any())
				return prompt;

			var historyText = string.Join("\n", history.Select(msg =>
				$"{(msg.Role == "user" ? "Khách hàng" : "Trợ lý")}: {msg.Content}"
			));

			return $@"
=== LỊCH SỬ HỘI THOẠI ===
{historyText}

=== CÂU HỎI MỚI ===
{prompt}
";
		}

		/// <summary>
		/// Extract text từ IMessage response
		/// </summary>
		private string ExtractTextFromResponse(IMessage response)
		{
			if (response == null)
				return "Xin lỗi, tôi không nhận được phản hồi.";

			// Case 1: TextMessage
			if (response is TextMessage textMessage)
			{
				return textMessage.Content ?? "Xin lỗi, tôi không có câu trả lời.";
			}

			// Case 2: IMessage<string>
			if (response is IMessage<string> stringMessage)
			{
				return stringMessage.Content ?? "Xin lỗi, tôi không có câu trả lời.";
			}

			// Case 3: Fallback - try GetContent()
			try
			{
				var content = response.GetContent();
				return content ?? "Xin lỗi, tôi không có câu trả lời.";
			}
			catch
			{
				return "Xin lỗi, tôi không thể xử lý câu trả lời.";
			}
		}

		/// <summary>
		/// Phân loại lỗi để xử lý phù hợp
		/// </summary>
		private ErrorType ClassifyError(Exception ex)
		{
			var message = ex.Message.ToLower();
			var innerMessage = ex.InnerException?.Message?.ToLower() ?? "";

			// ✅ Rate Limit (429, TooManyRequests, Resource exhausted)
			if (message.Contains("429") ||
			    message.Contains("toomanyrequests") ||
			    message.Contains("rate limit") ||
			    message.Contains("resource has been exhausted") ||
			    innerMessage.Contains("429"))
			{
				return ErrorType.RateLimit;
			}

			// ✅ Not Found (model không tồn tại)
			if (message.Contains("notfound") ||
			    message.Contains("404") ||
			    message.Contains("model not found") ||
			    innerMessage.Contains("notfound"))
			{
				return ErrorType.NotFound;
			}

			// ✅ Authentication (API key sai)
			if (message.Contains("unauthorized") ||
			    message.Contains("401") ||
			    message.Contains("invalid api key") ||
			    message.Contains("api key not valid"))
			{
				return ErrorType.Authentication;
			}

			// ✅ Network/Timeout
			if (message.Contains("timeout") ||
			    message.Contains("network") ||
			    message.Contains("connection"))
			{
				return ErrorType.Network;
			}

			return ErrorType.Unknown;
		}

		/// <summary>
		/// Xử lý lỗi cuối cùng (không retry được nữa)
		/// </summary>
		private string HandleFinalError(ErrorType errorType, Exception ex)
		{
			switch (errorType)
			{
				case ErrorType.NotFound:
					return $"❌ Lỗi hệ thống: Model AI không tồn tại. Vui lòng kiểm tra cấu hình model trong appsettings.json (hiện tại: {_model})";

				case ErrorType.Authentication:
					return "❌ Lỗi xác thực API Key. Vui lòng kiểm tra cấu hình GeminiAI:ApiKey";

				case ErrorType.RateLimit:
					return "⏳ Hệ thống AI đang quá tải. Vui lòng chờ 1 phút và thử lại.";

				case ErrorType.Network:
					return "🌐 Lỗi kết nối mạng. Vui lòng kiểm tra internet và thử lại.";

				default:
					return $"❌ Lỗi không xác định: {ex.Message}. Vui lòng liên hệ quản trị viên.";
			}
		}

		/// <summary>
		/// Enum phân loại lỗi
		/// </summary>
		private enum ErrorType
		{
			RateLimit,
			NotFound,
			Authentication,
			Network,
			Unknown
		}
	}

	/// <summary>
	/// Chat message DTO - Tránh conflict với Microsoft.Extensions.AI.ChatMessage
	/// </summary>
	public class ChatMessageDto
	{
		public string Role { get; set; } // "user", "assistant", "model"
		public string Content { get; set; }
	}
}