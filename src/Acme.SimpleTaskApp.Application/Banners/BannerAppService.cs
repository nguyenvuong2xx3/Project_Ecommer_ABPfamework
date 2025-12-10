using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Banners.Dtos;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Banners
{
	[AbpAuthorize(PermissionNames.Pages_Banners)]
	public class BannerAppService : ApplicationService, IBannerAppService
	{
		private readonly IRepository<Banner, int> _bannerRepository;
		private readonly IUploadFileAppService _uploadFileAppService;

		public BannerAppService(IRepository<Banner, int> bannerRepository, IUploadFileAppService uploadFileAppService)
		{
			_bannerRepository = bannerRepository;
			_uploadFileAppService = uploadFileAppService;
		}

		[AbpAuthorize(PermissionNames.Pages_Banners_Create)]
		public async Task<Banner> CreateBanner(CreateBannerDto input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không hợp lệ");
			}

			// Tạo đối tượng Banner
			Banner banner = new Banner
			{
				Title = input.Title,
				SortOrder = input.SortOrder,
				IsActive = input.IsActive,
				Position = input.Position,
				CreationTime = DateTime.Now
			};

			// Upload image nếu có
			if (input.BannerImage != null && input.BannerImage.Length > 0)
			{
				banner.ImageUrl = await _uploadFileAppService.UploadImageAsync(input.BannerImage, "banners");
			}

			// Insert vào database
			var result = await _bannerRepository.InsertAsync(banner);
			await CurrentUnitOfWork.SaveChangesAsync();

			return result;
		}

		[AbpAuthorize(PermissionNames.Pages_Banners_Edit)]
		public async Task<Banner> UpdateBanner(Banner input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không hợp lệ");
			}

			var banner = await _bannerRepository.FirstOrDefaultAsync(input.Id);
			if (banner == null)
				throw new UserFriendlyException($"Không tìm thấy Banner có Id = {input.Id}");

			if (input.BannerImage != null && input.BannerImage.Length > 0)
			{
				// Delete old image
				if (!string.IsNullOrEmpty(banner.ImageUrl))
				{
					await _uploadFileAppService.RemoveImage(banner.ImageUrl);
				}

				// Upload new image
				input.ImageUrl = await _uploadFileAppService.UploadImageAsync(input.BannerImage, "banners");
			}

			// Cập nhật các thuộc tính
			banner.Title = input.Title;
			banner.ImageUrl = input.ImageUrl;
			banner.SortOrder = input.SortOrder;
			banner.IsActive = input.IsActive;
			banner.Position = input.Position;

			await _bannerRepository.UpdateAsync(banner);
			return banner;
		}

		[AbpAuthorize(PermissionNames.Pages_Banners_Delete)]
		public async Task DeleteBanner(int id)
		{
			var banner = await _bannerRepository.FirstOrDefaultAsync(id);
			if (banner == null)
				throw new UserFriendlyException($"Không tìm thấy Banner có Id = {id}");

			await _bannerRepository.DeleteAsync(banner);
		}

		[AbpAuthorize(PermissionNames.Pages_Banners_View)]
		public async Task<PagedResultDto<Banner>> GetAllBanners(GetAllBannerDto input)
		{
			var query = _bannerRepository.GetAll();

			// Lọc theo từ khóa (Title)
			if (!string.IsNullOrWhiteSpace(input.Filter))
			{
				var f = input.Filter.Trim().ToLower();
				query = query.Where(x => x.Title.ToLower().Contains(f));
			}

			// Lọc theo trạng thái
			if (input.IsActive.HasValue)
				query = query.Where(x => x.IsActive == input.IsActive.Value);

			// Lọc theo vị trí
			if (input.Position.HasValue)
				query = query.Where(x => x.Position == input.Position.Value);

			// Tổng số banner
			var totalCount = await query.CountAsync();
			var result = await query
				.OrderBy(c => c.SortOrder)
				.PageBy(input)
				.ToListAsync();

			return new PagedResultDto<Banner>(totalCount, result);
		}

		[UnitOfWork]
		[AbpAllowAnonymous]
		public async Task<List<Banner>> GetListBanners(GetAllBannerDto input)
		{
			var query = _bannerRepository.GetAll();

			// Lọc theo từ khóa (Title)
			if (!string.IsNullOrWhiteSpace(input.Filter))
			{
				var f = input.Filter.Trim().ToLower();
				query = query.Where(x => x.Title.ToLower().Contains(f));
			}

			// Lọc theo trạng thái
			if (input.IsActive.HasValue)
				query = query.Where(x => x.IsActive == input.IsActive.Value);

			// Lọc theo vị trí
			if (input.Position.HasValue)
				query = query.Where(x => x.Position == input.Position.Value);

			// Tổng số banner
			var totalCount = await query.CountAsync();
			var result = await query
				.OrderBy(c => c.SortOrder)
				.ToListAsync();

			return new List<Banner>(result);
		}

		[AbpAuthorize(PermissionNames.Pages_Banners_View)]
		public Task<Banner> GetBannerById(int id)
		{
			if (id <= 0)
			{
				throw new UserFriendlyException("Id không hợp lệ");
			}
			var result = _bannerRepository.FirstOrDefaultAsync(id);
			if (result == null)
			{
				throw new UserFriendlyException($"Không tìm thấy Banner có Id = {id}");
			}
			return result;
		}
	}
}
