using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Banners.Dtos
{
	public class CreateBannerDto
	{
		[Required(ErrorMessage = "Tiêu đề không được để trống")]
		[StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề phải có độ dài từ 3 đến 200 ký tự")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Thứ tự hiển thị không được để trống")]
		[Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải >= 0")]
		public int SortOrder { get; set; }

		public bool IsActive { get; set; } = true;

		public DateTime CreationTime { get; set; } = DateTime.Now;

		[Required(ErrorMessage = "Vui lòng chọn ảnh banner")]
		public IFormFile BannerImage { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn vị trí hiển thị")]
		public BannerPosition Position { get; set; }
	}
}
