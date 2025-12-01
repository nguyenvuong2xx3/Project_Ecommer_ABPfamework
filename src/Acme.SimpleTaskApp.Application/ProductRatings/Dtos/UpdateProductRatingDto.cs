using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
    public class UpdateProductRatingDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [MaxLength(200)]
        public string Title { get; set; }
        
        [MaxLength(2000)]
        public string ReviewText { get; set; }
        
        [MaxLength(1000)]
        public string ImageUrls { get; set; }
    }
}
