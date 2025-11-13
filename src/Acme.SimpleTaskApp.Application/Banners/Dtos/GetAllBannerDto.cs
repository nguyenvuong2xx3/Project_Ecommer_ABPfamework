using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Banners.Dtos
{
	public class GetAllBannerDto : PagedResultRequestDto
	{
		public string? Filter { get; set; }
		public bool? IsActive { get; set; } // Lọc theo trạng thái hoạt động
		public BannerPosition? Position { get; set; } // Lọc theo vị trí banner
	}
}
