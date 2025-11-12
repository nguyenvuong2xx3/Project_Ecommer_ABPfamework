using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport.Dtos
{
	public class GetProductInput : PagedAndSortedResultRequestDto
	{
		public string SearchTerm { get; set; }
		public string Name { get; set; }
		public int? CategoryId { get; set; }
		public string Screen { get; set; }
		public string Processor { get; set; }
		public string CameraSystem { get; set; }
		public string Battery { get; set; }

		// Phải khớp với thuộc tính gán trong index.js
		public DateTime? CreationTimeStart { get; set; }
		public DateTime? CreationTimeEnd { get; set; }

		// Bạn có thể thêm các bộ lọc khác nếu cần
	}
}
