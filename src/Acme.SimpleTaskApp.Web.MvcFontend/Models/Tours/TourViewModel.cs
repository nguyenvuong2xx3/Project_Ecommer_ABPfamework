using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Tours.Dtos;
using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Tours
{
	public class TourViewModel
	{
		public IReadOnlyList<TourListDto> Tours { get; set; }
		public TourViewModel(IReadOnlyList<TourListDto> tours)
		{
			Tours = tours;
		}
	}
}
