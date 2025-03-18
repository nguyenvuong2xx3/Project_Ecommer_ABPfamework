using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.AppTours
{
		[Table("AppTours")]
		public class Tour : FullAuditedEntity<long>
		{

		
			[Required]
			public string TourName { get; set; } // Tên tour

			public int MinGroupSize { get; set; } // Số khách tối thiểu

			public int MaxGroupSize { get; set; } // Số khách tối đa

			public long TourTypeCid { get; set; } // Kiểu tour: Tour du lịch nội tỉnh, Tour du lịch liên tỉnh, Tour du lịch quốc tế // select 

			public DateTime? StartDate { get; set; } // thời gian bắt đầu

			public DateTime? EndDate { get; set; } // thời gian kết thúc

			[Required]
			public string Transportation { get; set; } // Phương tiện

			public decimal? TourPrice { get; set; } // Giá tour

			[Required]
			public string PhoneNumber { get; set; } // Điện thoại

			[Required]
			public string Description { get; set; } // Mô tả

			public string Attachment { get; set; } // Đính kèm
		}
	}
