using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings
{
	/// <summary>
	/// Tracks which users voted a rating as helpful/not helpful
	/// Prevents users from voting multiple times on the same rating
	/// </summary>
	public class ProductRatingHelpful : CreationAuditedEntity<int>, IMayHaveTenant
	{
		public int? TenantId { get; set; }

		/// <summary>
		/// User ID who voted (NO FK)
		/// </summary>
		[Required]
		public long UserId { get; set; }

		/// <summary>
		/// Product Rating ID being voted (NO FK)
		/// </summary>
		[Required]
		public int ProductRatingId { get; set; }

		/// <summary>
		/// Is this a helpful vote (true) or not helpful (false)
		/// </summary>
		[Required]
		public bool IsHelpful { get; set; }
	}
}
