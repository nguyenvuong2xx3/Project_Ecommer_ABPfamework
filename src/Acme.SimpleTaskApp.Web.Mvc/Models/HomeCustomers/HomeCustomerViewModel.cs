using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Products;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.HomeCustomers
{
	public class HomeCustomerViewModel
	{
		public List<Product> ProductsInfo { get; set; }

		public Product ProductInfo { get; set; }

		public List<Category> Categories { get; set; }
		/// <summary>
		///  cho kết quả tìm kiếm
		/// </summary>
		public string Filter { get; set; }

		public string CategoryName { get; set; }

	}
}
