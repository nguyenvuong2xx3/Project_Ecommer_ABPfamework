using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Abp.AspNetCore.Mvc.Authorization;
using System.Collections.Generic;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Acme.SimpleTaskApp.Authorization;
using Abp.Authorization;


namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize]
	public class ProductsController : SimpleTaskAppControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Product> _productRepository;
		public ProductsController(IProductAppService productAppService,
							ICategoryAppService categoryAppService,
							IWebHostEnvironment webHostEnvironment,
							IRepository<Category> categoryRepository)
		{
			_categoryRepository = categoryRepository;
			_productAppService = productAppService;
			_categoryAppService = categoryAppService;
			this.webHostEnvironment = webHostEnvironment;
		}
		[AbpAuthorize(PermissionNames.Pages_products_view)]
		public async Task<ActionResult> Index()
		{
			var query = _categoryRepository.GetAll();
			var model = new ProductViewModel()
			{
				Categories = query.ToList()
			};
			return View(model);
		}

		[AbpAuthorize(PermissionNames.Pages_products_create)]
		public async Task<PartialViewResult> CreateModal()
		{
			var categories = await _categoryRepository.GetAllListAsync();

			var model = new ProductViewModel()
			{
				Categories = categories
			};

			return PartialView("_CreateProductModal", model);
		}

		[AbpAuthorize(PermissionNames.Pages_products_create)]
		public IActionResult CreateProduct(CreateProductDto model)
		{
			if (ModelState.IsValid)
			{
				var product = _productAppService.CreateProducts(model);
				//_productAppService.CreateProductVariants(product.Id, model.ProductVariants, model.Name);
				_productAppService.CreateGeneralProductImages(product.Id, model.ProductImages, model.Name);
				if (product != null)
				{
					return RedirectToAction("Index");
				}
			}
			return View("Create");
		}

		[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task<PartialViewResult> EditModal(int productId)
		{
			var categories = await _categoryRepository.GetAllAsync();
			var product = await _productAppService.GetProductById(productId);

			var model = new ProductViewModel()
			{
				Categories = categories.ToList(),
				Product = product
			};
			return PartialView("_EditProductModal", model);
		}
		[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task EditProduct(Product model)
		{
			var product = await _productAppService.EditProduct(model);
		}
		[AbpAuthorize(PermissionNames.Pages_products_delete)]
		public async Task Delete(int id)
		{
			await _productAppService.DeleteProduct(id);
		}

		public async Task<PartialViewResult> ImportModal()
		{
			return PartialView("_ImportProductModal");
		}
	}
}
