using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HomeCustomerAppService : IHomeCustomerAppService
{
	private readonly IRepository<Product> _productRepository;
	private readonly IRepository<ProductVariant> _productVariantRepository;
	private readonly IRepository<Category> _categoryRepository;
	private readonly IRepository<ProductImage> _productImageRepository;

	public HomeCustomerAppService(
		IRepository<Product> productRepository,
		IRepository<ProductVariant> productVariantRepository,
		IRepository<Category> categoryRepository,
		IRepository<ProductImage> productImageRepository)
	{
		_productRepository = productRepository;
		_productVariantRepository = productVariantRepository;
		_categoryRepository = categoryRepository;
		_productImageRepository = productImageRepository;
	}

	public async Task<PagedResultDto<Product>> GetAllProductHomeCustomers(SearchHomeCustomerDto input)
	{
		if (input == null) input = new SearchHomeCustomerDto();

		// Base product query
		var prodQuery = _productRepository.GetAll();

		// Text filter on Name
		// Replace exact match filter with approximate (contains) match
		// Thay thế đoạn lọc tên sản phẩm bằng bộ lọc không phân biệt khoảng trắng và chữ hoa/thường
		if (!string.IsNullOrWhiteSpace(input.Filter))
		{
			var f = input.Filter.Trim().ToLower().Replace(" ", "");
			prodQuery = prodQuery.Where(x => x.Name != null && x.Name.ToLower().Replace(" ", "").Contains(f));
		}

		// Category filter
		if (input.CategoryId.HasValue)
		{
			prodQuery = prodQuery.Where(p => p.CategoryId == input.CategoryId.Value);
		}

		// Price filter based on product variants
		var variantQueryForPriceFiltering = _productVariantRepository.GetAll();
		var priceFilteringNeeded = false;
		if (input.MinPrice > 0)
		{
			variantQueryForPriceFiltering = variantQueryForPriceFiltering.Where(v => v.Price >= input.MinPrice);
			priceFilteringNeeded = true;
		}
		if (input.MaxPrice > 0)
		{
			variantQueryForPriceFiltering = variantQueryForPriceFiltering.Where(v => v.Price <= input.MaxPrice);
			priceFilteringNeeded = true;
		}
		if (priceFilteringNeeded)
		{
			var productIdsWithVariantsInPriceRange = await variantQueryForPriceFiltering
				.Select(v => v.ProductId)
				.Distinct() // chú ý 
				.ToListAsync();

			prodQuery = prodQuery.Where(p => productIdsWithVariantsInPriceRange.Contains(p.Id));
		}

		// Total count before paging
		var totalCount = await prodQuery.CountAsync();
			prodQuery = prodQuery.OrderBy(p => p.CreationTime).PageBy(input);


		// Apply sorting: if no sorting provided, default to Name
		//if (string.IsNullOrWhiteSpace(input.Sorting))
		//{
		//	prodQuery = prodQuery.OrderBy(p => p.Name);
		//}


		// Ensure sensible paging values
		//var skip = Math.Max(0, input.SkipCount);
		//var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;

		// Fetch products page
		//var products = await prodQuery.Skip(skip).Take(take).ToListAsync();

		//// If no products, return empty DTO
		//if (products.Count == 0)
		//{
		//	var emptyDto = new GetAllProductCustomerDto { ProductsInfo = new List<Product>() };
		//	var emptyResult = new PagedResultDto<GetAllProductCustomerDto>(totalCount, new List<GetAllProductCustomerDto> { emptyDto });
		//	return emptyResult;
		//}

		var productIds = prodQuery.Select(p => p.Id).ToList();

		// Fetch variants and images for those products in bulk
		var variants = await _productVariantRepository.GetAll()
			.Where(v => productIds.Contains(v.ProductId))
			.ToListAsync();

		var images = await _productImageRepository.GetAll()
			.Where(i => productIds.Contains(i.ProductId))
			.ToListAsync();

		// Attach variants and images to products
		foreach (var p in prodQuery)
		{
			// Attach variants
			var pVariants = variants.Where(v => v.ProductId == p.Id).ToList();
			foreach (var v in pVariants)
			{
				v.ProductName = p.Name;
				// Images for this variant
				v.ImageUrls = images
					.Where(img => img.ProductVariantId.HasValue && img.ProductVariantId.Value == v.Id)
					.OrderBy(img => img.SortOrder)
					.Select(img => img.ImageUrl)
					.ToList();
				v.ImageUrl = v.ImageUrls.FirstOrDefault();
			}
			p.ProductVariants = pVariants;

			// Attach images that are product-level (ProductVariantId == null)
			p.ProductImages = images
				.Where(img => img.ProductId == p.Id && img.ProductVariantId == null)
				.OrderBy(img => img.SortOrder)
				.ToList();

			p.ImageUrls = p.ProductImages.Select(img => img.ImageUrl).ToList();
			p.ImageUrl = p.ImageUrls.FirstOrDefault();
		}
		var dto = new GetAllProductCustomerDto
		{
			ProductsInfo = prodQuery.ToList()
		};

		var result = new PagedResultDto<Product>(totalCount, prodQuery.ToList());
		return result;
	}
}
