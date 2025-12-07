using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Chatbot.Dtos
{
	/// <summary>
	/// DTO for sending a message to chatbot
	/// </summary>
	public class SendChatMessageDto
	{
		/// <summary>
		/// Message content from user
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Optional: Product category filter
		/// </summary>
		public int? CategoryId { get; set; }

		/// <summary>
		/// Optional: Price range filter
		/// </summary>
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }

		/// <summary>
		/// Conversation ID to maintain context
		/// </summary>
		public string ConversationId { get; set; }
	}

	/// <summary>
	/// DTO for chatbot response
	/// </summary>
	public class ChatbotResponseDto
	{
		/// <summary>
		/// Response message from chatbot
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Recommended products (if any)
		/// </summary>
		public List<ProductRecommendationDto> RecommendedProducts { get; set; }

		/// <summary>
		/// Suggested questions
		/// </summary>
		public List<string> SuggestedQuestions { get; set; }

		/// <summary>
		/// Conversation ID
		/// </summary>
		public string ConversationId { get; set; }

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp { get; set; }

		public ChatbotResponseDto()
		{
			RecommendedProducts = new List<ProductRecommendationDto>();
			SuggestedQuestions = new List<string>();
			Timestamp = DateTime.Now;
		}
	}

	/// <summary>
	/// Product recommendation info
	/// </summary>
	public class ProductRecommendationDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
		public string ShortDescription { get; set; }
		public double Rating { get; set; }
		public string Url { get; set; }
		
		// Variant info
		public int? VariantId { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Color { get; set; }
	}

	/// <summary>
	/// Chat history item
	/// </summary>
	public class ChatHistoryDto
	{
		public string Role { get; set; } // "user" or "assistant"
		public string Message { get; set; }
		public DateTime Timestamp { get; set; }
		public List<ProductRecommendationDto> Products { get; set; }

		public ChatHistoryDto()
		{
			Products = new List<ProductRecommendationDto>();
		}
	}

	/// <summary>
	/// Get chat history request
	/// </summary>
	public class GetChatHistoryDto
	{
		public string ConversationId { get; set; }
		public int MaxMessages { get; set; } = 50;
	}
}
