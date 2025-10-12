using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Products.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	public interface IProductAppService : IApplicationService
	{
		Product CreateProducts(CreateProductDto input);
		//void CreateProductVariants(int productId, List<ProductVariant> variants, string productName);
		Task<PagedResultDto<Product>> GetAllProduct(SearchProductDto input);
		void CreateGeneralProductImages(int productId, List<ProductImage> imageInputs, string productName);
		Task<Product> GetProductById(int id);
		Task<Product> EditProduct(Product input);
		Task<Product> DeleteProduct(int id);
	}
}
