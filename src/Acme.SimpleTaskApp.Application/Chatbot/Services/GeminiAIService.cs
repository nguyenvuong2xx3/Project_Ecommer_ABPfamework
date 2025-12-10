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

			// ✅ Đọc model từ config (không validate để test)
			_model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";

			Console.WriteLine($"✅ GeminiAIService initialized with model: {_model}");

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

				Console.WriteLine("✅ GeminiChatAgent initialized successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error initializing GeminiChatAgent: {ex.Message}");
				throw new Exception($"❌ Lỗi khởi tạo GeminiChatAgent: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Generate response - SIMPLIFIED VERSION FOR TESTING
		/// </summary>
		public async Task<string> GenerateResponse(string prompt, List<ChatMessageDto> conversationHistory = null)
		{
			try
			{
				Console.WriteLine($"📤 Sending to Gemini: {prompt?.Substring(0, Math.Min(100, prompt?.Length ?? 0))}...");

				// ✅ Tạo message đơn giản
				var userMessage = new TextMessage(Role.User, prompt);

				// ✅ Gửi request qua AutoGen
				var response = await _geminiAgent.SendAsync(userMessage);

				Console.WriteLine($"📥 Received response from Gemini");

				// ✅ Extract text từ response
				var result = ExtractTextFromResponse(response);
				
				Console.WriteLine($"✅ Extracted text: {result?.Substring(0, Math.Min(100, result?.Length ?? 0))}...");

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error in GenerateResponse: {ex.Message}");
				Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
				
				if (ex.InnerException != null)
				{
					Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
				}

				return $"❌ Lỗi kết nối Gemini API: {ex.Message}";
			}
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
		/// Extract text từ IMessage response
		/// </summary>
		private string ExtractTextFromResponse(IMessage response)
		{
			if (response == null)
			{
				Console.WriteLine("⚠️ Response is null");
				return "Xin lỗi, tôi không nhận được phản hồi.";
			}

			Console.WriteLine($"📋 Response type: {response.GetType().Name}");

			// Case 1: TextMessage
			if (response is TextMessage textMessage)
			{
				Console.WriteLine("✅ Response is TextMessage");
				return textMessage.Content ?? "Xin lỗi, tôi không có câu trả lời.";
			}

			// Case 2: IMessage<string>
			if (response is IMessage<string> stringMessage)
			{
				Console.WriteLine("✅ Response is IMessage<string>");
				return stringMessage.Content ?? "Xin lỗi, tôi không có câu trả lời.";
			}

			// Case 3: Fallback - try GetContent()
			try
			{
				Console.WriteLine("⚠️ Trying GetContent()");
				var content = response.GetContent();
				Console.WriteLine($"✅ GetContent() returned: {content?.GetType().Name}");
				return content?.ToString() ?? "Xin lỗi, tôi không có câu trả lời.";
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ GetContent() failed: {ex.Message}");
				return "Xin lỗi, tôi không thể xử lý câu trả lời.";
			}
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