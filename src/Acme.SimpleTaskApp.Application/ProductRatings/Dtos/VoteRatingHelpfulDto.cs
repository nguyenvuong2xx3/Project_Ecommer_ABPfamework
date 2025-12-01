using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
	public class VoteRatingHelpfulDto
	{
		[Required]
		public int ProductRatingId { get; set; }

		[Required]
		public bool IsHelpful { get; set; } // true = helpful, false = not helpful
	}
}
