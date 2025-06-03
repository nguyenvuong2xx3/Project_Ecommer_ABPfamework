using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	public class Order : FullAuditedEntity<int>
	{
		//public int CartId { get; set; }

		//[ForeignKey("CartId")]
		//public Cart Cart { get; set; }
		public int PaymentMethod { get; set; }
		public long UserId { get; set; }
		//public decimal TotalAmount { get; set; }
		public int Status { get; set; } // 0: Pending, 1: Completed, 2: Cancelled
	}
}
