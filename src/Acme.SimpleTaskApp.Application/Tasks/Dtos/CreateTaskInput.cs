using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Tasks.Dtos
{
    [AutoMapTo(typeof(Task))]
    public class CreateTaskInput
    {
        [Required]
        [StringLength(Task.MaxTitleLength)]
        public string Title { get; set; }

        [StringLength(Task.MaxDescriptionLength)]
        public string Description { get; set; }

        public long AssignedPersonId { get; set; }
    }
}
