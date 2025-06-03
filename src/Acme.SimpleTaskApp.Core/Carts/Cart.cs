using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Acme.SimpleTaskApp.Authorization.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

[Table("AppCarts")]
public class Cart : FullAuditedEntity<int>
{
	public long UserId { get; set; }

	public ICollection<CartItem> CartItems { get; set; }

	public Cart()
	{
		CreationTime = DateTime.Now;
		CartItems = new List<CartItem>();
	}
}
