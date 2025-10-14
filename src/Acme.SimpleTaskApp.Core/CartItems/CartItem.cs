using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

[Table("AppCartItem")]
public class CartItem : FullAuditedEntity
{
	public int ProductVariantId { get; set; }
	public int Quantity { get; set; }
	public int CartId { get; set; }
}
