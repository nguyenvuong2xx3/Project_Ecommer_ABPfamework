using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport.Dtos
{
	public class ImportResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public int TotalProducts { get; set; }
		public int TotalVariants { get; set; }
		public List<string> ErrorList { get; set; } = new List<string>();
	}
	public class ImportProductsInput
	{
		public IFormFile File { get; set; }
	}
	public class ImportProductRowResult
	{
		public int? ProductId { get; set; }
		public int? ProductVariantId { get; set; }
		public List<string> Errors { get; set; } = new List<string>();
		public bool IsSuccess { get; set; }
	}
}
