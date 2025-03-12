using Abp.Application.Services.Dto;
using System;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
	public class GetAllCategoryDto : PagedAndSortedResultRequestDto
    {
        public string Name { get; set; } // Tìm kiếm theo tên sản phẩm

		    public string Description { get; set; } // Tìm kiếm theo mô tả sản phẩm

		    public DateTime CreateTime { get; set; }


    }
}
