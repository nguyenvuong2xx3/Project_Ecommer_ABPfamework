using System;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
	public class SearchCategoryDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public string Keyword { get; set; }

        public string Image { get; set; }  // Lưu đường dẫn ảnh
    }
}
