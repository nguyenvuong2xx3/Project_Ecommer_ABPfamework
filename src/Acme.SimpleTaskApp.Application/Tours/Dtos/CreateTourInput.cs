using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Tours.Dtos
{
	public class CreateTourInput
	{
		public string TourName { get; set; }

		public int MinGroupSize { get; set; }

		public int MaxGroupSize { get; set; }

		public long TourTypeCid { get; set; }

		public DateTime? StartDate { get; set; } // thời gian bắt đầu

		public DateTime? EndDate { get; set; } // thời gian kết thúc

		public string Transportation { get; set; } // Phương tiện

		public decimal? TourPrice { get; set; } // Giá tour

		public string PhoneNumber { get; set; } // Điện thoại

		public string Description { get; set; } // Mô tả

		public string Attachment { get; set; } // Đính kèm

		public IFormFile ImageFile { get; set; }

	}
}
