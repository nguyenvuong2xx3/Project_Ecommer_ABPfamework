(function ($) {
	var _orderService = abp.services.app.orders,
		l = abp.localization.getSource('SimpleTaskApp'),
		_$table = $('#OrdersTable');

	var _permissions = {
		view: abp.auth.hasPermission('Pages.Orders.View'),
		create: abp.auth.hasPermission('Pages.Orders.Create'),
		edit: abp.auth.hasPermission('Pages.Orders.Edit'),
		delete: abp.auth.hasPermission('Pages.Orders.Delete'),
		confirm: abp.auth.hasPermission('Pages.Orders.Confirm'),
		cancel: abp.auth.hasPermission('Pages.Orders.Cancel'),
		ship: abp.auth.hasPermission('Pages.Orders.Ship'),
		complete: abp.auth.hasPermission('Pages.Orders.Complete'),
	};

	var _detailModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Orders/DetailModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Orders/_DetailModal.js',
		modalClass: 'DetailOrderModal',
		modalSize: 'modal-lg'
	});

	$(document).on('click', '.detail-order', function () {
		var orderId = $(this).data('order-id');
		_detailModal.open({ orderId: orderId });
	});

	var getFilter = function () {
		let dataFilter = {};

		// Lấy giá trị từ ô tìm kiếm NameUser
		dataFilter.userName = $('#NameUser').val();

		// Lấy giá trị từ dropdown PaymentMethod
		let paymentMethod = $('#PaymentMethod').val();
		dataFilter.paymentMethod = paymentMethod ? parseInt(paymentMethod) : null;

		// Lấy giá trị từ dropdown OrderStatus
		let status = $('#OrderStatus').val();
		dataFilter.status = status ? parseInt(status) : null;

		console.log(dataFilter);
		return dataFilter;
	};


	var _$ordersTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		processing: true,
		listAction: {
			ajaxFunction: _orderService.getAllOrder,
			inputFilter: getFilter
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$ordersTable.draw(false)
			}
		],
		columnDefs: [
			{
				targets: 0,
				data: 'fullName'
			},
			{
				targets: 1,
				data: 'totalPrice',
				render: function (data) {
					return Number(data).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
				}
			},
			{
				targets: 2,
				data: 'paymentMethod',
				render: function (data) {
					if (data == 0) {
						return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#4CAF50;color:white;">Thanh toán tiền mặt</span>';
					} else if (data == 2) {
						return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#2196F3;color:white;">Thanh toán VNPay</span>';
					} else {
						return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#9E9E9E;color:white;">Không xác định</span>';
					}
				}
			},
			{
				targets: 3,
				data: 'creationTime',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<span class="badge bg-info">${moment(data).format('DD-MM-YYYY HH:mm')}</span>`;
					}
					return '<span class="text-muted">N/A</span>';
				}
			},
			{
				targets: 4,
				data: 'status',
				render: function (data) {

					const style = "display:inline-block;padding:2px 8px;border-radius:5px;color:white;";

					switch (data) {
						case 0:
							return `<span style="${style}background-color:#FFC107;">Chờ xác nhận</span>`;
						case 1:
							return `<span style="${style}background-color:#2196F3;">Đang xử lý</span>`;
						case 2:
							return `<span style="${style}background-color:#4CAF50;">Đang giao hàng</span>`;
						case 3:
							return `<span style="${style}background-color:#9C27B0;">Thành công</span>`;
						case 4:
							return `<span style="${style}background-color:#F44336;">Đã hủy bởi admin</span>`;
						case 5:
							return `<span style="${style}background-color:#FF9800;">Đã hủy bởi người dùng</span>`;
						default:
							return `<span style="${style}background-color:#9E9E9E;">Không xác định</span>`;
					}
				}
			},
			{
				targets: 5,
				data: null,
				orderable: false,
				render: function (data, type, row) {
					let buttons = [];

					if (_permissions.confirm && row.status === 0) { // Chờ xác nhận
						buttons.push(
							`<button type="button" class="btn btn-sm btn-primary xuly-order mr-2" data-order-id="${row.id}">
                      <i class="fas fa-check"></i> ${l('Xác nhận')}
                    </button>`,

							_permissions.cancel ? `<button type="button" class="btn btn-sm btn-danger huydon-order mr-2" data-order-id="${row.id}">
                      <i class="fas fa-times"></i> ${l('Hủy đơn')}
                    </button>` : ''
						);
					}

					if (_permissions.ship && row.status === 1) { // Đang xử lý
						buttons.push(
							`<button type="button" class="btn btn-sm btn-success danggiao-order mr-2" data-order-id="${row.id}">
                      <i class="fas fa-truck"></i> ${l('Đang giao')}
                    </button>`
						);
					}

					if (_permissions.complete && row.status === 2) { // Đang giao
						buttons.push(
							`<button type="button" class="btn btn-sm btn-success thanhcong-order mr-2" data-order-id="${row.id}">
                      <i class="fas fa-check-circle"></i> ${l('Thành công')}
                    </button>`
						);
					}

					// Nút chi tiết luôn hiển thị
					if (_permissions.view) {
						buttons.push(
							`<button type="button" class="btn btn-sm btn-info detail-order mt-2" data-order-id="${row.id}">
                    <i class="fas fa-eye"></i> ${l('Chi tiết')}
                  </button>`
						);
					}

					return buttons.join('');
				}
			}
		]
	});

	// Xem chi tiết đơn hàng
	//$(document).on('click', '.detail-order', function () {
	//  var orderId = $(this).data('order-id');
	//  _orderService.getOrder(orderId)
	//    .done(function (result) { 
	//      var detailHtml = buildOrderDetailHtml(result);
	//      $('#OrderDetailModal .modal-body').html(detailHtml);
	//      $('#OrderDetailModal').modal('show');
	//    })
	//    .fail(function () {
	//      abp.notify.error('Could not load order detail');
	//    });
	//});

	//function buildOrderDetailHtml(orderDetail) {
	//  var html = '<div class="order-detail">';
	//  html += '<h4>Order #' + orderDetail.id + '</h4>';
	//  html += '<p><strong>Customer:</strong> ' + orderDetail.fullName + '</p>';
	//  html += '<p><strong>Total Price:</strong> ' + Number(orderDetail.totalPrice).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' }) + '</p>';
	//  // Add more fields as needed
	//  html += '</div>';
	//  return html;
	//}

	$(document).on('click', '.huydon-order', function () {
		var orderId = $(this).data('order-id');
		_orderService.huyAdminOrder(orderId)
			.done(function () {
				abp.notify.success('Đã hủy đơn thành công!');
				_$ordersTable.ajax.reload();

			})
			.fail(function () {
				abp.notify.error('Hủy đơn thất bại!');
			});
	});

	$(document).on('click', '.xuly-order', function () {
		var orderId = $(this).data('order-id');
		_orderService.xuLyOrder(orderId)
			.done(function () {
				abp.notify.success('Đã duyệt đơn thành công!');
				_$ordersTable.ajax.reload();
			})
			.fail(function () {
				abp.notify.error('Duyệt đơn thất bại!');
			});
	});

	$(document).on('click', '.danggiao-order', function () {
		var orderId = $(this).data('order-id');
		_orderService.dangGiaoOrder(orderId)
			.done(function () {
				abp.notify.success('Đã duyệt đơn thành công!');
				_$ordersTable.ajax.reload();
			})
			.fail(function () {
				abp.notify.error('Duyệt đơn thất bại!');
			});
	});

	$(document).on('click', '.thanhcong-order', function () {
		var orderId = $(this).data('order-id');
		_orderService.thanhCongOrder(orderId)
			.done(function () {
				abp.notify.success('Đã duyệt đơn thành công!');
				_$ordersTable.ajax.reload();
			})
			.fail(function () {
				abp.notify.error('Duyệt đơn thất bại!');
			});
	});

	// Chỉnh sửa đơn hàng
	$(document).on('click', '.edit-order', function () {
		var orderId = $(this).data('order-id');
		abp.ajax({
			url: abp.appPath + 'Orders/EditModal?orderId=' + orderId,
			type: 'GET',
			dataType: 'html',
			success: function (content) {
				$('#OrderEditModal .modal-content').html(content);
				$('#OrderEditModal').modal('show');
			},
			error: function (e) {
				abp.notify.error('Could not load edit form');
			}
		});
	});

	// Tìm kiếm
	$('.btn-search').on('click', function () {
		_$ordersTable.ajax.reload();
	});

	$('#OrdersTableFilter').on('keypress', function (e) {
		if (e.which === 13) {
			_$ordersTable.ajax.reload();
		}
	});

})(jQuery);