namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
    /// <summary>
    /// Product rating statistics and distribution
    /// </summary>
    public class ProductRatingStatisticsDto
    {
        public int ProductVariantId { get; set; }
        
        /// <summary>
        /// Total number of ratings
        /// </summary>
        public int TotalRatings { get; set; }
        
        /// <summary>
        /// Average rating (1.0 - 5.0)
        /// </summary>
        public double AverageRating { get; set; }
        
        /// <summary>
        /// Rating distribution by stars
        /// </summary>
        public RatingDistribution Distribution { get; set; }
        
        /// <summary>
        /// Number of verified purchase ratings
        /// </summary>
        public int VerifiedPurchaseCount { get; set; }
    }
    
    public class RatingDistribution
    {
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
        
        // Percentages
        public double FiveStarsPercent { get; set; }
        public double FourStarsPercent { get; set; }
        public double ThreeStarsPercent { get; set; }
        public double TwoStarsPercent { get; set; }
        public double OneStarPercent { get; set; }
    }
}
