using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

[Table("AppCartItem")]
public class CartItem : FullAuditedEntity
{
	[Required]
	public int ProductId { get; set; }

	public int Quantity { get; set; }

	// Khóa ngoại tham chiếu đến Cart
	public int CartId { get; set; }

	[ForeignKey("CartId")]
	public Cart Cart { get; set; }
}
