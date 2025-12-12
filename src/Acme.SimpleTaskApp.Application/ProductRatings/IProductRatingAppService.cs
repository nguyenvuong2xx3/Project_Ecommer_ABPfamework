using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.ProductRatings.Dtos;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductRatings
{
	public interface IProductRatingAppService : IApplicationService
	{
		Task<PagedResultDto<ProductRatingDto>> GetAllRatings(GetProductRatingsInput input);

		/// <summary>
		/// Thống kê trung bình
		/// </summary>
		Task<ProductRatingStatisticsDto> GetProductRatingStatistics(int productVariantId);

		Task<ProductRatingDto> CreateRating(CreateProductRatingDto input);
		Task<ProductRatingDto> UpdateRating(UpdateProductRatingDto input);

		/// <summary>
		/// Delete rating (creator or admin)
		/// </summary>
		Task DeleteRating(int id);

		/// <summary>
		/// đánh giá có ích hay không
		/// </summary>
		Task<ProductRatingDto> VoteRatingHelpful(VoteRatingHelpfulDto input);

// user hiện tại có quyền đánh giá không 
		Task<bool> CanUserRateProduct(int productVariantId);

		/// <summary>
		/// Admin: trả lời đánh giá
		Task<ProductRatingDto> AddAdminResponse(int ratingId, string response);

		/// Admin: Approve/Reject rating
		Task ApproveRating(int id, bool isApproved);
	}
}
