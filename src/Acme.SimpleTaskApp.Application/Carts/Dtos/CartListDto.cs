using Acme.SimpleTaskApp.CartItems.Dtos;
using Acme.SimpleTaskApp.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
	public class CartListDto
	{
		public int Id { get; set; }

		public long UserId { get; set; }

		public DateTime CreationTime { get; set; }

		public List<CartItemListDto> CartItems { get; set; }

		public List<ProductListDto> Products { get; set; }
	}
}
