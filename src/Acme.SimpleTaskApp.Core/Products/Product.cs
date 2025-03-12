using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Timing;
using Acme.SimpleTaskApp.Categories;
namespace Acme.SimpleTaskApp.Products
{
    [Table("AppProducts")]
    public class Product : Entity, IHasCreationTime
    {
        public const int MaxNameLength = 256;
        public const int MaxDescriptionLength = 64 * 1024; // 64KB

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public ProductState State { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Categories{ get; set; }
        public int? CategoryId { get; set; }

        public Product()
        {
            CreationTime = Clock.Now;
            State = ProductState.Available;
        }

        public Product(string name, string description = null, decimal price = 0)
            : this()
        {
            Name = name;
            Description = description;
            Price = price;
        }

        public enum ProductState : byte
        {
            Available = 0,
            OutOfStock = 1,
        }
    }
}
