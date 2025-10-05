using System.Collections.Generic;
using System;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreationTime { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new List<CategoryTreeDto>();
    }
}