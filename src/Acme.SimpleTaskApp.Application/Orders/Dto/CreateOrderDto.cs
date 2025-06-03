using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Entities.Auditing;
using MyProject.OrderDetails.Dto;

namespace Acme.SimpleTaskApp.Orders.Dto
{
	public class CreateOrderDto : FullAuditedEntity<int>
	{
		public long UserId { get; set; }
		public string NameUser { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public int	PaymentMethod { get; set; }
		public int OrderStatus { get; set; }
	}
}
