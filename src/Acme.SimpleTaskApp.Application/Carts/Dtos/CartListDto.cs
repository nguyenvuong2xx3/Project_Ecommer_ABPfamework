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
		public List<CartDto> CartItems { get; set; }	
	}
	public class CartDto
	{
		public int IdCart { get; set; }
		public int IdCartItem { get; set; }
		public int IdProductVariant { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
		public string Name { get; set; }
	}
}
