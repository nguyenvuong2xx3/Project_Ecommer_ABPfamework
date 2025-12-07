using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductRatings.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductRatings
{
	public class ProductRatingAppService : ApplicationService, IProductRatingAppService
	{
		private readonly IRepository<ProductRating, int> _ratingRepository;
		private readonly IRepository<ProductRatingHelpful, int> _helpfulRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant, int> _productVariantRepository;
		private readonly IRepository<Order, int> _orderRepository;

		public ProductRatingAppService(
				IRepository<ProductRating, int> ratingRepository,
				IRepository<ProductRatingHelpful, int> helpfulRepository,
				IRepository<User, long> userRepository,
				IRepository<Product, int> productRepository,
				IRepository<ProductVariant, int> productVariantRepository,
				IRepository<Order, int> orderRepository)
		{
			_ratingRepository = ratingRepository;
			_helpfulRepository = helpfulRepository;
			_userRepository = userRepository;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_orderRepository = orderRepository;
		}

		public async Task<PagedResultDto<ProductRatingDto>> GetAllRatings(GetProductRatingsInput input)
		{
			var query = _ratingRepository.GetAll()
					.WhereIf(input.ProductId.HasValue, r => r.ProductId == input.ProductId.Value)
					.WhereIf(input.UserId.HasValue, r => r.UserId == input.UserId.Value)
					.WhereIf(input.Rating.HasValue, r => r.Rating == input.Rating.Value)
					.WhereIf(input.IsVerifiedPurchase.HasValue, r => r.IsVerifiedPurchase == input.IsVerifiedPurchase.Value)
					.WhereIf(input.IsApproved.HasValue, r => r.IsApproved == input.IsApproved.Value);

			var totalCount = await query.CountAsync();

			var ratings = await query
					.OrderByDescending(r => r.CreationTime)
					.PageBy(input)
					.ToListAsync();

			var ratingDtos = await MapRatingsToDto(ratings);

			return new PagedResultDto<ProductRatingDto>(totalCount, ratingDtos);
		}

		public async Task<ProductRatingStatisticsDto> GetProductRatingStatistics(int productId)
		{
			var ratings = await _ratingRepository.GetAll()
					.Where(r => r.ProductId == productId && r.IsApproved)
					.ToListAsync();

			if (!ratings.Any())
			{
				return new ProductRatingStatisticsDto
				{
					ProductId = productId,
					TotalRatings = 0,
					AverageRating = 0,
					VerifiedPurchaseCount = 0,
					Distribution = new RatingDistribution()
				};
			}

			var totalRatings = ratings.Count;
			var averageRating = ratings.Average(r => r.Rating);
			var verifiedCount = ratings.Count(r => r.IsVerifiedPurchase);

			var fiveStars = ratings.Count(r => r.Rating == 5);
			var fourStars = ratings.Count(r => r.Rating == 4);
			var threeStars = ratings.Count(r => r.Rating == 3);
			var twoStars = ratings.Count(r => r.Rating == 2);
			var oneStar = ratings.Count(r => r.Rating == 1);

			return new ProductRatingStatisticsDto
			{
				ProductId = productId,
				TotalRatings = totalRatings,
				AverageRating = Math.Round(averageRating, 1),
				VerifiedPurchaseCount = verifiedCount,
				Distribution = new RatingDistribution
				{
					FiveStars = fiveStars,
					FourStars = fourStars,
					ThreeStars = threeStars,
					TwoStars = twoStars,
					OneStar = oneStar,
					FiveStarsPercent = Math.Round((double)fiveStars / totalRatings * 100, 1),
					FourStarsPercent = Math.Round((double)fourStars / totalRatings * 100, 1),
					ThreeStarsPercent = Math.Round((double)threeStars / totalRatings * 100, 1),
					TwoStarsPercent = Math.Round((double)twoStars / totalRatings * 100, 1),
					OneStarPercent = Math.Round((double)oneStar / totalRatings * 100, 1)
				}
			};
		}

		[AbpAuthorize]
		public async Task<ProductRatingDto> CreateRating(CreateProductRatingDto input)
		{
			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để đánh giá sản phẩm");
			}

			// Check if product exists
			var product = await _productRepository.GetAsync(input.ProductId);
			if (product == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại");
			}

			// Check if user already rated this product
			var existingRating = await _ratingRepository.FirstOrDefaultAsync(
					r => r.UserId == currentUserId.Value && r.ProductId == input.ProductId);

			if (existingRating != null)
			{
				throw new UserFriendlyException("Bạn đã đánh giá sản phẩm này rồi. Bạn có thể chỉnh sửa đánh giá của mình.");
			}

			// Check if user purchased this product
			var hasPurchased = await HasUserPurchasedProduct(currentUserId.Value, input.ProductId);

			var rating = new ProductRating
			{
				UserId = currentUserId.Value,
				ProductId = input.ProductId,
				OrderId = input.OrderId,
				Rating = input.Rating,
				Title = input.Title?.Trim(),
				ReviewText = input.ReviewText?.Trim(),
				ImageUrls = input.ImageUrls?.Trim(),
				IsVerifiedPurchase = hasPurchased,
				IsApproved = true, // Auto-approve by default
				HelpfulCount = 0,
				NotHelpfulCount = 0
			};

			await _ratingRepository.InsertAsync(rating);
			await CurrentUnitOfWork.SaveChangesAsync();

			var createdRating = await _ratingRepository.GetAsync(rating.Id);
			var result = await MapRatingsToDto(new List<ProductRating> { createdRating });

			return result.FirstOrDefault();
		}

		[AbpAuthorize]
		public async Task<ProductRatingDto> UpdateRating(UpdateProductRatingDto input)
		{
			var rating = await _ratingRepository.GetAsync(input.Id);

			// Check ownership
			if (rating.UserId != AbpSession.UserId)
			{
				throw new UserFriendlyException("Bạn không có quyền sửa đánh giá này");
			}

			rating.Rating = input.Rating;
			rating.Title = input.Title?.Trim();
			rating.ReviewText = input.ReviewText?.Trim();
			rating.ImageUrls = input.ImageUrls?.Trim();
			rating.IsEdited = true;
			rating.EditedTime = DateTime.Now;

			await _ratingRepository.UpdateAsync(rating);
			await CurrentUnitOfWork.SaveChangesAsync();

			var updatedRating = await _ratingRepository.GetAsync(rating.Id);
			var result = await MapRatingsToDto(new List<ProductRating> { updatedRating });

			return result.FirstOrDefault();
		}

		[AbpAuthorize]
		public async Task DeleteRating(int id)
		{
			var rating = await _ratingRepository.GetAsync(id);

			// Check permission: owner or admin
			var isAdmin = await PermissionChecker.IsGrantedAsync(PermissionNames.Pages_Roles);
			if (rating.UserId != AbpSession.UserId && !isAdmin)
			{
				throw new UserFriendlyException("Bạn không có quyền xóa đánh giá này");
			}

			// Delete associated helpful votes
			var helpfulVotes = await _helpfulRepository.GetAll()
					.Where(h => h.ProductRatingId == id)
					.ToListAsync();

			foreach (var vote in helpfulVotes)
			{
				await _helpfulRepository.DeleteAsync(vote);
			}

			await _ratingRepository.DeleteAsync(rating);
		}

		[AbpAuthorize]
		public async Task<ProductRatingDto> VoteRatingHelpful(VoteRatingHelpfulDto input)
		{
			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để vote");
			}

			var rating = await _ratingRepository.GetAsync(input.ProductRatingId);

			// Check if user already voted
			var existingVote = await _helpfulRepository.FirstOrDefaultAsync(
					h => h.UserId == currentUserId.Value && h.ProductRatingId == input.ProductRatingId);

			if (existingVote != null)
			{
				// Update existing vote
				var wasHelpful = existingVote.IsHelpful;
				existingVote.IsHelpful = input.IsHelpful;

				// Update counts
				if (wasHelpful && !input.IsHelpful)
				{
					// Changed from helpful to not helpful
					rating.HelpfulCount--;
					rating.NotHelpfulCount++;
				}
				else if (!wasHelpful && input.IsHelpful)
				{
					// Changed from not helpful to helpful
					rating.HelpfulCount++;
					rating.NotHelpfulCount--;
				}
				// If same vote, no change needed

				await _helpfulRepository.UpdateAsync(existingVote);
			}
			else
			{
				// Create new vote
				var newVote = new ProductRatingHelpful
				{
					UserId = currentUserId.Value,
					ProductRatingId = input.ProductRatingId,
					IsHelpful = input.IsHelpful
				};

				await _helpfulRepository.InsertAsync(newVote);

				// Update counts
				if (input.IsHelpful)
				{
					rating.HelpfulCount++;
				}
				else
				{
					rating.NotHelpfulCount++;
				}
			}

			await _ratingRepository.UpdateAsync(rating);
			await CurrentUnitOfWork.SaveChangesAsync();

			var updatedRating = await _ratingRepository.GetAsync(rating.Id);
			var result = await MapRatingsToDto(new List<ProductRating> { updatedRating });

			return result.FirstOrDefault();
		}

		[AbpAuthorize]
		public async Task<bool> CanUserRateProduct(int productId)
		{
			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				return false;
			}

			// Check if already rated
			var existingRating = await _ratingRepository.FirstOrDefaultAsync(
					r => r.UserId == currentUserId.Value && r.ProductId == productId);

			if (existingRating != null)
			{
				return false; // Already rated
			}

			// Check if purchased
			return await HasUserPurchasedProduct(currentUserId.Value, productId);
		}

		[AbpAuthorize(PermissionNames.Pages_Roles)]
		public async Task<ProductRatingDto> AddAdminResponse(int ratingId, string response)
		{
			var rating = await _ratingRepository.GetAsync(ratingId);

			rating.AdminResponse = response?.Trim();
			rating.AdminResponseTime = DateTime.Now;

			await _ratingRepository.UpdateAsync(rating);
			await CurrentUnitOfWork.SaveChangesAsync();

			var updatedRating = await _ratingRepository.GetAsync(rating.Id);
			var result = await MapRatingsToDto(new List<ProductRating> { updatedRating });

			return result.FirstOrDefault();
		}

		[AbpAuthorize(PermissionNames.Pages_Roles)]
		public async Task ApproveRating(int id, bool isApproved)
		{
			var rating = await _ratingRepository.GetAsync(id);
			rating.IsApproved = isApproved;
			await _ratingRepository.UpdateAsync(rating);
		}

		// Helper methods

		private async Task<bool> HasUserPurchasedProduct(long userId, int productId)
		{
			// Since OrderDetails is stored as JSON in Order.OrderDetailJson,
			// we need to deserialize and check
			var userOrders = await _orderRepository.GetAll()
					.Where(o => o.UserId == userId && o.Status == 3) // 3 = Completed
					.ToListAsync();

			foreach (var order in userOrders)
			{
				order.Deserialize();

				if (order.OrderDetails != null && order.OrderDetails.Any())
				{
					var variantIds = order.OrderDetails
							.Where(od => od.ProductVariantId.HasValue)
							.Select(od => od.ProductVariantId.Value)
							.ToList();

					if (variantIds.Any())
					{
						// Check if any of these variants belong to the product
						var hasProduct = await _productVariantRepository.GetAll()
								.AnyAsync(pv => variantIds.Contains(pv.Id) && pv.ProductId == productId);

						if (hasProduct)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private async Task<List<ProductRatingDto>> MapRatingsToDto(List<ProductRating> ratings)
		{
			var ratingDtos = new List<ProductRatingDto>();

			// Batch load users
			var userIds = ratings.Select(r => r.UserId).Distinct().ToList();
			var users = await _userRepository.GetAll()
					.Where(u => userIds.Contains(u.Id))
					.Select(u => new { u.Id, u.UserName, u.Name, u.Surname, u.EmailAddress })
					.ToListAsync();
			var userDict = users.ToDictionary(u => u.Id);

			// Batch load products
			var productIds = ratings.Select(r => r.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
					.Where(p => productIds.Contains(p.Id))
					.Select(p => new { p.Id, p.Name })
					.ToListAsync();
			var productDict = products.ToDictionary(p => p.Id);

			// Get current user's votes if logged in
			Dictionary<int, bool?> currentUserVotes = new Dictionary<int, bool?>();
			if (AbpSession.UserId.HasValue)
			{
				var ratingIds = ratings.Select(r => r.Id).ToList();
				var votes = await _helpfulRepository.GetAll()
						.Where(h => h.UserId == AbpSession.UserId.Value && ratingIds.Contains(h.ProductRatingId))
						.ToListAsync();

				currentUserVotes = votes.ToDictionary(v => v.ProductRatingId, v => (bool?)v.IsHelpful);
			}

			foreach (var rating in ratings)
			{
				// ✅ Manual mapping from ProductRating to ProductRatingDto (NO AutoMapper)
				var dto = new ProductRatingDto
				{
					Id = rating.Id,
					UserId = rating.UserId,
					ProductId = rating.ProductId,
					OrderId = rating.OrderId,
					Rating = rating.Rating,
					Title = rating.Title, // ✅ FIXED: Added Title mapping
					ReviewText = rating.ReviewText,
					IsVerifiedPurchase = rating.IsVerifiedPurchase,
					IsApproved = rating.IsApproved,
					HelpfulCount = rating.HelpfulCount,
					NotHelpfulCount = rating.NotHelpfulCount,
					AdminResponse = rating.AdminResponse,
					AdminResponseTime = rating.AdminResponseTime,
					IsEdited = rating.IsEdited,
					EditedTime = rating.EditedTime,
					CreationTime = rating.CreationTime
				};

				// Map user info
				if (userDict.TryGetValue(rating.UserId, out var user))
				{
					dto.UserName = user.UserName;
					dto.UserFullName = $"{user.Name} {user.Surname}".Trim();
					dto.UserEmail = user.EmailAddress;
				}

				// Map product info
				if (productDict.TryGetValue(rating.ProductId, out var product))
				{
					dto.ProductName = product.Name;
				}

				// Map image URLs (split comma-separated string)
				if (!string.IsNullOrEmpty(rating.ImageUrls))
				{
					dto.ImageUrls = rating.ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries)
						.Select(url => url.Trim())
						.ToList();
				}
				else
				{
					dto.ImageUrls = new List<string>();
				}

				// Map current user's vote
				dto.CurrentUserVote = currentUserVotes.ContainsKey(rating.Id)
						? currentUserVotes[rating.Id]
						: null;

				ratingDtos.Add(dto);
			}

			return ratingDtos;
		}
	}
}