using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.ProductRatings.Dtos;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductRatings
{
	public interface IProductRatingAppService : IApplicationService
	{
		/// <summary>
		/// Get all ratings with pagination and filtering
		/// </summary>
		Task<PagedResultDto<ProductRatingDto>> GetAllRatings(GetProductRatingsInput input);

		/// <summary>
		/// Get product variant rating statistics (average, distribution)
		/// </summary>
		Task<ProductRatingStatisticsDto> GetProductRatingStatistics(int productVariantId);

		/// <summary>
		/// Create new product rating
		/// </summary>
		Task<ProductRatingDto> CreateRating(CreateProductRatingDto input);

		/// <summary>
		/// Update existing rating (only by creator)
		/// </summary>
		Task<ProductRatingDto> UpdateRating(UpdateProductRatingDto input);

		/// <summary>
		/// Delete rating (creator or admin)
		/// </summary>
		Task DeleteRating(int id);

		/// <summary>
		/// Vote rating as helpful/not helpful
		/// </summary>
		Task<ProductRatingDto> VoteRatingHelpful(VoteRatingHelpfulDto input);

		/// <summary>
		/// Check if current user can rate this product variant (must have purchased)
		/// </summary>
		Task<bool> CanUserRateProduct(int productVariantId);

		/// <summary>
		/// Admin: Add response to rating
		/// </summary>
		Task<ProductRatingDto> AddAdminResponse(int ratingId, string response);

		/// <summary>
		/// Admin: Approve/Reject rating
		/// </summary>
		Task ApproveRating(int id, bool isApproved);
	}
}
