//using Acme.SimpleTaskApp.Controllers;
//using Acme.SimpleTaskApp.Products;
//using Acme.SimpleTaskApp.Products.Dtos;
//using Acme.SimpleTaskApp.Web.Models.Products;
//using Microsoft.AspNetCore.Mvc;
//using Acme.SimpleTaskApp.Categories;
//using Acme.SimpleTaskApp.Categories.Dtos;
//using Microsoft.AspNetCore.Mvc.Rendering;


//namespace Acme.SimpleTaskApp.Web.Fontend.Controllers
//{
//	public class IndexController : SimpleTaskAppControllerBase
//	{
//		private readonly IProductAppService _productAppService;
//		private readonly IWebHostEnvironment webHostEnvironment;
//		private readonly ICategoryAppService _categoryAppService;
//		public IndexController(IProductAppService productAppService,
//															ICategoryAppService categoryAppService,
//															IWebHostEnvironment webHostEnvironment)
//		{
//			_productAppService = productAppService;
//			_categoryAppService = categoryAppService;
//			this.webHostEnvironment = webHostEnvironment;
//		}
//		public async Task<ActionResult> Index(GetAllProductsInput input, GetAllCategoryDto input1)
//		{
//			var output = await _productAppService.GetAllProducts(input);
//			var categories = await _categoryAppService.GetAllCategories(input1);

//			// Chuyển đổi CategoryListDto sang SelectListItem
//			var categoriesSelectList = categories.Items
//					.Select(c => new SelectListItem
//					{
//						Value = c.Id.ToString(),
//						Text = c.Name
//					})
//					.ToList();

//			var model = new ProductViewModel(output.Items)
//			{
//				Categories = categoriesSelectList // Gán danh sách đã chuyển đổi
//			};

//			return View(model);
//		}
//	}
//}