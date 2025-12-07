using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Acme.SimpleTaskApp.Chatbot.Dtos;
using Acme.SimpleTaskApp.Chatbot.Services;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;

namespace Acme.SimpleTaskApp.Chatbot
{
	/// <summary>
	/// Chatbot Application Service
	/// </summary>
	public interface IChatbotAppService : IApplicationService
	{
		Task<ChatbotResponseDto> SendMessage(SendChatMessageDto input);
		Task<List<ChatHistoryDto>> GetChatHistory(GetChatHistoryDto input);
		Task ClearChatHistory(string conversationId);
	}

	public class ChatbotAppService : ApplicationService, IChatbotAppService
	{
		private readonly IGeminiAIService _geminiAIService;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant, int> _productVariantRepository;
		private readonly IRepository<ProductImage, int> _productImageRepository;
		private readonly ICacheManager _cacheManager;

		public ChatbotAppService(
			IGeminiAIService geminiAIService,
			IRepository<Product, int> productRepository,
			IRepository<ProductVariant, int> productVariantRepository,
			IRepository<ProductImage, int> productImageRepository,
			ICacheManager cacheManager)
		{
			_geminiAIService = geminiAIService;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_productImageRepository = productImageRepository;
			_cacheManager = cacheManager;
		}

		/// <summary>
		/// Send message to chatbot and get response
		/// </summary>
		public async Task<ChatbotResponseDto> SendMessage(SendChatMessageDto input)
		{
			try
			{
				// Generate conversation ID if not exists
				if (string.IsNullOrEmpty(input.ConversationId))
				{
					input.ConversationId = Guid.NewGuid().ToString();
				}

				// Get conversation history from cache
				var historyCache = _cacheManager.GetCache("ChatHistory");
				var history = await historyCache.GetOrDefaultAsync(input.ConversationId);
				var conversationHistory = history as List<ChatHistoryDto> ?? new List<ChatHistoryDto>();

				// Build product context
				var productsContext = await BuildProductContext(input);

				// Create enhanced prompt with system context
				var systemPrompt = @"
Bạn là trợ lý tư vấn sản phẩm AI thông minh cho cửa hàng điện thoại và phụ kiện.

NHIỆM VỤ CỦA BẠN:
1. Lắng nghe và hiểu nhu cầu khách hàng
2. Đề xuất sản phẩm phù hợp từ danh sách có sẵn
3. Giải thích rõ ràng, dễ hiểu về sản phẩm
4. So sánh các lựa chọn khi cần thiết
5. Hỗ trợ quyết định mua hàng

QUY TẮC:
- Luôn trả lời bằng tiếng Việt
- Thân thiện, chuyên nghiệp
- Chỉ đề xuất sản phẩm có trong danh sách
- Nếu không hiểu, hỏi lại khách hàng
- Không bịa đặt thông tin sản phẩm
";

				var fullPrompt = $@"{systemPrompt}

DANH SÁCH SẢN PHẨM:
{productsContext}

KHÁCH HÀNG NÓI: {input.Message}

Hãy trả lời một cách tự nhiên và hữu ích.";

				// Convert history to Gemini format
				var geminiHistory = conversationHistory
					.Select(h => new ChatMessage
					{
						Role = h.Role == "user" ? "user" : "model",
						Content = h.Message
					})
					.ToList();

				// Get AI response
				var aiResponse = await _geminiAIService.GenerateResponse(fullPrompt, geminiHistory);

				// Extract product recommendations from response
				var recommendedProducts = await ExtractProductRecommendations(aiResponse, input);

				// Save to history
				conversationHistory.Add(new ChatHistoryDto
				{
					Role = "user",
					Message = input.Message,
					Timestamp = DateTime.Now
				});

				conversationHistory.Add(new ChatHistoryDto
				{
					Role = "assistant",
					Message = aiResponse,
					Timestamp = DateTime.Now,
					Products = recommendedProducts
				});

				// Update cache (expire after 30 minutes)
				await historyCache.SetAsync(
					input.ConversationId,
					conversationHistory,
					TimeSpan.FromMinutes(30)
				);

				// Generate suggested questions
				var suggestedQuestions = GenerateSuggestedQuestions(input.Message);

				return new ChatbotResponseDto
				{
					Message = aiResponse,
					RecommendedProducts = recommendedProducts,
					SuggestedQuestions = suggestedQuestions,
					ConversationId = input.ConversationId,
					Timestamp = DateTime.Now
				};
			}
			catch (Exception ex)
			{
				Logger.Error("Error in ChatbotAppService.SendMessage", ex);
				
				return new ChatbotResponseDto
				{
					Message = "Xin lỗi, tôi đang gặp chút vấn đề kỹ thuật. Vui lòng thử lại sau hoặc liên hệ bộ phận hỗ trợ.",
					ConversationId = input.ConversationId,
					Timestamp = DateTime.Now
				};
			}
		}

		/// <summary>
		/// Get chat history
		/// </summary>
		public async Task<List<ChatHistoryDto>> GetChatHistory(GetChatHistoryDto input)
		{
			var historyCache = _cacheManager.GetCache("ChatHistory");
			var history = await historyCache.GetOrDefaultAsync(input.ConversationId);
			var conversationHistory = history as List<ChatHistoryDto> ?? new List<ChatHistoryDto>();

			return conversationHistory.TakeLast(input.MaxMessages).ToList();
		}

		/// <summary>
		/// Clear chat history
		/// </summary>
		public async Task ClearChatHistory(string conversationId)
		{
			var historyCache = _cacheManager.GetCache("ChatHistory");
			await historyCache.RemoveAsync(conversationId);
		}

		/// <summary>
		/// Build product context for AI - ABP style with manual relationship loading
		/// </summary>
		private async Task<string> BuildProductContext(SendChatMessageDto input)
		{
			// Get products
			var query = _productRepository.GetAll();

			// Apply filters
			if (input.CategoryId.HasValue)
			{
				query = query.Where(p => p.CategoryId == input.CategoryId.Value);
			}

			var products = await query
				.OrderByDescending(p => p.Id)
				.Take(20) // Limit to avoid too much context
				.ToListAsync();

			if (!products.Any())
			{
				return "Hiện tại chưa có sản phẩm nào trong hệ thống.";
			}

			// Get product IDs
			var productIds = products.Select(p => p.Id).ToList();

			// Load variants for these products (manual relationship)
			var allVariants = await _productVariantRepository.GetAll()
				.Where(v => productIds.Contains(v.ProductId))
				.ToListAsync();

			// Group variants by productId
			var variantsByProduct = allVariants
				.GroupBy(v => v.ProductId)
				.ToDictionary(g => g.Key, g => g.ToList());

			var sb = new StringBuilder();
			
			foreach (var product in products)
			{
				// Get variants for this product
				var variants = variantsByProduct.ContainsKey(product.Id) 
					? variantsByProduct[product.Id] 
					: new List<ProductVariant>();

				if (!variants.Any()) continue;

				var firstVariant = variants.OrderBy(v => v.Price).FirstOrDefault();
				if (firstVariant == null) continue;

				var price = firstVariant.Price;
				var discountedPrice = firstVariant.HasActiveDiscount 
					? firstVariant.DiscountedPrice 
					: price;

				var description = product.Description ?? "";
				var descLength = Math.Min(100, description.Length);

				sb.AppendLine($@"
Sản phẩm: {product.Name}
- ID: {product.Id}
- Giá: {price:N0}₫{(firstVariant.HasActiveDiscount ? $" (Giảm còn {discountedPrice:N0}₫)" : "")}
- Mô tả: {(descLength > 0 ? description.Substring(0, descLength) + "..." : "Chưa có mô tả")}
- Màn hình: {product.Screen}
- CPU: {product.Processor}
- Camera: {product.CameraSystem}
- Pin: {product.Battery}
- Biến thể:");

				foreach (var variant in variants.Where(v => v.StockQuantity > 0).Take(3))
				{
					sb.AppendLine($"  + {variant.Ram} / {variant.Storage} / {variant.Color} - {variant.Price:N0}₫ (Còn {variant.StockQuantity} sp)");
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Extract product recommendations from AI response - ABP style
		/// </summary>
		private async Task<List<ProductRecommendationDto>> ExtractProductRecommendations(string aiResponse, SendChatMessageDto input)
		{
			var recommendations = new List<ProductRecommendationDto>();

			try
			{
				// Get all products
				var products = await _productRepository.GetAll().ToListAsync();

				foreach (var product in products)
				{
					// Check if product name is mentioned in response
					if (aiResponse.Contains(product.Name, StringComparison.OrdinalIgnoreCase))
					{
						// Get variants for this product (manual relationship)
						var variants = await _productVariantRepository.GetAll()
							.Where(v => v.ProductId == product.Id && v.StockQuantity > 0)
							.OrderBy(v => v.Price)
							.ToListAsync();

						var firstVariant = variants.FirstOrDefault();
						if (firstVariant != null)
						{
							// Get first image for this variant
							var image = await _productImageRepository.GetAll()
								.Where(i => i.ProductVariantId == firstVariant.Id)
								.OrderBy(i => i.SortOrder)
								.FirstOrDefaultAsync();

							var description = product.Description ?? "";
							var descLength = Math.Min(100, description.Length);

							recommendations.Add(new ProductRecommendationDto
							{
								Id = product.Id,
								Name = product.Name,
								Price = firstVariant.HasActiveDiscount 
									? firstVariant.DiscountedPrice 
									: firstVariant.Price,
								ImageUrl = image?.ImageUrl ?? "/img/products/default.png",
								ShortDescription = descLength > 0 ? description.Substring(0, descLength) : "Chưa có mô tả",
								Rating = firstVariant.AverageRating,
								Url = $"/HomeCustomer/DetailProductCustomer?id={product.Id}",
								VariantId = firstVariant.Id,
								Ram = firstVariant.Ram,
								Storage = firstVariant.Storage,
								Color = firstVariant.Color
							});
						}
					}
				}

				return recommendations.Take(5).ToList(); // Max 5 recommendations
			}
			catch (Exception ex)
			{
				Logger.Error("Error extracting product recommendations", ex);
				return recommendations;
			}
		}

		/// <summary>
		/// Generate suggested follow-up questions
		/// </summary>
		private List<string> GenerateSuggestedQuestions(string userMessage)
		{
			var questions = new List<string>();

			// Keyword-based suggestions
			if (userMessage.Contains("giá", StringComparison.OrdinalIgnoreCase) || 
			    userMessage.Contains("bao nhiêu", StringComparison.OrdinalIgnoreCase))
			{
				questions.Add("Sản phẩm nào có giá tốt nhất?");
				questions.Add("Có khuyến mãi gì không?");
			}

			if (userMessage.Contains("camera", StringComparison.OrdinalIgnoreCase) ||
			    userMessage.Contains("chụp ảnh", StringComparison.OrdinalIgnoreCase))
			{
				questions.Add("Sản phẩm nào có camera tốt nhất?");
				questions.Add("So sánh camera giữa các dòng máy");
			}

			if (userMessage.Contains("pin", StringComparison.OrdinalIgnoreCase) ||
			    userMessage.Contains("sạc", StringComparison.OrdinalIgnoreCase))
			{
				questions.Add("Sản phẩm nào có pin trâu nhất?");
				questions.Add("Có sạc nhanh không?");
			}

			// Default suggestions
			if (questions.Count == 0)
			{
				questions.AddRange(new[]
				{
					"Sản phẩm bán chạy nhất là gì?",
					"Có sản phẩm nào đang giảm giá không?",
					"So sánh các sản phẩm trong tầm giá này"
				});
			}

			return questions.Take(3).ToList();
		}
	}
}
