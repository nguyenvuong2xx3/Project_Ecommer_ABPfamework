using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class OrderListDto : FullAuditedEntity<int>
	{
		public string UserName { get; set; }
		public string Name { get; set; }

		public int Status { get; set; }
		public int PaymentMethod { get; set; }
		public string CreationTime { get; set; }
		public decimal TotalCount { get; set; }
	}
}
