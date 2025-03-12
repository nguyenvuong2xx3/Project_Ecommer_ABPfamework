using Microsoft.AspNetCore.Http;
using System;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
