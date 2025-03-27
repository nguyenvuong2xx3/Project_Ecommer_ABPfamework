using Acme.SimpleTaskApp.CartItems.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
	public class CreateCartInput
	{
		[Required]
		public int UserId { get; set; }
		public DateTime CreationTime { get; set; }

		public int CartId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }

		List<CartItemListDto> CartItems { get; set; }

	}
}
