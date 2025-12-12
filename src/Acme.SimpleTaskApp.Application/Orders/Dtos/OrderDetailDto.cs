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

		public string ProductName { get; set; } 
		public string ImageUrl { get; set; } 
		public decimal NewPrice { get; set; }

		public int Quantity { get; set; }

		public decimal TotalUnit { get; set; }
	}
}
