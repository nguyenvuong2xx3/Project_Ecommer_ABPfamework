using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.HomeCustomers.Dtos
{
	public class SearchHomeCustomerDto : PagedAndSortedResultRequestDto
	{
		public string Filter { get; set; }
		public int? CategoryId { get; set; }
		public int MaxPrice { get; set; } = int.MaxValue;
		public int MinPrice { get; set; } = 0;
		//public string Sorting { get; set; } = "Name";
		public int SkipCount { get; set; } = 0;
		public int MaxResultCount { get; set; } = 10;
	}
}
