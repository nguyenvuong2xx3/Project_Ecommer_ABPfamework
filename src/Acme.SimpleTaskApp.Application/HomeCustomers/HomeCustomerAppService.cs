using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Carts.Dtos;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.ProductRatings;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Sales;
using Microsoft.AspNetCore.Authorization;
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
	private readonly IRepository<ProductRating> _productRatingRepository;
	private readonly ISaleAppService _saleAppService;
	private readonly IRepository<Order> _orderRepository;

	public HomeCustomerAppService(
		IRepository<Order> orderRepository,
		IRepository<Product> productRepository,
		IRepository<ProductVariant> productVariantRepository,
		IRepository<Category> categoryRepository,
		IRepository<ProductImage> productImageRepository,
		IRepository<ProductRating> productRatingRepository,
		ISaleAppService saleAppService)
	{
		_orderRepository = orderRepository;
		_productRepository = productRepository;
		_productVariantRepository = productVariantRepository;
		_categoryRepository = categoryRepository;
		_productImageRepository = productImageRepository;
		_productRatingRepository = productRatingRepository;
		_saleAppService = saleAppService;
	}
	
	[UnitOfWork]
	[AbpAllowAnonymous]
	public async Task<PagedResultDto<Product>> GetAllProductHomeCustomers(SearchHomeCustomerDto input)
	{
		input.MaxResultCount = 12;
		if (input == null) input = new SearchHomeCustomerDto();

		// lấy sản phẩm ra
		var prodQuery = _productRepository.GetAll();

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
		if (input.CategoryId != null)
		{
			prodQuery = prodQuery.Where(p => input.CategoryId == p.CategoryId.Value);
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
			//mặc định xếp theo thời gian mới nhất
			prodQuery = prodQuery.OrderByDescending(p => p.CreationTime);
		}

		var productIds = prodQuery.Select(p => p.Id).ToList();

		// lấy biến thể
		var variants = await _productVariantRepository.GetAll()
			.Where(v => productIds.Contains(v.ProductId)).ToListAsync();

		var variantIds = variants.Select(pv => pv.Id).ToList();

		// ảnh biến thể
		var images = await _productImageRepository.GetAll()
			.Where(i => variantIds.Contains(i.ProductVariantId.Value))
			.ToListAsync();
		// số lượng sản phẩm đã bán
		var countSold = await _orderRepository.GetAllAsync();
		countSold = countSold.Where(x => x.Status == 3);

		// lấy đánh giá
		var ratings = await _productRatingRepository.GetAll()
			.Where(r => productIds.Contains(r.ProductVariantId) && r.IsApproved)
			.GroupBy(r => r.ProductVariantId)
			.Select(g => new
			{
				ProductId = g.Key,
				AverageRating = g.Average(r => r.Rating),
				TotalRatings = g.Count()
			})
			.ToListAsync();
		var ratingDict = ratings.ToDictionary(r => r.ProductId);

		//lấy ảnh
		foreach (var p in prodQuery)
		{
			// Attach variants
			var pVariants = variants.Where(v => v.ProductId == p.Id).ToList();
			
			// Get sales for each variant
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


				// lấy số lượng đã bán
				foreach (var o in countSold)
				{
					o.Deserialize();
					foreach (var item in o.OrderDetails)
					{
						if (item.ProductVariantId == v.Id)
						{
							v.SoldQuantity += item.Quantity ?? 0;
						}
					}
				};
				// đánh giá %
				if (ratingDict.TryGetValue(p.Id, out var ratingInfo))
				{
					v.AverageRating = System.Math.Round(ratingInfo.AverageRating, 1);
					v.TotalRatings = ratingInfo.TotalRatings;
				}

				// lấy sale
				var bestSale = await _saleAppService.GetBestSaleForProductVariant(v.Id, p.Id, p.CategoryId);
				if (bestSale != null)
				{
					v.DiscountPercentage = bestSale.DiscountPercentage;
					v.DiscountedPrice = v.GetDiscountedPrice(bestSale.DiscountPercentage);
					v.HasActiveDiscount = true;
				}
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
				
			// NEW: Get best sale for variant
			var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == variant.ProductId);
			var bestSale = await _saleAppService.GetBestSaleForProductVariant(variant.Id, variant.ProductId, product?.CategoryId);
			if (bestSale != null)
			{
				variant.DiscountPercentage = bestSale.DiscountPercentage;
				variant.DiscountedPrice = variant.GetDiscountedPrice(bestSale.DiscountPercentage);
				variant.HasActiveDiscount = true;
				variant.SaleName = bestSale.Name;
			}
		}

		// lấy sản phẩm tổng quát
		var productResult = await _productRepository.FirstOrDefaultAsync(x => x.Id == item.ProductId);

		// add vào product
		productResult.ProductVariants.AddRange(allVariants);
		productResult.ProductVariant = allVariants.FirstOrDefault(x => x.Id == id);
		
		// lấy ảnh product
		productResult.ImageUrls = await _productImageRepository.GetAll()
			.Where(x => x.ProductId == productResult.Id && x.ProductVariantId == null)
			.Select(ig => ig.ImageUrl)
			.ToListAsync();
			
		return productResult;
	}
}
