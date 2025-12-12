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
					.WhereIf(input.ProductVariantId.HasValue, r => r.ProductVariantId == input.ProductVariantId.Value)
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

		public async Task<ProductRatingStatisticsDto> GetProductRatingStatistics(int productVariantId)
		{
			var ratings = await _ratingRepository.GetAll()
					.Where(r => r.ProductVariantId == productVariantId && r.IsApproved)
					.ToListAsync();

			if (!ratings.Any())
			{
				return new ProductRatingStatisticsDto
				{
					ProductVariantId = productVariantId,
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
				ProductVariantId = productVariantId,
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

			// Check if product variant exists
			var productVariant = await _productVariantRepository.GetAsync(input.ProductVariantId);
			if (productVariant == null)
			{
				throw new UserFriendlyException("Biến thể sản phẩm không tồn tại");
			}

			//user đánh giá chưa?
			var existingRating = await _ratingRepository.FirstOrDefaultAsync(
					r => r.UserId == currentUserId.Value && r.ProductVariantId == input.ProductVariantId);

			if (existingRating != null)
			{
				throw new UserFriendlyException("Bạn đã đánh giá biến thể sản phẩm này rồi. Bạn có thể chỉnh sửa đánh giá của mình.");
			}

			// user mua sản phẩm chưa?
			var hasPurchased = await HasUserPurchasedProductVariant(currentUserId.Value, input.ProductVariantId);

			var rating = new ProductRating
			{
				UserId = currentUserId.Value,
				ProductVariantId = input.ProductVariantId,
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


		//HÀM VOTE ĐÁNH GIÁ CÓ ÍCH HAY KHÔNG
		[AbpAuthorize]
		public async Task<ProductRatingDto> VoteRatingHelpful(VoteRatingHelpfulDto input)
		{
			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để vote");
			}

			var rating = await _ratingRepository.GetAsync(input.ProductRatingId);

			var existingVote = await _helpfulRepository.FirstOrDefaultAsync(
					h => h.UserId == currentUserId.Value && h.ProductRatingId == input.ProductRatingId);


			// nếu tồn tại thì sửa, không thì tạo mới
			if (existingVote != null)
			{
				var wasHelpful = existingVote.IsHelpful;
				existingVote.IsHelpful = input.IsHelpful;

				if (wasHelpful && !input.IsHelpful)
				{
					rating.HelpfulCount--;
					rating.NotHelpfulCount++;
				}
				else if (!wasHelpful && input.IsHelpful)
				{
					rating.HelpfulCount++;
					rating.NotHelpfulCount--;
				}
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
		public async Task<bool> CanUserRateProduct(int productVariantId)
		{
			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				return false;
			}

			// Check if already rated
			var existingRating = await _ratingRepository.FirstOrDefaultAsync(
					r => r.UserId == currentUserId.Value && r.ProductVariantId == productVariantId);

			if (existingRating != null)
			{
				return false; // Already rated
			}

			// Check if purchased
			return await HasUserPurchasedProductVariant(currentUserId.Value, productVariantId);
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

		// check user đã mua sp
		private async Task<bool> HasUserPurchasedProductVariant(long userId, int productVariantId)
		{
			var userOrders = await _orderRepository.GetAll()
					.Where(o => o.UserId == userId && o.Status == 3) // 3 = Completed - thành công
					.ToListAsync();

			foreach (var order in userOrders)
			{
				order.Deserialize();

				if (order.OrderDetails != null && order.OrderDetails.Any())
				{
					
					var hasPurchased = order.OrderDetails.Any(od => od.ProductVariantId.HasValue && od.ProductVariantId.Value == productVariantId);

					if (hasPurchased)
					{
						return true;
					}
				}
			}

			return false;
		}

		private async Task<List<ProductRatingDto>> MapRatingsToDto(List<ProductRating> ratings)
		{
			var ratingDtos = new List<ProductRatingDto>();

			//lấy thông tin users
			var userIds = ratings.Select(r => r.UserId).Distinct().ToList();
			var users = await _userRepository.GetAll()
					.Where(u => userIds.Contains(u.Id))
					.Select(u => new { u.Id, u.UserName, u.Name, u.Surname, u.EmailAddress })
					.ToListAsync();
			var userDict = users.ToDictionary(u => u.Id);

			// lấy thông tin biến thể
			var productVariantIds = ratings.Select(r => r.ProductVariantId).Distinct().ToList();
			var productVariants = await _productVariantRepository.GetAll()
					.Where(pv => productVariantIds.Contains(pv.Id))
					.Select(pv => new { pv.Id, pv.Ram, pv.Storage, pv.Color, pv.ProductId })
					.ToListAsync();
			var productVariantDict = productVariants.ToDictionary(pv => pv.Id);

			//lấy tên thông ua sp
			var productIds = productVariants.Select(pv => pv.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
					.Where(p => productIds.Contains(p.Id))
					.Select(p => new { p.Id, p.Name })
					.ToListAsync();
			var productDict = products.ToDictionary(p => p.Id);

			//trả ra vote của currentUser
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
				var dto = new ProductRatingDto
				{
					Id = rating.Id,
					UserId = rating.UserId,
					ProductVariantId = rating.ProductVariantId,
					OrderId = rating.OrderId,
					Rating = rating.Rating,
					Title = rating.Title,
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

				//Thông tin user
				if (userDict.TryGetValue(rating.UserId, out var user))
				{
					dto.UserName = user.UserName;
					dto.UserFullName = $"{user.Name} {user.Surname}".Trim();
					dto.UserEmail = user.EmailAddress;
				}

				//Thông tin biến thể
				if (productVariantDict.TryGetValue(rating.ProductVariantId, out var productVariant))
				{
					var productName = productDict.TryGetValue(productVariant.ProductId, out var product) ? product.Name : "";
					dto.ProductVariantName = $"{productName} - {productVariant.Color} {productVariant.Ram}/{productVariant.Storage}".Trim();
				}

				//ảnh đánh giá
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
