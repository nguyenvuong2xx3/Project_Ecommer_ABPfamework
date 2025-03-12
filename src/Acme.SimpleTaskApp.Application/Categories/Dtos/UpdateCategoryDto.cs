using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Categories.Category;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
    public class UpdateCategoryDto : EntityDto, IHasCreationTime
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
