using Acme.SimpleTaskApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using MyProject.Orders;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class OrdersController : SimpleTaskAppControllerBase
	{
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IOrderAppService _orderAppService;
		private readonly IProductAppService _productAppService;


		public OrdersController(IOrderDetailAppService orderDetailAppService, IProductAppService productAppService, IOrderAppService orderAppService)
		{
			_orderDetailAppService = orderDetailAppService;
			_productAppService = productAppService;
			_orderAppService = orderAppService;
		}
		public ActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> DetailOrder(int orderId)
		{
			var orderDetail = await _orderDetailAppService.GetAllOrder(orderId);

			var productIds = orderDetail.Select(x => x.ProductId).Distinct().ToList();
			var products = await _productAppService.GetProductByIds(productIds);

			var model = new DetailOrderViewModel
			{
				OrderList = orderDetail,
				ProductList = products
			};
			return View("DetailOrder", model);
		}

		public async Task<IActionResult> EditOrderModal(int orderId)
		{
			var order = await _orderAppService.GetOrderById(orderId);

			var model = new OrderViewModel
			{
				Order = order,
			};

			return PartialView("EditOrderModal", model);
		}
	}
}
