using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
    public class CreateProductRatingDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? OrderId { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Rating ph?i t? 1 ??n 5 sao")]
        public int Rating { get; set; }
        
        [MaxLength(200)]
        public string Title { get; set; }
        
        [MaxLength(2000)]
        public string ReviewText { get; set; }
        
        [MaxLength(1000)]
        public string ImageUrls { get; set; } // Comma-separated URLs
    }
}
