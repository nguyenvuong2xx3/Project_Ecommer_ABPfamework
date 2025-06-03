using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject.OrderDetails.Dto;

namespace Acme.SimpleTaskApp.Orders.Dto
{
	public class CreateOrderRequestDto
	{
		public long UserId { get; set; }
		public string NameUser { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public int PaymentMethod { get; set; }
		public List<OrderDetailDto> Items { get; set; }
	}
}
