using Abp.Application.Services.Dto;

namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
    public class GetProductRatingsInput : PagedAndSortedResultRequestDto
    {
        public int? ProductId { get; set; }
        public long? UserId { get; set; }
        public int? Rating { get; set; } // Filter by star rating (1-5)
        public bool? IsVerifiedPurchase { get; set; }
        public bool? IsApproved { get; set; }
    }
}
