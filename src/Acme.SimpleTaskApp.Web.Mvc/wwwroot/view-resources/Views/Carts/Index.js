(function ($) {
	var _cartItemService = abp.services.app.cartItem;
	var _cartService = abp.services.app.cart

	var _OrderInfoModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Carts/OrderInfoModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Carts/_OrderInfoModal.js',
		modalClass: 'OrderInfoModal',
		modalSize: 'modal-lg'
	});

	$('#OrderInfoButton').click(function () {
		_OrderInfoModal.open({}, function (result) {
			if (result) {
			console.log(result)
				$('#FullName').text(result.userInfo.surname + result.userInfo.name);
				$('#PhoneNumber').text(result.userInfo.phoneNumber);
				$('#DiaChi').text(result.address.diaChiChiTiet + result.address.phuongXa.name + result.address.tinhThanh.name);
			}
		});
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

	// Xử lý đặt hàng với AJAX và hiển thị vị trí trong queue
	//$('form[action*="CreateOrder"]').on('submit', function (e) {
	//	e.preventDefault();
		
	//	var $form = $(this);
	//	var paymentMethod = $form.find('input[name="PaymentMethod"]:checked').val();
		
	//	abp.message.confirm(
	//		'Bạn có chắc chắn muốn đặt hàng?',
	//		'Xác nhận đặt hàng',
	//		function (isConfirmed) {
	//			if (isConfirmed) {
	//				// Hiển thị loading với thông báo đang xử lý
	//				var $loadingMessage = abp.message.info(
	//					'Đơn hàng của bạn đang được xử lý...<br/>Vui lòng đợi trong giây lát.',
	//					'Đang xử lý',
	//					{
	//						isHtml: true
	//					}
	//				);
					
	//				abp.ui.setBusy();
					
	//				$.ajax({
	//					url: abp.appPath + 'Orders/CreateOrder',
	//					type: 'POST',
	//					data: { PaymentMethod: paymentMethod },
	//					success: function (result) {
	//						abp.ui.clearBusy();
							
	//						// Đóng message loading
	//						if ($loadingMessage && $loadingMessage.close) {
	//							$loadingMessage.close();
	//						}
							
	//						// Nếu trả về JSON (lỗi)
	//						if (result.success === false) {
	//							abp.message.error(result.message || 'Đặt hàng thất bại', 'Lỗi');
	//							return;
	//						}
							
	//						// Nếu thành công
	//						abp.message.success(
	//							'Đặt hàng thành công!<br/>Cảm ơn bạn đã mua hàng.',
	//							'Thành công',
	//							{
	//								isHtml: true
	//							}
	//						)
	//						abp.services.app.sendMail.sendMailOrder()
	//							.done(function () {
	//								abp.notify.success('Email xác nhận đơn hàng đã được gửi!');
	//							})
	//							.fail(function (error) {
	//								abp.notify.error('Không thể gửi email xác nhận đơn hàng.');
	//								console.error(error);
	//							});

	//						// Redirect đến trang orders sau khi đặt hàng thành công
	//						window.location.href = abp.appPath + 'Orders/IndexForCustomer';
	//					},
	//					error: function (xhr) {
	//						abp.ui.clearBusy();
							
	//						// Đóng message loading
	//						if ($loadingMessage && $loadingMessage.close) {
	//							$loadingMessage.close();
	//						}
							
	//						var errorMessage = 'Đã xảy ra lỗi khi đặt hàng';
							
	//						if (xhr.responseJSON && xhr.responseJSON.error) {
	//							errorMessage = xhr.responseJSON.error.message || errorMessage;
								
	//							// Nếu lỗi do hết hàng, hiển thị thông báo rõ ràng
	//							if (errorMessage.includes('chỉ còn')) {
	//								abp.message.warn(
	//									'<i class="fas fa-exclamation-triangle"></i> ' + errorMessage,
	//									'Sản phẩm hết hàng',
	//									{
	//										isHtml: true
	//									}
	//								);
	//								return;
	//							}
	//						}
							
	//						abp.message.error(errorMessage, 'Lỗi đặt hàng');
	//					}
	//				});
	//			}
	//		}
	//	);
		
	//	return false;
	//});
})(jQuery);
