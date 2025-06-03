using Acme.SimpleTaskApp.OrderItems;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Orders
{
	public class OrdersViewModel
	{
		public int PaymentMethod { get; set; } // Payment method used for the order
		public int Status { get; set; } // Current status of the order
		public long UserId { get; set; } // ID of the user who placed the order
		public List<OrderDetails> OrderDetails { get; set; } // List of order details
		
	}
}
