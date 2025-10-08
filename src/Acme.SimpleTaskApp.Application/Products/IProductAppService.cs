using Abp.Application.Services;
using Acme.SimpleTaskApp.Products.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	public interface IProductAppService : IApplicationService
	{
		Product CreateProducts(CreateProductDto input);
		//void CreateProductVariants(int productId, List<ProductVariant> variants, string productName);
		void CreateGeneralProductImages(int productId, List<ProductImage> imageInputs, string productName);
		Task<Product> GetProductById(int id);
		Task<Product> EditProduct(Product input);
		Task<Product> DeleteProduct(int id);
	}
}
