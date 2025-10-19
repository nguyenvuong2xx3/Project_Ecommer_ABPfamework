using Abp.Domain.Repositories;
using Abp.UI;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductVariants;
using Acme.SimpleTaskApp.Web.Models.ProductVariants;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class ProductVariantsController : SimpleTaskAppControllerBase
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IProductVariantAppService _productVariantAppService;
		public ProductVariantsController(IRepository<Product> productRepository, IProductVariantAppService productVariantAppService)
		{
			_productVariantAppService = productVariantAppService;
			_productRepository = productRepository;
		}
		public IActionResult Index()
		{
			var getallProduct = _productRepository.GetAll().ToList();
			var model = new ProductVariantViewModel()
			{
				Products = getallProduct
			};
			return View(model);
		}
		public ActionResult CreateModal()
		{
			var getProduct = _productRepository.GetAll().ToList();
			var model = new ProductVariantViewModel()
			{
				Products = getProduct
			};
			return PartialView("_CreateProductVariantModal", model);
		}
		public async Task Create(ProductVariant input)
		{

			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}
			await _productVariantAppService.CreateProductVariant(input);
		}
		public ActionResult EditModal(int id)
		{
			var getProduct = _productRepository.GetAll().ToList();
			var getProductVariant = _productVariantAppService.GetById(id);
			var model = new ProductVariantViewModel()
			{
				Products = getProduct,
				ProductVariant = getProductVariant.Result
			};
			return PartialView("_EditProductVariantModal", model);
		}
		public async Task Edit(ProductVariant input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}
			await _productVariantAppService.EditProductVariant(input);
		}
		public ActionResult DetailModal(int id)
		{
			var getProductVariant = _productVariantAppService.GetById(id);
			var model = new ProductVariantViewModel()
			{
				ProductVariant = getProductVariant.Result
			};
			return PartialView("_DetailProductVariantModal", model);
		}
		public async Task Delete(int id)
		{
			if (id <= 0)
			{
				throw new UserFriendlyException("Dữ liệu không hợp lệ");
			}
			await _productVariantAppService.DeleteProductVariant(id);
		}
	}
}
