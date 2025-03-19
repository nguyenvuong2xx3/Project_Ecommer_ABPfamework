using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Carts
{
	[Table("AppCart")]
	public class Cart : Entity, IHasCreationTime
	{
		public int UserId { get; set; }
		public int Count { get; set; }

		public DateTime CreationTime { get; set; }

		[ForeignKey(nameof(IdCartItem))]
		public int IdCartItem { get; set; }
	}

}
