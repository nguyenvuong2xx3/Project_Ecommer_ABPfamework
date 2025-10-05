using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	//[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;

		public ProductAppService(IRepository<Product> productRepository, IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IWebHostEnvironment env)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}

		public Task<ProductListDto> CreateProducts(CreateProductDto input)
		{
			throw new NotImplementedException();
		}

		public Task DeleteProducts(EntityDto<int> input)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> ExportProductsToExcel(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<ProductListDto> GetByIdProducts(EntityDto<int> input)
		{
			throw new NotImplementedException();
		}

		public Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file)
		{
			throw new NotImplementedException();
		}

		public Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<ProductListDto> UpdateProducts(UpdateProductDto input)
		{
			throw new NotImplementedException();
		}

		//[AbpAuthorize(PermissionNames.Pages_products)]
		//public async Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		//{
		//	var query = _productRepository.GetAll();

		//	var totalCount = await query.CountAsync();

		//	var products = await query.OrderByDescending(x => x.CreationTime)
		//			.PageBy(input)
		//			.ToListAsync();

		//	var productIds = products.Select(p => p.Id).ToList();
		//	var images = await _productImageRepository.GetAll().Where(pi => productIds.Contains(pi.ProductId)).ToListAsync();

		//	var productDtos = products.Select(p => new ProductListDto
		//	{
		//		Id = p.Id,
		//		Name = p.Name,
		//		Description = p.Description,
		//		Price = p.Price,
		//		CreationTime = p.CreationTime,
		//		Image = images.FirstOrDefault(i => i.ProductId == p.Id)?.ImageUrl,
		//		CategoryId = p.CategoryId
		//	}).ToList();

		//	return new PagedResultDto<ProductListDto>(totalCount, productDtos);
		//}

		//[AbpAuthorize(PermissionNames.Pages_products_create)]
		//public async Task<ProductListDto> CreateProducts(CreateProductDto input)
		//{
		//	// Create product entity
		//	var product = new Product
		//	{
		//		Name = input.Name,
		//		Description = input.Description,
		//		Price = input.Price,
		//		CategoryId = input.CategoryId.GetValueOrDefault()
		//	};

		//	await _productRepository.InsertAsync(product);
		//	await CurrentUnitOfWork.SaveChangesAsync();

		//	// Save multiple images if provided
		//	if (input.ImageFiles != null && input.ImageFiles.Any())
		//	{
		//		var uploadsFolder = Path.Combine(_env.WebRootPath, "img", "products");
		//		Directory.CreateDirectory(uploadsFolder);

		//		int sort = 0;
		//		foreach (var file in input.ImageFiles)
		//		{
		//			if (file == null || file.Length == 0) continue;

		//			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
		//			var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
		//			if (!allowed.Contains(ext)) continue;

		//			var uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + ext;
		//			var filePath = Path.Combine(uploadsFolder, uniqueFileName);
		//			using (var stream = new FileStream(filePath, FileMode.Create))
		//			{
		//				await file.CopyToAsync(stream);
		//			}

		//			var imagePath = "/img/products/" + uniqueFileName;
		//			var productImage = new ProductImage
		//			{
		//				ProductId = product.Id,
		//				ImageUrl = imagePath,
		//				SortOrder = sort++
		//			};
		//			await _productImageRepository.InsertAsync(productImage);
		//		}

		//		await CurrentUnitOfWork.SaveChangesAsync();
		//	}

		//	var firstImage = await _productImageRepository.GetAll().Where(pi => pi.ProductId == product.Id).OrderBy(pi => pi.SortOrder).FirstOrDefaultAsync();

		//	return new ProductListDto
		//	{
		//		Id = product.Id,
		//		Name = product.Name,
		//		Description = product.Description,
		//		Price = product.Price,
		//		CreationTime = product.CreationTime,
		//		Image = firstImage?.ImageUrl,
		//		CategoryId = product.CategoryId
		//	};
		//}

		//[AbpAuthorize(PermissionNames.Pages_products_delete)]
		//public async Task DeleteProducts(EntityDto<int> input)
		//{
		//	await _productRepository.DeleteAsync(input.Id);
		//}

		//public async Task<ProductListDto> GetByIdProducts(EntityDto<int> input)
		//{
		//	var product = await _productRepository.GetAsync(input.Id);
		//	if (product == null)
		//	{
		//		throw new UserFriendlyException("Product not found!");
		//	}
		//	else
		//	{
		//		var firstImage = await _productImageRepository.GetAll().Where(pi => pi.ProductId == product.Id).OrderBy(pi => pi.SortOrder).FirstOrDefaultAsync();
		//		return new ProductListDto
		//		{
		//			Id = product.Id,
		//			Name = product.Name,
		//			Description = product.Description,
		//			Price = product.Price,
		//			CreationTime = product.CreationTime,
		//			Image = firstImage?.ImageUrl,
		//			CategoryId = product.CategoryId
		//		};
		//	}
		//}

		//[AbpAuthorize(PermissionNames.Pages_products_update)]
		//public async Task<ProductListDto> UpdateProducts(UpdateProductDto input)
		//{
		//	var product = await _productRepository.GetAsync(input.Id);
		//	if (product == null)
		//	{
		//		throw new Exception("Sản phẩm không tồn tại!");
		//	}

		//	product.Name = input.Name;
		//	product.Description = input.Description;
		//	product.Price = input.Price;
		//	product.CategoryId = input.CategoryId.GetValueOrDefault();

		//	await _productRepository.UpdateAsync(product);
		//	await CurrentUnitOfWork.SaveChangesAsync();

		//	var firstImage = await _productImageRepository.GetAll().Where(pi => pi.ProductId == product.Id).OrderBy(pi => pi.SortOrder).FirstOrDefaultAsync();

		//	return new ProductListDto
		//	{
		//		Id = product.Id,
		//		Name = product.Name,
		//		Description = product.Description,
		//		Price = product.Price,
		//		CreationTime = product.CreationTime,
		//		Image = firstImage?.ImageUrl,
		//		CategoryId = product.CategoryId
		//	};
		//}

		//[AbpAuthorize(PermissionNames.Pages_products_search)]
		//public async Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input)
		//{
		//	var productQuery = _productRepository.GetAll();
		//	if (!string.IsNullOrWhiteSpace(input.Keyword))
		//	{
		//		string keywordLower = input.Keyword.ToLower();
		//		productQuery = productQuery.Where(p => p.Name.ToLower().Contains(keywordLower));
		//	}

		//	if (!string.IsNullOrWhiteSpace(input.Category))
		//	{
		//		int categoryId = Convert.ToInt32(input.Category);
		//		productQuery = productQuery.Where(x => x.CategoryId == categoryId);
		//	}

		//	var count = await productQuery.CountAsync();

		//	var products = await productQuery.OrderByDescending(p => p.CreationTime)
		//			.PageBy(input)
		//			.ToListAsync();

		//	var productIds = products.Select(p => p.Id).ToList();
		//	var images = await _productImageRepository.GetAll().Where(pi => productIds.Contains(pi.ProductId)).ToListAsync();

		//	var productDtos = products.Select(p => new ProductListDto
		//	{
		//		Id = p.Id,
		//		Name = p.Name,
		//		Description = p.Description,
		//		Price = p.Price,
		//		CreationTime = p.CreationTime,
		//		Image = images.FirstOrDefault(i => i.ProductId == p.Id)?.ImageUrl,
		//		CategoryId = p.CategoryId
		//	}).ToList();

		//	return new PagedResultDto<ProductListDto>(count, productDtos);
		//}

		//public async Task<byte[]> ExportProductsToExcel(GetAllProductsInput input)
		//{
		//	var query = _productRepository.GetAll();
		//	if (!string.IsNullOrWhiteSpace(input.Keyword))
		//	{
		//		query = query.Where(x => x.Name.ToLower().Contains(input.Keyword.ToLower()));
		//	}

		//	if (int.TryParse(input.Category, out var categoryId))
		//	{
		//		query = query.Where(x => x.CategoryId == categoryId);
		//	}

		//	var products = await query.OrderByDescending(x => x.CreationTime).ToListAsync();

		//	var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
		//	var categories = await _categoryRepository.GetAll().Where(c => categoryIds.Contains(c.Id)).ToListAsync();
		//	var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

		//	var images = await _productImageRepository.GetAll().Where(pi => products.Select(p => p.Id).Contains(pi.ProductId)).ToListAsync();

		//	ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

		//	using (var package = new ExcelPackage())
		//	{
		//		var worksheet = package.Workbook.Worksheets.Add("Products");

		//		// Tiêu đề cột
		//		worksheet.Cells[1, 1].Value = "Tên sản phẩm";
		//		worksheet.Cells[1, 2].Value = "Mô tả";
		//		worksheet.Cells[1, 3].Value = "Thời gian tạo";
		//		worksheet.Cells[1, 4].Value = "Giá";
		//		worksheet.Cells[1, 5].Value = "Danh mục";
		//		worksheet.Cells[1, 6].Value = "Ảnh sản phẩm";
		//		int row = 2; // bắt đầu từ dòng 2

		//		foreach (var item in products)
		//		{
		//			worksheet.Cells[row, 1].Value = item.Name;
		//			worksheet.Cells[row, 2].Value = item.Description;
		//			worksheet.Cells[row, 3].Value = item.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
		//			worksheet.Cells[row, 4].Value = item.Price;
		//			worksheet.Cells[row, 5].Value = categoryMap.ContainsKey(item.CategoryId) ? categoryMap[item.CategoryId] : string.Empty;
		//			worksheet.Cells[row, 6].Value = images.FirstOrDefault(i => i.ProductId == item.Id)?.ImageUrl;
		//			row++;
		//		}

		//		worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

		//		return package.GetAsByteArray();
		//	}
		//}

		//public async Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file)
		//{
		//	var getAllCategories = await _categoryRepository.GetAllListAsync();

		//	ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

		//	var results = new List<ImportProductResultDto>();
		//	var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "products");

		//	Directory.CreateDirectory(uploadsFolder);

		//	if (file == null || file.Length == 0)
		//		throw new Exception("File không tồn tại");

		//	if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
		//		throw new Exception("Chỉ hỗ trợ file Excel (.xlsx)");

		//	using (var stream = new MemoryStream())
		//	{
		//		await file.CopyToAsync(stream);

		//		using (var package = new ExcelPackage(stream))
		//		{
		//			if (package.Workbook.Worksheets.Count == 0)
		//				throw new Exception("File Excel không có sheet nào");

		//			var worksheet = package.Workbook.Worksheets[0];
		//			int rowCount = worksheet.Dimension?.Rows ?? 0;

		//			for (int row = 2; row <= rowCount; row++)
		//			{
		//				var result = new ImportProductResultDto
		//				{
		//					RowNumber = row,
		//					Id = int.Parse(worksheet.Cells[row, 1]?.Text?.Trim()),
		//					Name = worksheet.Cells[row, 2]?.Text?.Trim()
		//				};

		//				try
		//				{
		//					var existingProduct = await _productRepository.FirstOrDefaultAsync(p => p.Id == result.Id);
		//					if (existingProduct != null)
		//					{
		//						existingProduct.Name = worksheet.Cells[row, 2]?.Text?.Trim();
		//						existingProduct.Description = worksheet.Cells[row, 3]?.Text?.Trim();
		//						existingProduct.Price = decimal.TryParse(worksheet.Cells[row, 5]?.Text, out var parsedPrice) ? parsedPrice : 0;

		//						var category = getAllCategories.FirstOrDefault(c => c.Name.Equals(worksheet.Cells[row, 6]?.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
		//						if (category != null)
		//							existingProduct.CategoryId = category.Id;

		//						await _productRepository.UpdateAsync(existingProduct);
		//						result.IsSuccess = true;
		//						result.Message = "Cập nhật sản phẩm thành công";
		//						results.Add(result);
		//						continue;
		//					}

		//					string categoryName = worksheet.Cells[row, 6]?.Text?.Trim();

		//					var categoryObj = getAllCategories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

		//					if (categoryObj == null)
		//						throw new Exception($"Không tìm thấy danh mục: {categoryName}");

		//					// Create product from excel row (no images)
		//					await CreateProducts(new CreateProductDto { Name = result.Name, CategoryId = categoryObj.Id, Description = worksheet.Cells[row, 3]?.Text?.Trim(), Price = decimal.TryParse(worksheet.Cells[row, 5]?.Text, out var price) ? price : 0 });
		//					result.IsSuccess = true;
		//					result.Message = "Thành công";
		//				}
		//				catch (Exception ex)
		//				{
		//					result.IsSuccess = false;
		//					result.Message = $"Dòng {row}: {ex.Message}";
		//				}

		//				results.Add(result);
		//			}
		//		}
		//	}

		//	return results;
		//}

		//public async Task<ProductListDto> Create(CreateProductDto input)
		//{
		//	// compatibility wrapper to call CreateProducts
		//	return await CreateProducts(input);
		//}
	}
}
