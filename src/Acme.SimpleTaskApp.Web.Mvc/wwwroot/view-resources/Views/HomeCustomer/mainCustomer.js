(function ($) {
	"use strict";

	// Fixed Navbar
	let lastScrollTop = 0;
	$(window).scroll(function () {
		let currentScroll = $(this).scrollTop();
		if (currentScroll > lastScrollTop) {
			$('.navbar2').slideUp(200);
		} else {
			$('.navbar2').slideDown(200);
		}
		lastScrollTop = currentScroll <= 0 ? 0 : currentScroll;
	});

	// Carousel sử dụng Bootstrap, khởi tạo khi DOM ready
	$(function () {
		var myCarouselEl = document.querySelector('#carouselId');
		if (myCarouselEl) {
			var carousel = new bootstrap.Carousel(myCarouselEl, {
				interval: 1000,
				pause: false
			});
			$('.carousel-control-prev').click(function () { carousel.prev(); });
			$('.carousel-control-next').click(function () { carousel.next(); });
		}
	});

	// Back to top button
	$(window).scroll(function () {
		if ($(this).scrollTop() > 300) {
			$('.back-to-top').fadeIn('slow');
		} else {
			$('.back-to-top').fadeOut('slow');
		}
	});
	$('.back-to-top').click(function () {
		$('html, body').animate({ scrollTop: 0 }, 1500, 'easeInOutExpo');
		return false;
	});

	// Product Quantity
	$('.quantity button').on('click', function () {
		var button = $(this);
		var input = button.closest('.quantity').find('input');
		var oldValue = parseFloat(input.val()) || 0;
		var newVal = button.hasClass('btn-plus') ? oldValue + 1 : Math.max(0, oldValue - 1);
		input.val(newVal);
	});

	// Product Details
	window.goToDetail = function (productId) {
		window.location.href = `/Product/DetailProductCustomer?id=${productId}`;
	};

	$(document).on('click', '.product-click-detail', function () {
		var productId = $(this).data('id');
		window.location.href = '/HomeCustomer/DetailProductCustomer?id=' + productId;
	});

	// ABP ModalManager cho Create Product (chuẩn abp)
	var _productFilterModal = new app.ModalManager({
		viewUrl: abp.appPath + 'HomeCustomer/FilterAdvancedModal',
		scriptUrl: abp.appPath + 'view-resources/Views/HomeCustomer/_FilterAdvancedModal.js',
		modalClass: 'FilterAdvancedModal',
		modalSize: 'modal-lg'
	});
	$('#btnOpenFilterModal').click(function () {
		_productFilterModal.open();
	});

	// Shop
	$(document).on('click', '.click-shop', function () {
		window.location.href = '/HomeCustomer/ShopProductCusTomer';
	});

	// Search xử lý chuẩn abp (có thể dùng ajax nếu muốn)
	$(function () {
		const searchInput = $('.txt-search');
		const searchButton = $('#searchButton');

		function handleSearch() {
			const keyword = searchInput.val().trim();
			if (keyword) {
				window.location.href = '/HomeCustomer/SearchProductCustomer?filter=' + encodeURIComponent(keyword);
			} else {
				abp.notify.warn('Vui lòng nhập từ khóa tìm kiếm');
			}
		}

		searchButton.on('click', handleSearch);
		searchInput.on('keypress', function (e) {
			if (e.which === 13) {
				e.preventDefault();
				handleSearch();
			}
		});
	});

	// Tìm kiếm theo danh mục
	$(document).on('click', '.category-click-getid', function () {
		var categoryId = $(this).data('id');
		window.location.href = '/HomeCustomer/SearchProductCustomer?category=' + categoryId;
	});
	$(document).on('click', '.category-click-getall', function () {
		window.location.href = '/HomeCustomer';
	});
	$(document).on('click', '.firt-button', function () {
		window.location.href = '/HomeCustomer?page=1';
	});


	// Xem chi tiết sản phẩm (chuẩn abp)
	$(document).on('click', '.detail-product', function () {
		var productId = $(this).data("product-id");
		abp.ajax({
			url: abp.appPath + 'Products/DetailModal?productId=' + productId,
			type: 'GET',
			dataType: 'html',
			success: function (content) {
				$('#ProductDetailModal .modal-content').html(content);
				$('#ProductDetailModal').modal('show');
			},
			error: function () {
				abp.notify.error('Không thể tải chi tiết sản phẩm');
			}
		});
	});

})(jQuery);