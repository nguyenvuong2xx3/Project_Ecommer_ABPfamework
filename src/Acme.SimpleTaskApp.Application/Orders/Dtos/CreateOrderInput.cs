using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class CreateOrderInput
	{
		public int PaymentMethod { get; set; } 
		public int Status { get; set; } 
		public long UserId { get; set; } // User who placed the order
		public List<OrderDetailDto> OrderDetails { get; set; }
	}
}
