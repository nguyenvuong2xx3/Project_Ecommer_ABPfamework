(function () {
	var _orderService = abp.services.app.orders;
	var _currentStatus = null;
	var _currentPaymentMethod = null;
	var _selectedDateRange = {
		StartTime: null,
		EndTime: null
	};

	$(document).ready(function () {
		initializeDateRangePicker();
		bindEvents();
		loadOrders();
	});

	function loadOrders() {
		var input = {
			StatusUser: _currentStatus,
			PaymentMethod: _currentPaymentMethod,
			StartTime: _selectedDateRange.StartTime,
			EndTime: _selectedDateRange.EndTime,
			Sorting: "CreationTime DESC"
		};

		abp.ui.setBusy($('#orders-tab-pane'));

		_orderService.getOrderByCurrentUser(input)
			.done(function (result) {
				renderOrders(result);
			})
			.fail(function (error) {
				console.error('Lỗi khi tải đơn hàng:', error);
				abp.notify.error('Không thể tải danh sách đơn hàng!');
			})
			.always(function () {
				abp.ui.clearBusy($('#orders-tab-pane'));
			});
	}

	function renderOrders(orders) {
		var $container = $('#orders-tab-pane');
		var $emptyContainer = $container.find('.empty-orders-container');
		var $ordersList = $container.find('.orders-list-container');

		if ($ordersList.length === 0) {
			$emptyContainer.after('<div class="orders-list-container"></div>');
			$ordersList = $container.find('.orders-list-container');
		}

		if (!orders || orders.length === 0) {
			$emptyContainer.show();
			$ordersList.hide().empty();
			return;
		}

		$emptyContainer.hide();

		var html = '';
		orders.forEach(function (order) {
			html += createOrderCard(order);
		});

		$ordersList.html(html).show();
	}

	function createOrderCard(order) {
		var statusText = getStatusText(order.status);
		var statusClass = getStatusClass(order.status);
		var totalAmount = formatCurrency(order.totalAmount || order.totalPrice);
		var creationTime = formatDate(order.creationTime);

		var productCount = order.orderDetails ? order.orderDetails.length : 0;
		var firstProductName = 'Sản phẩm';
		if (order.orderDetails && order.orderDetails.length > 0) {
			var firstDetail = order.orderDetails[0];
			firstProductName = firstDetail.productVariant ? firstDetail.productVariant.productName : 'Sản phẩm';
			if (productCount > 1) {
				firstProductName += ' và ' + (productCount - 1) + ' sản phẩm khác';
			}
		}

		var recipientName = order.fullName || 'Không có thông tin';
		var address = getFullAddress(order);

		// Nút hủy đơn: cho phép hủy khi status = -1 (Chờ thanh toán) hoặc 0 (Chờ xác nhận)
		var cancelButton = '';
		if (order.status === 0 || order.status === -1) {
			cancelButton = '<button class="btn btn-outline-danger btn-sm cancel-order ms-2" data-order-id="' + order.id + '">Hủy đơn</button>';
		}

		return '<div class="order-card card mb-3" data-order-id="' + order.id + '">' +
			'<div class="card-body p-3">' +
			'<div class="d-flex justify-content-between align-items-start">' +
			'<div class="flex-grow-1">' +
			'<div class="d-flex align-items-center mb-1">' +
			'<span class="fw-bold me-2">#' + (order.code || order.id) + '</span>' +
			'<span class="badge ' + statusClass + '">' + statusText + '</span>' +
			'</div>' +
			'<div class="text-muted small mb-1">' + firstProductName + ' • ' + creationTime + '</div>' +
			'<div class="small"><strong>Người nhận:</strong> ' + recipientName + '</div>' +
			'<div class="small text-muted"><strong>Địa chỉ:</strong> ' + address + '</div>' +
			'</div>' +
			'<div class="text-end">' +
			'<div class="fw-bold text-primary mb-2">' + totalAmount + '</div>' +
			'<div>' +
			'<button class="btn btn-outline-primary btn-sm view-order-detail" data-order-id="' + order.id + '">Chi tiết</button>' +
			cancelButton +
			'</div>' +
			'</div>' +
			'</div>' +
			'<div class="order-detail-content mt-3" id="order-detail-' + order.id + '" style="display: none;">' +
			renderOrderDetails(order) +
			'</div>' +
			'</div>' +
			'</div>';
	}

	function renderOrderDetails(order) {
		if (!order.orderDetails || order.orderDetails.length === 0) {
			return '<div class="text-muted">Không có chi tiết đơn hàng</div>';
		}

		var html = '<div class="border-top pt-3"><h6 class="mb-3">Chi tiết sản phẩm: </h6>';

		order.orderDetails.forEach(function (detail, index) {
			var productName = detail.productVariant ? detail.productVariant.productName : 'Sản phẩm';
			var quantity = detail.quantity || 1;
			var price = formatCurrency(detail.price || detail.newPrice || 0);
			var totalPrice = formatCurrency((detail.price || detail.newPrice || 0) * quantity);

			html += '<div class="order-detail-item d-flex align-items-center mb-2 p-2 border-bottom">' +
				'<div class="me-3 text-muted small">' + (index + 1) + '.</div>' +
				'<div class="flex-grow-1">' +
				'<div class="fw-bold">' + productName + '</div>' +
				'<div class="text-muted small">Số lượng: ' + quantity + '</div>' +
				'</div>' +
				'<div class="text-end">' +
				'<div class="fw-bold">' + price + '</div>' +
				'<div class="text-muted small">Thành tiền: ' + totalPrice + '</div>' +
				'</div>' +
				'</div>';
		});

		html += '</div>';
		return html;
	}

	function huyUserOrder(orderId) {
		abp.message.confirm(
			'Bạn có chắc chắn muốn hủy đơn hàng này? ',
			'Xác nhận hủy đơn hàng',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy($('#orders-tab-pane'));

					_orderService.huyUserOrder(orderId)
						.done(function (result) {
							abp.notify.success('Hủy đơn hàng thành công!');
							loadOrders();
						})
						.fail(function (error) {
							console.error('Lỗi khi hủy đơn hàng:', error);
							abp.notify.error('Hủy đơn hàng thất bại:  ' + (error.message || ''));
						})
						.always(function () {
							abp.ui.clearBusy($('#orders-tab-pane'));
						});
				}
			}
		);
	}

	function getFullAddress(order) {
		var addressParts = [];
		if (order.diaChiChiTiet) addressParts.push(order.diaChiChiTiet);
		if (order.phuongXa) addressParts.push(order.phuongXa);
		if (order.tinhThanh) addressParts.push(order.tinhThanh);
		return addressParts.length > 0 ? addressParts.join(', ') : 'Không có địa chỉ';
	}

	function getStatusText(status) {
		var statusMap = {
			'-1': 'Chờ thanh toán',
			0: 'Chờ xác nhận',
			1: 'Đang xử lý',
			2: 'Đang giao hàng',
			3: 'Thành công',
			4: 'Đã hủy bởi hệ thống',
			5: 'Đã hủy',
			6: 'Thanh toán thất bại'
		};
		return statusMap[status] || 'Không xác định';
	}

	function getStatusClass(status) {
		var classMap = {
			'-1': 'bg-warning text-dark',
			0: 'bg-info text-dark',
			1: 'bg-info text-white',
			2: 'bg-primary text-white',
			3: 'bg-success text-white',
			4: 'bg-danger text-white',
			5: 'bg-secondary text-white',
			6: 'bg-danger text-white'
		};
		return classMap[status] || 'bg-light text-dark';
	}

	function formatCurrency(amount) {
		if (!amount) return '0 ₫';
		return new Intl.NumberFormat('vi-VN', {
			style: 'currency',
			currency: 'VND'
		}).format(amount);
	}

	function formatDate(dateString) {
		if (!dateString) return '';
		var date = new Date(dateString);
		return date.toLocaleDateString('vi-VN', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	function initializeDateRangePicker() {
		$('#StartEndRange').daterangepicker({
			autoUpdateInput: false,
			opens: 'left',
			locale: {
				format: 'DD/MM/YYYY',
				applyLabel: 'Áp dụng',
				cancelLabel: 'Hủy bỏ',
				fromLabel: 'Từ',
				toLabel: 'Đến',
				customRangeLabel: 'Phạm vi tùy chỉnh',
				firstDay: 1
			}
		});

		$('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
			$(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
			_selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm: ss');
			_selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
			loadOrders();
		});

		$('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
			$(this).val('');
			_selectedDateRange.StartTime = null;
			_selectedDateRange.EndTime = null;
			loadOrders();
		});
	}

	function bindEvents() {
		// Bộ lọc trạng thái
		$('.order-status-filters .nav-link').on('click', function (e) {
			e.preventDefault();

			$('.order-status-filters .nav-link').removeClass('active');
			$(this).addClass('active');

			var statusValue = $(this).data('status').toString();

			if (statusValue === 'all') {
				_currentStatus = null;
			} else {
				if (statusValue.includes(',')) {
					_currentStatus = statusValue.split(',').map(Number);
				} else {
					_currentStatus = [parseInt(statusValue)];
				}
			}

			loadOrders();
		});

		// Bộ lọc phương thức thanh toán
		$('#PaymentMethod').on('change', function () {
			var paymentMethodValue = $(this).val();
			_currentPaymentMethod = paymentMethodValue === '' ? null : parseInt(paymentMethodValue);
			loadOrders();
		});

		// Reset bộ lọc
		$('#ResetFilters').on('click', function () {
			_selectedDateRange.StartTime = null;
			_selectedDateRange.EndTime = null;
			_currentPaymentMethod = null;
			_currentStatus = null;
			$('#StartEndRange').val('');
			$('#PaymentMethod').val('');
			$('. order-status-filters .nav-link').removeClass('active');
			$('. order-status-filters .nav-link[data-status="all"]').addClass('active');
			loadOrders();
		});

		// Xem chi tiết đơn hàng
		$(document).on('click', '.view-order-detail', function () {
			var orderId = $(this).data('order-id');
			var $detailContent = $('#order-detail-' + orderId);
			var $button = $(this);

			if ($detailContent.is(':visible')) {
				$detailContent.hide();
				$button.text('Chi tiết');
			} else {
				$detailContent.show();
				$button.text('Ẩn chi tiết');
			}
		});

		// Hủy đơn hàng
		$(document).on('click', '.cancel-order', function () {
			var orderId = $(this).data('order-id');
			huyUserOrder(orderId);
		});
	}

})();