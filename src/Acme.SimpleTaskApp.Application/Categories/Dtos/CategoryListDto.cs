using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;

namespace Acme.SimpleTaskApp.Categories.Dto
{
	public class CategoryListDto : EntityDto, IHasCreationTime
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public DateTime CreationTime { get; set; }
	}
}
