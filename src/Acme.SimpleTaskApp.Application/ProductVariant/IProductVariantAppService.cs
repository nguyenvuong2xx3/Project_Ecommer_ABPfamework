using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductVariants.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductVariants
{
	public interface IProductVariantAppService : IApplicationService
	{
		Task CreateProductVariant(ProductVariant input);
		Task EditProductVariant(ProductVariant input);
		Task DeleteProductVariant(int id);
		Task<PagedResultDto<ProductVariant>> GetAllProductVariant(GetProductVariantsInput input);
		Task<ProductVariant> GetById(int id);
	}
}
