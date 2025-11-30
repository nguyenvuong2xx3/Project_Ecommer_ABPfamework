using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Acme.SimpleTaskApp.Settings.Dtos
{
	public class StoreSettingDto
	{
		[Required(ErrorMessage = "Tên c?a hàng không ???c ?? tr?ng")]
		[StringLength(200, MinimumLength = 2, ErrorMessage = "Tên c?a hàng ph?i có ?? dài t? 2 ??n 200 ký t?")]
		public string NameStore { get; set; }

		[StringLength(500, ErrorMessage = "URL logo không ???c v??t quá 500 ký t?")]
		public string UrlLogo { get; set; }

		// File upload cho logo
		public IFormFile LogoFile { get; set; }
	}
}
