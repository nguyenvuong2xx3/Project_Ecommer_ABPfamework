(function ($) {
	var _cartItemService = abp.services.app.cartItem;
	var _cartService = abp.services.app.cart
	var _orderService = abp.services.app.order; // Thêm service order

	// Biến lưu thông tin từ modal (nếu có)
	var _latestUserInfo = null;

	var _OrderInfoModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Carts/OrderInfoModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Carts/_OrderInfoModal.js',
		modalClass: 'OrderInfoModal',
		modalSize: 'modal-lg'
	});

	// Nút "Thay đổi địa chỉ"
	$('#OrderInfoButton').click(function () {
		_OrderInfoModal.open({}, function (result) {
			if (result) {
				console.log(result);
				_latestUserInfo = result; // Lưu thông tin mới nhất

				// Hiển thị họ tên có dấu cách giữa surname và name
				const fullName = `${result.userInfo.surname} ${result.userInfo.name}`.trim();
				$('#FullName').text(fullName);

				// Số điện thoại
				$('#PhoneNumber').text(result.userInfo.phoneNumber || "Không có số điện thoại");

				// Địa chỉ: ghép chi tiết, phường/xã, tỉnh/thành có dấu phẩy và cách
				const diaChi = `${result.address.diaChiChiTiet}, ${result.address.phuongXa.name}, ${result.address.tinhThanh.name}`;
				$('#DiaChi').text(diaChi);
			}
		});
	});

	// Nút "Đặt hàng" - THÊM SỰ KIỆN NÀY
	$(document).on('click', '#SubmitOrderButton', function () {
		processOrder(_latestUserInfo); // Truyền thông tin mới nhất (có thể null)
	});

	// Xử lý khi nhấn nút tăng
	$(document).on('click', '.btl-click-plus', function () {
		let productvariantId = $(this).data('productvariant-id');
		let input = $(`#quantity-${productvariantId}`);
		let cartId = $(this).data('productvariant-cartid');
		let quantity = parseInt(input.val()) + 1;
		// Lấy số hiện tại trong #cart-count
		var $cartCount = $('#cart-count');
		var currentCount = parseInt($cartCount.text()) || 0;
		var newCount = currentCount + 1;

		// Cập nhật lại số và hiện badge nếu đang ẩn
		$cartCount.text(newCount);
		if (newCount > 0) {
			$cartCount.css('display', 'inline-block');
		}
		updateCart(productvariantId, quantity, input, cartId);
	});

	// Xử lý khi nhấn nút giảm
	$(document).on('click', '.btl-click-minus', function () {
		let productvariantId = $(this).data('productvariant-id');
		let input = $(`#quantity-${productvariantId}`);
		let cartId = $(this).data('productvariant-cartid');
		let quantity = parseInt(input.val()) - 1;

		// Lấy số hiện tại trong #cart-count
		var $cartCount = $('#cart-count');
		var currentCount = parseInt($cartCount.text()) || 0;
		var newCount = currentCount > 0 ? currentCount - 1 : 0;

		// Nếu số lượng sau khi giảm <= 0, xóa sản phẩm khỏi giỏ hàng
		if (quantity < 1) {
			deleteCartItem(productvariantId, cartId, $cartCount);
		} else {
			// Giảm số lượng sản phẩm
			let newQuantity = quantity - 1;
			input.val(newQuantity);

			// Cập nhật giỏ hàng
			updateCart(productvariantId, quantity, input, cartId);

			// Giảm badge giỏ hàng
			$cartCount.text(newCount);
			if (newCount > 0) {
				$cartCount.css('display', 'inline-block');
			} else {
				$cartCount.css('display', 'none');
			}
		}
	});

	// Xử lý khi người dùng thay đổi trực tiếp giá trị trong input
	$(document).on('change', '.quantity-input', function () {
		let productvariantId = $(this).data('productvariant-id');
		let input = $(`#quantity-${productvariantId}`);
		let cartId = $(this).data('productvariant-cartid');
		let quantity = parseInt($(this).val());

		if (isNaN(quantity) || quantity < 1) {
			quantity = 1; // Đặt về giá trị tối thiểu là 1 nếu nhập sai
		}

		updateCart(productvariantId, quantity, input, cartId);
	});

	// Hàm cập nhật giỏ hàng
	function updateCart(productvariantId, quantity, input, cartId) {
		abp.ui.setBusy();
		_cartItemService.updateItem({
			productvariantId: productvariantId,
			cartId: cartId,
			quantity: quantity
		}).done(function () {
			input.val(quantity);
			const price = parseFloat($('#productvariant-price-' + productvariantId).text());
			const newTotal = price * quantity;
			$('#productvariant-total-' + productvariantId).text(newTotal.toLocaleString('vi-VN') + ' VND');
			abp.notify.info('Cập nhật thành công!');
		}).fail(function (error) {
			abp.notify.error('Cập nhật thất bại!');
			console.error(error);
		}).always(function () {
			abp.ui.clearBusy();
		});
	}

	$(document).on('click', '.btl-click-delete', function () {
		let productvariantId = $(this).data('productvariant-id');
		let cartId = $(this).data('productvariant-cartid');
		deleteCartItem(productvariantId, cartId);
	});

	function deleteCartItem(productvariantId, cartId) {
		abp.message.confirm(
			abp.utils.formatString(
				'Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?'),
			null,
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy();
					_cartItemService.deleteItem(productvariantId, cartId)
						.done(function () {
							abp.notify.info('Xóa thành công!');
							location.reload();
						})
						.fail(function (error) {
							abp.notify.error('Xóa thất bại!');
							console.error(error);
						})
						.always(function () {
							abp.ui.clearBusy();
						});
				} else {
					document.getElementById(`quantity-${productvariantId}`).value = 1;
				}
			}
		);
	}

	// Helper function to get productId from URL query string (Id=...)
	function getProductIdFromUrl() {
		const urlParams = new URLSearchParams(window.location.search);
		return urlParams.get('id');
	}

	$(document).on('click', '.add-to-cart', function (e) {
		e.preventDefault();
		var productvariantId = getProductIdFromUrl();
		var quantity = 1;
		addToCart(productvariantId, quantity);
	});
	function addToCart(productvariantId, quantity) {
		abp.ui.setBusy();
		_cartService.createCart(productvariantId, quantity)
			.done(function () {
				abp.notify.success('Thêm vào giỏ hàng thành công!');
				var $cartCount = $('#cart-count');
				var currentCount = parseInt($cartCount.text()) || 0;
				var newCount = currentCount + 1;

				// Cập nhật lại số và hiện badge nếu đang ẩn
				$cartCount.text(newCount);
				if (newCount > 0) {
					$cartCount.css('display', 'inline-block');
				}
			})
			.fail(function (error) {
				abp.notify.error('Thêm vào giỏ hàng thất bại đăng nhập để thêm vào giỏ hàng ');
			})
			.always(function () {
				abp.ui.clearBusy();
			});
	}

	function processOrder(userInfo) {
		// Thu thập thông tin đơn hàng
		const orderData = collectOrderData(userInfo);

		// Gọi API đặt hàng
		submitOrder(orderData);
	}

	// Hàm thu thập dữ liệu đơn hàng
	function collectOrderData(userInfo) {
		const orderItems = [];
		let totalAmount = 0;

		// Thu thập thông tin từng sản phẩm trong giỏ hàng
		$('.card.mb-4.border-0.shadow-sm').each(function () {
			const productVariantId = $(this).find('.btl-click-plus').data('productvariant-id');
			const quantityInput = $(this).find('input[type="text"].form-control');
			const quantity = parseInt(quantityInput.val()) || 1;
			const priceText = $(this).find('.text-primary.mb-0').first().text().replace(/[^\d]/g, '');
			const price = parseFloat(priceText) || 0;
			const total = price * quantity;

			orderItems.push({
				productVariantId: productVariantId,
				quantity: quantity,
				price: price,
				total: total
			});

			totalAmount += total;
		});

		// Lấy phương thức thanh toán
		const paymentMethod = $('input[name="PaymentMethod"]:checked').val();

		return {
			orderItems: orderItems,
			totalAmount: totalAmount,
			paymentMethod: parseInt(paymentMethod),
			userInfo: userInfo ? userInfo.userInfo : null, // Có thể null
			address: userInfo ? userInfo.address : null,   // Có thể null
			note: ''
		};
	}

	function submitOrder(orderData) {
		abp.ui.setBusy();
		console.log(orderData);
		_orderService.createOrder(orderData)
			.done(function (response) {
				abp.notify.success('Đặt hàng thành công!');

				// Chuyển hướng đến trang xác nhận đơn hàng hoặc trang chủ
				setTimeout(function () {
					window.location.href = abp.appPath + 'Orders/OrderConfirmation?orderId=' + response.orderId;
				}, 2000);
			})
			.fail(function (error) {
				abp.notify.error('Đặt hàng thất bại: ' + error.message);
				console.error('Order error:', error);
			})
			.always(function () {
				abp.ui.clearBusy();
			});
	}

})(jQuery);