
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;
//using Acme.SimpleTaskApp.Products;
//using Acme.SimpleTaskApp.Products.Dtos;
//using Acme.SimpleTaskApp.Categories;
//using Acme.SimpleTaskApp.Categories.Dtos;
//using System.Linq;
//using Abp.Application.Services.Dto;

//namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductListCategory
//{

//	public class ProductListCategoryViewComponent : ViewComponent
//	{
//		private readonly IProductFEAppService _productFEAppService;
//		private readonly ICategoryAppService _categoryAppService;

//		public ProductListCategoryViewComponent(IProductFEAppService productFEAppService)
//		{
//			_productFEAppService = productFEAppService;
//		}

//		public ProductListCategoryViewComponent(IProductFEAppService productFEAppService, ICategoryAppService categoryAppService)
//		{
//			_productFEAppService = productFEAppService;
//			_categoryAppService = categoryAppService;
//		}

//		public async Task<IViewComponentResult> InvokeAsync()
//		{
//			var products = await _productFEAppService.GetAllProducts(new GetAllProductsInput { });
//			var categories = await _categoryAppService.GetAllCategories(new GetAllCategoryDto { });

//			var model = new ProductListCategoryViewModel
//			{
//				Products = new ListResultDto<ProductListDto>
//				{
//					Items = products.Items.Select(product => new ProductListDto
//					{
//						Name = product.Name,
//						Description = product.Description,
//						Price = product.Price,
//						CreationTime = product.CreationTime,
//						State = product.State,
//						Image = product.Image,
//						CategoryName = categories.Items.FirstOrDefault(c => c.Id == product.CategoryId)?.Name,
//						CategoryId = product.CategoryId
//					}).ToList()
//				}
//			};

//			return View(model);
//		}
//	}
//}
