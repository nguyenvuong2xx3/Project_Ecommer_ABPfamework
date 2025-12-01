using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.ProductRatings.Dtos
{
    /// <summary>
    /// ProductRatingDto - Manual mapping, no AutoMapper
    /// </summary>
    public class ProductRatingDto : EntityDto<int>
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        
        public int? OrderId { get; set; }
        
        public int Rating { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        
        public List<string> ImageUrls { get; set; }
        
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        
        public string AdminResponse { get; set; }
        public DateTime? AdminResponseTime { get; set; }
        
        public bool IsEdited { get; set; }
        public DateTime? EditedTime { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        // Additional computed properties
        public int TotalVotes => HelpfulCount + NotHelpfulCount;
        public double HelpfulPercentage => TotalVotes > 0 ? (double)HelpfulCount / TotalVotes * 100 : 0;
        
        // Current user's vote on this rating
        public bool? CurrentUserVote { get; set; } // true=helpful, false=not helpful, null=not voted
    }
}
