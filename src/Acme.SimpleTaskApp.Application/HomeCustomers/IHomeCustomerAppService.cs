using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.HomeCustomers
{
	public interface IHomeCustomerAppService : IApplicationService
	{
		Task<PagedResultDto<Product>> GetAllProductHomeCustomers(SearchHomeCustomerDto input);
		Task<PagedResultDto<Product>> GetAllProductHomeCustomersV2(SearchHomeCustomerDto input);
		Task<Product> GetProductById(int id);
	}
}
