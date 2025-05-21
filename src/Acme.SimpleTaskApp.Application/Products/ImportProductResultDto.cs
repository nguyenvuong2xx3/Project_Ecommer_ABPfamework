using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	public class ImportProductResultDto
	{
		public int RowNumber { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
	}
}
