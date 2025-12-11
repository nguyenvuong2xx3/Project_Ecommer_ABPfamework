(function ($) {
	var _cartItemService = abp.services.app.cartItem;
	var _cartService = abp.services.app.cart
	var _orderService = abp.services.app.orders; // Thêm service order
	var _saleService = abp.services.app.sale;

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
		// Kiểm tra phương thức thanh toán
		var paymentMethod = $('input[name="PaymentMethod"]:checked').val();
		
		if (paymentMethod === '2') {
			// VNPay payment
			processVNPayOrder(_latestUserInfo);
		} else {
			// COD or Bank Transfer
			processOrder(_latestUserInfo);
		}
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
		// cập nhật ở trên navbar
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
			$(`#quantity-${productvariantId}`).val(quantity)
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
	function getProductIdFromUrl() {
		const urlParams = new URLSearchParams(window.location.search);
		return urlParams.get('id');
	}

	$(document).on('click', '.add-buy', function (e) {
		e.preventDefault();
		var productvariantId = getProductIdFromUrl();
		var quantity = 1;
		addToCart(productvariantId, quantity);
		(function ($) {
			$(document).ready(function () {
				// Điều hướng vào HomeCustomer/Cart
				window.location.href = abp.appPath + 'HomeCustomer/Cart';
			});
		})(jQuery);
	});

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

	$("#applyVoucherBtn").click(function () {
		applyVoucher();
	});

	function applyVoucher() {
		var voucherCode = $('#voucherCodeInput').val().trim();

		if (!voucherCode) {
			showVoucherMessage('Vui lòng nhập mã voucher. ', 'error');
			return;
		}

		var cartItems = collectCartItemsForDiscount();

		if (cartItems.length === 0) {
			showVoucherMessage('Không có sản phẩm trong giỏ hàng.', 'error');
			return;
		}

		// Ẩn message cũ và đợi animation xong
		var $message = $('#VoucherMessage');
		$message.stop(true, true).hide();

		abp.ui.setBusy();

		_saleService.calculateCartDiscount({
			cartItems: cartItems,
			voucherCode: voucherCode
		}).done(function (result) {
			var data = result.result !== undefined ? result.result : result;

			if (data.isSuccess === true) {
				showVoucherMessage(data.message, 'success');
				updateOrderSummary(data);
				$('#voucherCodeInput').data('applied-voucher', voucherCode);
				$('#applyVoucherBtn').prop('disabled', true);
			} else {
				showVoucherMessage(data.message || 'Voucher không hợp lệ', 'error');
				clearVoucherData();
			}
		}).fail(function (error) {
			var errorMsg = 'Có lỗi xảy ra khi áp dụng voucher';
			if (error && error.message) {
				errorMsg += ':  ' + error.message;
			}
			showVoucherMessage(errorMsg, 'error');
			clearVoucherData();
		}).always(function () {
			abp.ui.clearBusy();
		});
	}

	function showVoucherMessage(message, type) {
		var $message = $('#VoucherMessage');

		// Dừng mọi animation và reset về trạng thái ẩn
		$message.stop(true, true).hide();

		// Reset classes
		$message.removeClass('alert alert-success alert-danger alert-warning');

		// Add appropriate classes
		if (type === 'success') {
			$message.addClass('alert alert-success');
		} else {
			$message.addClass('alert alert-danger');
		}

		// Set content
		$message.html(message);

		// Hiển thị ngay lập tức bằng show(), không dùng fadeIn
		$message.show();
	}

	function clearVoucherData() {
		$('#voucherCodeInput').removeData('applied-voucher');
		$('#applyVoucherBtn').prop('disabled', false);
	}

	function resetVoucher() {
		clearVoucherData();
		$('#VoucherMessage').stop(true, true).hide();
	}

	function showVoucherMessage(message, type) {
		var $message = $('#VoucherMessage');

		// Reset classes
		$message.removeClass('alert alert-success alert-danger alert-warning');

		// Add appropriate classes
		if (type === 'success') {
			$message.addClass('alert alert-success');
		} else if (type === 'error') {
			$message.addClass('alert alert-danger');
		} else {
			$message.addClass('alert alert-warning');
		}

		// Set content và hiển thị
		$message.html(message);
		$message.stop(true, true).fadeIn(300);
	}

	function hideVoucherMessage() {
		$('#VoucherMessage').stop(true, true).fadeOut(200);
	}

	// Chỉ clear data, không ẩn message
	function clearVoucherData() {
		$('#voucherCodeInput').removeData('applied-voucher');
		$('#applyVoucherBtn').prop('disabled', false);
	}

	// Reset toàn bộ (dùng khi cần clear cả message)
	function resetVoucher() {
		clearVoucherData();
		// Nếu muốn ẩn message khi reset, uncomment dòng dưới
		// hideVoucherMessage();
	}

	function updateOrderSummary(data) {
		// Update UI với discount data
		if (data.discountAmount > 0) {
			// Hiển thị số tiền giảm
			$('. discount-amount').text('-' + formatCurrency(data.discountAmount));
			$('.discount-row').show();
		}

		// Update tổng tiền cuối cùng
		$('. final-amount').text(formatCurrency(data.finalAmount));
	}

	// Helper function format tiền VNĐ
	function formatCurrency(amount) {
		return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
	}

	// Hàm thu thập danh sách sản phẩm trong giỏ hàng để tính discount
	function collectCartItemsForDiscount() {
		var cartItems = [];

		$('.cart-item').each(function () {
			var $card = $(this);

			var $plusButton = $card.find('.btl-click-plus');

			var productVariantId = Number($plusButton.data('productvariant-id'));
			var productId = Number($plusButton.data('product-id'));
			var categoryId = $plusButton.data('category-id') !== undefined
				? Number($plusButton.data('category-id'))
				: null;

			var quantity = Number($card.find('input[type="text"].form-control').val());

			// Lấy giá sản phẩm: bỏ dấu chấm, phẩy trước khi convert
			var priceText = $card.find('.text-primary.mb-0').first().text().replace(/[^\d]/g, '');
			var price = Number(priceText); // ⇦ dạng số → backend nhận decimal OK

			cartItems.push({
				productVariantId,
				productId,
				categoryId,
				price,
				quantity
			});
		});

		return cartItems;
	}

	// Hàm hiển thị thông báo voucher
	function showVoucherMessage(message, type) {
		var $message = $('#VoucherMessage');


		$message.removeClass('alert-success alert-danger');
		$message.addClass(type === 'success' ? 'alert alert-success' : 'alert alert-danger');
		$message.html(message).show();
	}

	// Hàm cập nhật tổng đơn hàng sau khi áp dụng voucher
	function updateOrderSummary(result) {
		// Cập nhật phần giảm giá
		$('.discount-amount').text('-' + result.discountAmount.toLocaleString('vi-VN') + 'đ');

		// Cập nhật tổng cộng
		$('.final-amount').text(result.finalAmount.toLocaleString('vi-VN') + 'đ');
		
		// Update hidden total for VNPay
		$('#totalAmountValue').val(result.finalAmount);
		
		// Hiển thị thông tin voucher đã áp dụng (nếu có)
		if (result.appliedVoucher) {
			var voucherInfo = ' (Mã: ' + result.appliedVoucher.voucherCode + ' - Giảm ' + result.appliedVoucher.discountPercentage + '%)';
			$('.discount-amount').append(voucherInfo);
		}
	}

	// Hàm reset voucher (khi có thay đổi giỏ hàng)
	function resetVoucher() {
		$('#VoucherCodeInput').val('').data('applied-voucher', '');
		$('#ApplyVoucherBtn').prop('disabled', false);
		$('#VoucherMessage').hide();

		// Reset phần giảm giá về 0
		var originalTotal = calculateOriginalTotal();
		$('.discount-amount').text('- 0đ');
		$('.final-amount').text(originalTotal.toLocaleString('vi-VN') + 'đ');
		$('.total-cart').text(originalTotal.toLocaleString('vi-VN') + 'đ');
		$('#totalAmountValue').val(originalTotal);
	}

	// Hàm tính tổng tiền gốc
	function calculateOriginalTotal() {
		var total = 0;
		$('.cart-item').each(function () {
			var $card = $(this);
			var quantityInput = $card.find('input[type="text"].form-control');
			var quantity = parseInt(quantityInput.val()) || 1;
			var $priceElement = $card.find('.text-primary.mb-0').first();
			var priceText = $priceElement.text().replace(/[^\d]/g, '');
			var price = parseFloat(priceText) || 0;
			total += price * quantity;
		});
		return total;
	}

	$(".btl-click-plus, .btl-click-minus, .btl-click-delete").click(function () {
		setTimeout(resetVoucher, 100);
	});

	// ========== VNPay Integration ==========
	function processVNPayOrder(userInfo) {
		// Validate thông tin người nhận
		if (!validateUserInfo(userInfo)) {
			abp.notify.warn('Vui lòng cập nhật đầy đủ thông tin nhận hàng trước khi thanh toán.');
			$('#OrderInfoButton').click();
			return;
		}

		const orderData = collectOrderData(userInfo);
		
		// Show confirmation dialog
		var totalAmount = parseFloat($('#totalAmountValue').val());
		var formattedAmount = totalAmount.toLocaleString('vi-VN') + 'đ';
		
		abp.message.confirm(
			'Bạn sẽ được chuyển đến trang thanh toán VNPay để thanh toán số tiền: ' + formattedAmount,
			'Xác nhận thanh toán VNPay',
			function (isConfirmed) {
				if (isConfirmed) {
					submitVNPayOrder(orderData, totalAmount);
				}
			}
		);
	}

	function submitVNPayOrder(orderData, totalAmount) {
		abp.ui.setBusy();
		console.log('Creating VNPay order:', orderData);

		// Tạo đơn hàng trước
		_orderService.createOrder(orderData)
			.done(function (orderId) {
				abp.notify.info('Đang chuyển đến trang thanh toán VNPay...');

				// Tạo payment request
				var paymentRequest = {
					orderId: orderId.toString(),
					amount: totalAmount,
					orderDescription: 'Thanh toán đơn hàng #' + orderId
				};

				// Gọi service VNPay để tạo payment URL
				abp.services.app.vnpay.createPaymentUrl(paymentRequest)
					.done(function (response) {
						debugger
						if (response.success && response.paymentUrl) {
							// Redirect to VNPay
							window.location.href = response.paymentUrl;
						} else {
							abp.notify.error(response.message || 'Không thể tạo link thanh toán VNPay');
							abp.ui.clearBusy();
						}
					})
					.fail(function (error) {
						debugger
						var errorMsg = error.message || error.error?.message || 'Có lỗi xảy ra khi tạo link thanh toán';
						abp.notify.error(errorMsg);
						console.error('VNPay payment error:', error);
						abp.ui.clearBusy();
					});
			})
			.fail(function (error) {
				const errorMsg = error.message || error.error?.message || 'Vui lòng thử lại';
				abp.notify.error('Tạo đơn hàng thất bại: ' + errorMsg);
				console.error('Order error:', error);
				abp.ui.clearBusy();
			});
	}

	function validateUserInfo(userInfo) {
		// Check if user info is complete
		var fullName = $('#FullName').text().trim();
		var phoneNumber = $('#PhoneNumber').text().trim();
		var address = $('#DiaChi').text().trim();
		
		return fullName && phoneNumber && address && 
			   fullName !== '' && 
			   phoneNumber !== 'Không có số điện thoại' &&
			   address !== '';
	}

	// ========== Regular Order (COD/Bank Transfer) ==========
	function processOrder(userInfo) {
		const orderData = collectOrderData(userInfo);
		submitOrder(orderData);
	}

	function collectOrderData(userInfo) {
		const orderDetails = [];
		let totalAmount = 0;

		$('.cart-item').each(function () {
			const $card = $(this);
			const variantElement = $card.find('.btl-click-plus')
			const productVariantId = variantElement.data('productvariant-id');

			let input = $(`#quantity-${productVariantId}`);
			let quantity = parseInt(input.val());

			// Get price (đã bao gồm automatic discount nếu có)
			const $priceElement = $card.find('.text-primary');
			const priceText = $priceElement.text().replace(/[^\d]/g, '');
			const price = parseFloat(priceText) || 0;

			const total = price * quantity;

			orderDetails.push({
				productVariantId: productVariantId,
				quantity: quantity,
				newPrice: price
			});

			totalAmount += total;
		});

		const paymentMethod = $('input[name="PaymentMethod"]:checked').val();

		const order = {
			userId: abp.session.userId || null,
			paymentMethod: parseInt(paymentMethod),
			status: 0,
			totalPrice: parseFloat($('#totalAmountValue').val()) || totalAmount
		};

		if (userInfo && userInfo.userInfo) {
			order.fullName = `${userInfo.userInfo.surname} ${userInfo.userInfo.name}`.trim();
			order.gioiTinh = userInfo.userInfo.gioiTinh || null;
		}

		if (userInfo && userInfo.address) {
			order.tinhThanh = userInfo.address.tinhThanh?.name || null;
			order.phuongXa = userInfo.address.phuongXa?.name || null;
			order.diaChiChiTiet = userInfo.address.diaChiChiTiet || null;
		}

		const voucherCode = $('#voucherCodeInput').val();
		
		return {
			order: order,
			orderDetails: orderDetails,
			voucherCode: voucherCode
		};
	}

	function submitOrder(orderData) {
		abp.ui.setBusy();
		console.log('Order data:', orderData);

		_orderService.createOrder(orderData)
			.done(function (orderId) {
				abp.notify.success('Đặt hàng thành công!');

				setTimeout(function () {
					window.location.href = abp.appPath + 'Orders/OrderConfirmation?orderId=' + orderId;
				}, 2000);
			})
			.fail(function (error) {
				const errorMsg = error.message || error.error?.message || 'Vui lòng thử lại';
				abp.notify.error('Đặt hàng thất bại: ' + errorMsg);
				console.error('Order error:', error);
			})
			.always(function () {
				abp.ui.clearBusy();
			});
	}

})(jQuery);