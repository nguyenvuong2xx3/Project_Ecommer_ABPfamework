using Acme.SimpleTaskApp.CartItems.Dtos;
using System.Collections.Generic;
using System;
using Acme.SimpleTaskApp.Carts.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Carts
{
	public class CartViewModel
	{
		
		public int Id { get; set; }
		public long UserId { get; set; }

		public DateTime CreationTime { get; set; }

		public List<CartItemListDto> CartItems { get; set; }
	}
}
