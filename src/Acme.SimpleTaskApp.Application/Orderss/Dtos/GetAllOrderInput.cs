using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{

		public class GetAllOrderInput : PagedAndSortedResultRequestDto, IShouldNormalize
		{
			public string Search { get; set; }

			public void Normalize()
			{
				if (Sorting.IsNullOrWhiteSpace())
				{
					Sorting = "CreationTime DESC";
				}
			}
	}
}
