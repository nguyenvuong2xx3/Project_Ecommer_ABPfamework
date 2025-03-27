using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems.Dtos
{
	public class CartItemListDto
	{
		public int ProductId { get; set; }

		public int CartId { get; set; }

		public string ProductName { get; set; }

		public int Quantity { get; set; }

		public decimal Price { get; set; }

		public string Image { get; set; }
	}

}
