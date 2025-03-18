using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.AppTours;
using Acme.SimpleTaskApp.Tours.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Tours
{
	public class TourAppService : ApplicationService, ITourAppService
	{
		private readonly IRepository<Tour, long> _tourRepository;

		public TourAppService(IRepository<Tour, long> tourRepository)
		{
			_tourRepository = tourRepository;
		}

		public async Task<PagedResultDto<TourListDto>> GetAllTour(GetAllTourInput input)
		{
			var tours = _tourRepository.GetAll();

			var count = await tours.CountAsync();

			var tourDtos = await tours.OrderByDescending(x => x.CreationTime)
																.PageBy(input)
																.Select(t => new TourListDto
																{
																	TourName = t.TourName,
																	MinGroupSize = t.MinGroupSize,
																	MaxGroupSize = t.MaxGroupSize,
																	TourTypeCid = t.TourTypeCid,
																	StartDate = t.StartDate,
																	EndDate = t.EndDate,
																	Transportation = t.Transportation,
																	TourPrice = t.TourPrice,
																	PhoneNumber = t.PhoneNumber,
																	Description = t.Description,
																	Attachment = t.Attachment
																}).ToListAsync();

			return new PagedResultDto<TourListDto>(count, tourDtos);
		}

		public async Task<TourListDto> CreateTour(CreateTourInput input)
		{
			var tour = new Tour
			{
				TourName = input.TourName,
				MinGroupSize = input.MinGroupSize,
				MaxGroupSize = input.MaxGroupSize,
				TourTypeCid = input.TourTypeCid,
				StartDate = input.StartDate,
				EndDate = input.EndDate,
				Transportation = input.Transportation,
				TourPrice = input.TourPrice,
				PhoneNumber = input.PhoneNumber,
				Description = input.Description,
				Attachment = input.Attachment
			};

			await _tourRepository.InsertAsync(tour);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new TourListDto
			{
				Id = tour.Id,
				TourName = tour.TourName,
				MinGroupSize = tour.MinGroupSize,
				MaxGroupSize = tour.MaxGroupSize,
				TourTypeCid = tour.TourTypeCid,
				StartDate = tour.StartDate,
				EndDate = tour.EndDate,
				Transportation = tour.Transportation,
				TourPrice = tour.TourPrice,
				PhoneNumber = tour.PhoneNumber,
				Description = tour.Description,
				Attachment = tour.Attachment
			};
		}


		public async Task DeleteTour(EntityDto<long> input)
		{
			// Xóa sản phẩm
			await _tourRepository.DeleteAsync(input.Id);
		}

		public async Task<TourListDto> GetByIdTour(EntityDto<long> input)
		{
			var tour = await _tourRepository.GetAsync(input.Id);
			if (tour == null)
			{
				throw new UserFriendlyException("Product not found!");
			}
			else
			{
				// Ánh xạ sang DTO
				return new TourListDto
				{
					Id = tour.Id,
					TourName = tour.TourName,
					MinGroupSize = tour.MinGroupSize,
					MaxGroupSize = tour.MaxGroupSize,
					TourTypeCid = tour.TourTypeCid,
					StartDate = tour.StartDate,
					EndDate = tour.EndDate,
					Transportation = tour.Transportation,
					TourPrice = tour.TourPrice,
					PhoneNumber = tour.PhoneNumber,
					Description = tour.Description,
					Attachment = tour.Attachment
				};
			}
		}

		public async Task<PagedResultDto<TourListDto>> SearchTour(GetAllTourInput input)
		{
			var tourQuery = _tourRepository.GetAll();

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				tourQuery = tourQuery.Where(p => 
				p.TourName.ToLower().Contains(keywordLower) ||
				p.Description.ToLower().Contains(keywordLower) ||
				p.PhoneNumber.ToLower().Contains(keywordLower)
				);
			}
			if (!string.IsNullOrWhiteSpace(input.Transportation))
			{
				tourQuery = tourQuery.Where(p => p.Transportation == input.Transportation);
			}

			if (input.TourTypeCid.HasValue && input.TourTypeCid != -1)
			{
				tourQuery = tourQuery.Where(p => p.TourTypeCid == input.TourTypeCid);
			}


			if (input.StartDate.HasValue || input.EndDate.HasValue)
			{
				if (input.EndDate < input.StartDate)
				{
					throw new UserFriendlyException("Ngày kết thúc không được nhỏ hơn ngày bắt đầu");
				}
				tourQuery = tourQuery
				.WhereIf(input.StartDate.HasValue, t => t.StartDate >= input.StartDate.Value)
				.WhereIf(input.EndDate.HasValue, t => t.EndDate <= input.EndDate.Value);

			}

				// Thực hiện các điều kiện tìm kiếm khác


			var count = await tourQuery.CountAsync();

			var tourDtos = await tourQuery.OrderByDescending(p => p.CreationTime)
																			.PageBy(input)
																			.Select(p => new TourListDto
																			{
																				Id = p.Id,
																				TourName = p.TourName,
																				MinGroupSize = p.MinGroupSize,
																				MaxGroupSize = p.MaxGroupSize,
																				TourTypeCid = p.TourTypeCid,
																				StartDate = p.StartDate,
																				EndDate = p.EndDate,
																				Transportation = p.Transportation,
																				TourPrice = p.TourPrice,
																				PhoneNumber = p.PhoneNumber,
																				Description = p.Description,
																				Attachment = p.Attachment
																			}).ToListAsync();

			return new PagedResultDto<TourListDto>(count, tourDtos);
		}

		public async Task<TourListDto> UpdateTour(UpdateTourInput input)
		{
			// Lấy sản phẩm hiện có
			var tour = await _tourRepository.GetAsync(input.Id);
			if (tour == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại!");
			}

			// Cập nhật thông tin (trừ Image)
			tour.Id = input.Id;
			tour.TourName = input.TourName;
			tour.MinGroupSize = input.MinGroupSize;
			tour.MaxGroupSize = input.MaxGroupSize;
			tour.TourTypeCid = input.TourTypeCid;
			tour.StartDate = input.StartDate;
			tour.EndDate = input.EndDate;
			tour.Transportation = input.Transportation;
			tour.TourPrice = input.TourPrice;
			tour.PhoneNumber = input.PhoneNumber;
			tour.Description = input.Description;

			// Chỉ cập nhật Image nếu có ảnh mới
			if (!string.IsNullOrEmpty(input.Attachment))
			{
				tour.Attachment = input.Attachment;
			}

			// Lưu thay đổi vào database
			await _tourRepository.UpdateAsync(tour);
			await CurrentUnitOfWork.SaveChangesAsync();

			//Ánh xạ sang DTO để trả về
			return new TourListDto
			{
				Id = tour.Id,
				TourName = tour.TourName,
				MinGroupSize = tour.MinGroupSize,
				MaxGroupSize = tour.MaxGroupSize,
				TourTypeCid = tour.TourTypeCid,
				StartDate = tour.StartDate,
				EndDate = tour.EndDate,
				Transportation = tour.Transportation,
				TourPrice = tour.TourPrice,
				PhoneNumber = tour.PhoneNumber,
				Description = tour.Description,
				Attachment = tour.Attachment
			};
		}


	}
}
