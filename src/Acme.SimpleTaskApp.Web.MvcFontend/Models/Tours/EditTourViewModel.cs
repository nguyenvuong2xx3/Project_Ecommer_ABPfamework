using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Tours.Dtos;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Tours
{
	public class EditTourViewModel
	{
		public UpdateTourInput Tours { get; set; }

	}
}
