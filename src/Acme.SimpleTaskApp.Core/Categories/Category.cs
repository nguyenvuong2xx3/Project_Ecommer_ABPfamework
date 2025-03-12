using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace Acme.SimpleTaskApp.Categories
{
    [Table("AppCategory")]
    public class Category : Entity, IHasCreationTime
    {
        public const int MaxNameLength = 256;
        public const int MaxDescriptionLength = 64 * 1024; // 64KB

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public Category()
        {
        }

        public Category(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
