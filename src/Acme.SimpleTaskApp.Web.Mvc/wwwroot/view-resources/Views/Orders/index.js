(function ($) {
	var _orderService = abp.services.app.order,

		l = abp.localization.getSource('MyProject'),
		_$modal = $('#OrderCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#OrdersTable');

	var _$orderTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _orderService.getAllOrder,
			inputFilter: function () {
				return $('#OrderSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$orderTable.draw(false)
			}
		],
		responsive: {
			details: {
				type: 'column'
			}
		},
		columnDefs: [
			{
				targets: 0,
				data: 'nameUser',
				sortable: false
			},
			{
				targets: 1,
				data: 'totalAmount',
				sortable: false,
				render: data => Number(data).toLocaleString('vi-VN') + ' ₫'
			},
			{
				targets: 2,
				data: 'discountAmount',
				sortable: false,
				render: data => Number(data).toLocaleString('vi-VN') + ' ₫'
			},
			{
				targets: 3,
				data: 'paymentMethod',
				sortable: false,
				render: function (data, type, row) {
					const paymentMethods = {
						0: '<i class="fas fa-university text-primary"></i> Bank Transfer',
						1: '<i class="fas fa-credit-card text-success"></i> Credit Card',
						2: '<i class="fab fa-paypal text-info"></i> PayPal',
						3: '<i class="fas fa-money-bill-wave text-warning"></i> Cash on Delivery'
					};
					return paymentMethods[data] || '<span class="text-danger">Unknown</span>';
				}
			},
			{
				targets: 4,
				data: 'creationTime',
				sortable: false,
				render: data => new Date(data).toLocaleDateString('vi-VN')
			},
			{
				targets: 5,
				data: 'orderStatus',
				sortable: false,
				render: function (data, type, row) {
					switch (data) {
						case 0: return '<span class="badge bg-warning">Pending</span>';
						case 1: return '<span class="badge bg-primary">Confirmed</span>';
						case 2: return '<span class="badge bg-info">Shipping</span>';
						case 3: return '<span class="badge bg-danger">Canceled</span>';
						case 4: return '<span class="badge bg-success">Success</span>';
						default: return '<span class="badge bg-dark">Unknown</span>';
					}
				}
			},
			{
				targets: 6,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					return [
						`   <button type="button" class="btn btn-sm bg-secondary edit-order" data-order-id="${row.id}" data-toggle="modal" data-target="#OrderEditModal">`,
						`       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-info detail-order" data-order-id="${row.id}" data-toggle="modal" >`,
						`       <i class="fas fa-eye"></i> ${l('Details')}`,
						'   </button>'
					].join('');
				}
			}
		]
	});



	//_$form.find('.save-button').on('click', (e) => {
	//	e.preventDefault();

	//	if (!_$form.valid()) {
	//		return;
	//	}

	//	var product = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
	//	var formData = new FormData(_$form[0]);
	//	abp.ui.setBusy(_$modal);
	//	$.ajax({

	//		url: abp.appPath + 'Products/Create', // Đường dẫn đến phương thức trong controller
	//		type: 'POST',
	//		processData: false, // Important! Không xử lý dữ liệu
	//		contentType: false, // Important!  Không đặt kiểu dữ liệu
	//		data: formData,
	//		error: function (xhr, textStatus, errorThrown) {
	//			var errorMessage;
	//			if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
	//				errorMessage = xhr.responseJSON.errors.join("<br/>");
	//			}
	//			else {
	//				errorMessage = "Có lỗi xảy ra khi tạo mới khách hàng (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
	//			}
	//			$("#error-message").html(errorMessage).show();
	//		}
	//	}).done(function () {
	//		/*resetDefaultImage();*/
	//		_$modal.modal('hide');
	//		_$form[0].reset();
	//		abp.notify.info(l('Lưu thành công'));
	//		_$productTable.ajax.reload();

	//	}).always(function () {

	//		abp.ui.clearBusy(_$modal);

	//	});
	//});


	$(document).on('click', '.edit-order', function (e) {
		var orderId = $(this).attr("data-order-id");

		e.preventDefault();
		abp.ajax({
			url: abp.appPath + 'Orders/EditOrderModal?orderId=' + orderId,
			type: 'POST',
			dataType: 'html',
			success: function (content) {
				$('#OrderEditModal div.modal-content').html(content);
			},
			error: function (e) {
			}
		});
	});


	abp.event.on('order.edited', (data) => {
		_$orderTable.ajax.reload();
	});

	_$modal.on('shown.bs.modal', () => {
		_$modal.find('input:not([type=hidden]):first').focus();
	}).on('hidden.bs.modal', () => {
		_$form.clearForm(); // Xóa toàn bộ dữ liệu trong form
		$('#previewImage').attr('src', '/img/products/default_product.png'); // Đặt lại ảnh về mặc định
	});


	$('.btn-search').on('click', (e) => {
		_$orderTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$orderTable.ajax.reload();
			return false;
		}
	});


	$(document).on('click', '.detail-order', function (e) {
		var orderId = $(this).attr("data-order-id");

		window.location.href = "/Orders/DetailOrder?orderId=" + orderId;
	});


})(jQuery);


