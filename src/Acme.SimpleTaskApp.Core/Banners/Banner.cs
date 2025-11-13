using Abp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Banners
{
	public class Banner : Entity<int>
	{
		public string? Title { get; set; }
		public string? ImageUrl { get; set; }
		public int? SortOrder { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreationTime { get; set; } = DateTime.Now;
		[NotMapped] public IFormFile? BannerImage { get; set; }
		public BannerPosition? Position { get; set; } = BannerPosition.HomeTop;
	}
	public enum BannerPosition
	{
		HomeTop,       // Trên cùng trang chủ
		HomeMiddle,    // Giữa trang chủ
		HomeBottom,    // Cuối trang chủ
		Sidebar,       // Thanh bên
		Header,        // Phần đầu trang
		Footer,        // Phần chân trang
		ProductPage,   // Trang chi tiết sản phẩm
		CategoryPage   // Trang danh mục
	}
}
