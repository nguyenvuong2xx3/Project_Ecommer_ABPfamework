using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class OrderDetailDto
	{
		public int ProductId { get; set; }

		public string ProductName { get; set; } // Name of the product
		public string ImageUrl { get; set; } // URL of the product image
		public decimal NewPrice { get; set; } // Price at the time of order

		public int Quantity { get; set; } // Quantity ordered

		public decimal TotalUnit { get; set; }
	}
}
