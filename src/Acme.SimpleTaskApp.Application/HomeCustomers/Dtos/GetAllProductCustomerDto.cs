using Acme.SimpleTaskApp.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.HomeCustomers.Dtos
{
	public class GetAllProductCustomerDto
	{
		public List<Product> ProductsInfo { get; set; }
	}
}
