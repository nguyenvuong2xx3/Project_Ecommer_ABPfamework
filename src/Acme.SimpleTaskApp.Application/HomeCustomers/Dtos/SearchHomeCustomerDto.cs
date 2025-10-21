using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Products;
using System.Collections.Generic;
using System.Linq;

public class SearchHomeCustomerDto : PagedResultRequestDto
{
	public string Filter { get; set; }
	public int? CategoryId { get; set; }
	public List<int>? CategoryIds { get; set; }
	public List<string> Brands { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public bool? HasDiscount { get; set; }
	public bool? InStock { get; set; } = true;
	public bool SortingByPrice { get; set; }
	public bool SortingByName { get; set; }
	public bool SortingCreation { get; set; }
	public string SortDirection { get; set; } // "ASC" , "DESC"
}