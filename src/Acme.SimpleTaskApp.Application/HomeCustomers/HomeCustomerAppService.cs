using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
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

		// lấy sản phẩm ra
		var prodQuery = _productRepository.GetAll();

		// Thay thế đoạn lọc tên sản phẩm bằng bộ lọc không phân biệt khoảng trắng và chữ hoa/thường
		// lọc theo tên tìm kiếm
		if (!string.IsNullOrWhiteSpace(input.Filter))
		{
			var f = input.Filter.Trim().ToLower().Replace(" ", "");
			prodQuery = prodQuery.Where(x => x.Name != null && x.Name.ToLower().Replace(" ", "").Contains(f));
		}

		// Lọc theo danh mục
		if (input.CategoryIds != null && input.CategoryIds.Count() > 0)
		{
			prodQuery = prodQuery.Where(p => input.CategoryIds.Contains(p.CategoryId.Value));
		}

		// lọc theo giá
		var productVariant = _productVariantRepository.GetAll();
		var priceFilteringNeeded = false;
		if (input.MinPrice > 0)
		{
			productVariant = productVariant.Where(v => v.Price >= input.MinPrice);
			priceFilteringNeeded = true;
		}
		if (input.MaxPrice > 0)
		{
			productVariant = productVariant.Where(v => v.Price <= input.MaxPrice);
			priceFilteringNeeded = true;
		}
		if (priceFilteringNeeded)
		{
			// trả ra Id sản phẩm thỏa mãn
			var productIdsWithVariantsInPriceRange = await productVariant
				.Select(v => v.ProductId)
				.Distinct() // chú ý 
				.ToListAsync();

			prodQuery = prodQuery.Where(p => productIdsWithVariantsInPriceRange.Contains(p.Id));
		}
		var totalCount = await prodQuery.CountAsync();
			prodQuery = prodQuery.PageBy(input);
		
		if (input.SortingByPrice)
		{
			prodQuery = input.SortDirection == "DESC"
					? prodQuery.OrderByDescending(p => p.ProductVariants.Min(v => v.Price))
					: prodQuery.OrderBy(p => p.ProductVariants.Min(v => v.Price));
		}
		else if (input.SortingByName)
		{
			prodQuery = input.SortDirection == "DESC"
					? prodQuery.OrderByDescending(p => p.Name)
					: prodQuery.OrderBy(p => p.Name);
		}
		else if (input.SortingCreation)
		{
			prodQuery = input.SortDirection == "ASC"
					? prodQuery.OrderBy(p => p.CreationTime)
					: prodQuery.OrderByDescending(p => p.CreationTime);
		}
		else
		{
			// Default: sort by creation date descending
			prodQuery = prodQuery.OrderByDescending(p => p.CreationTime);
		}

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
	public async Task<PagedResultDto<Product>> GetAllProductHomeCustomers1(SearchHomeCustomerDto input)
	{
		var getallVariant = _productVariantRepository.GetAll().ToList();
		var getImgage = _productImageRepository.GetAll().ToList();
		// lấy ảnh biến thể 
		foreach (var variant in getallVariant)
		{
			variant.ImageUrls = getImgage
				.Where(x => x.ProductVariantId == variant.Id)
				.Select(ig => ig.ImageUrl)
				.ToList();
		}
		var result = new PagedResultDto<Product>(totalCount, prodQuery.ToList());
		return result;
	}
	public async Task<Product> GetProductById(int id)
	{
		if (id <= 0)
			throw new UserFriendlyException("Dữ liệu không được để trống");
		// lấy biến thể
		var item = await _productVariantRepository.GetAsync(id);

		//lấy hết biến thể liên quan đến sản phẩm
		var allVariants = await _productVariantRepository.GetAll()
			.Where(x => x.ProductId == item.ProductId)
			.ToListAsync();
		// lấy ảnh biến thể cho tất cả biến thể
		foreach (var variant in allVariants)
		{
			variant.ImageUrls = await _productImageRepository.GetAll()
				.Where(x => x.ProductVariantId == variant.Id)
				.Select(ig => ig.ImageUrl)
				.ToListAsync();
		}

		// lấy sản phẩm tổng quát
		var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == item.ProductId);

		// lấy ảnh biến thể
		item.ImageUrls = await _productImageRepository.GetAll()
			.Where(x => x.ProductVariantId == item.Id)
			.Select(ig => ig.ImageUrl)
			.ToListAsync();

		// add vào product
		product.ProductVariants.AddRange(allVariants);
		product.ProductVariant = item;
		// lấy ảnh product
		product.ImageUrls = await _productImageRepository.GetAll()
			.Where(x => x.ProductId == product.Id)
			.Select(ig => ig.ImageUrl)
			.ToListAsync();
		return product;
	}
}
