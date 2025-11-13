using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Sales
{
	public class Sale : Entity<int>
	{
		public string? Description { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal DiscountPercentage { get; set; }
		public List<int>? CategoryId { get; set; }
		public List<int>? ProductId { get; set; }
		public List<int>? ProductVariantId { get; set; }
	}
}
