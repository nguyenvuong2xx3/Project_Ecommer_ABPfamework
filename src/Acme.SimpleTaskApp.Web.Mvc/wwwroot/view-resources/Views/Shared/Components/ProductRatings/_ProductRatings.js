(function ($) {
	var _ratingService = abp.services.app.productRating;
	var _productVariantId = $('#ProductVariantIdForRating').val();
	var _uploadedImages = [];
	var _maxImages = 5;
	var _maxImageSize = 5 * 1024 * 1024; // 5MB
	var _currentRatingId = null;

	$(document).ready(function () {
		initializeRatingForm();
		initializeImageUpload();
		initializeRatingActions();
		initializeFilterTabs();
	});

	// ========== KHỞI TẠO FORM ĐÁNH GIÁ ==========
	function initializeRatingForm() {
		// Hiển thị form đánh giá
		$('#btnShowRatingForm, #btnEditRating').on('click', function () {
			var ratingId = $(this).data('rating-id');
			if (ratingId) {
				loadRatingForEdit(ratingId);
			} else {
				resetRatingForm();
			}
			$('#ratingFormContainer').slideDown();
			$('html, body').animate({
				scrollTop: $('#ratingFormContainer').offset().top - 100
			}, 500);
		});

		// Hủy đánh giá
		$('#btnCancelRating').on('click', function () {
			$('#ratingFormContainer').slideUp();
			resetRatingForm();
		});

		// Nhập số sao
		$('#starRatingInput i').on('click', function () {
			var rating = $(this).data('rating');
			setStarRating(rating);
		});

		// Hiệu ứng hover cho sao
		$('#starRatingInput i').hover(
			function () {
				var rating = $(this).data('rating');
				highlightStars(rating);
			},
			function () {
				var currentRating = $('#RatingValue').val();
				highlightStars(currentRating);
			}
		);

		// Đếm ký tự
		$('#RatingTitle').on('input', function () {
			$('#titleCharCount').text($(this).val().length);
		});

		$('#ReviewText').on('input', function () {
			$('#reviewCharCount').text($(this).val().length);
		});

		// Gửi form đánh giá
		$('#btnSubmitRating').on('click', function () {
			submitRating();
		});
	}

	// ========== HÀM XỬ LÝ SỐ SAO ==========
	function setStarRating(rating) {
		$('#RatingValue').val(rating);
		highlightStars(rating);
		updateRatingText(rating);
	}

	function highlightStars(rating) {
		$('#starRatingInput i').each(function (index) {
			if (index < rating) {
				$(this).removeClass('far').addClass('fas');
			} else {
				$(this).removeClass('fas').addClass('far');
			}
		});
	}

	function updateRatingText(rating) {
		var texts = {
			1: 'Rất tệ',
			2: 'Tệ',
			3: 'Bình thường',
			4: 'Tốt',
			5: 'Rất tốt'
		};
		$('#ratingText').text(texts[rating] || 'Chọn số sao');
	}

	// ========== TẢI ẢNH ==========
	function initializeImageUpload() {
		var $uploadArea = $('#imageUploadArea');
		var $fileInput = $('#RatingImages');

		// Click để chọn ảnh
		$uploadArea.on('click', function () {
			$fileInput.click();
		});

		// Thay đổi input file
		$fileInput.on('change', function (e) {
			handleFiles(e.target.files);
		});

		// Kéo thả
		$uploadArea.on('dragover', function (e) {
			e.preventDefault();
			e.stopPropagation();
			$(this).addClass('dragover');
		});

		$uploadArea.on('dragleave', function (e) {
			e.preventDefault();
			e.stopPropagation();
			$(this).removeClass('dragover');
		});

		$uploadArea.on('drop', function (e) {
			e.preventDefault();
			e.stopPropagation();
			$(this).removeClass('dragover');

			var files = e.originalEvent.dataTransfer.files;
			handleFiles(files);
		});
	}

	function handleFiles(files) {
		if (!files || files.length === 0) return;

		// Kiểm tra số ảnh tối đa
		if (_uploadedImages.length + files.length > _maxImages) {
			abp.notify.warn('Bạn chỉ được tải lên tối đa ' + _maxImages + ' ảnh');
			return;
		}

		Array.from(files).forEach(file => {
			// Validate loại file
			if (!file.type.match('image.*')) {
				abp.notify.warn('File ' + file.name + ' không phải là ảnh');
				return;
			}

			// Kích thước
			if (file.size > _maxImageSize) {
				abp.notify.warn('Ảnh ' + file.name + ' quá lớn (tối đa 5MB)');
				return;
			}

			uploadImage(file);
		});
	}

	function uploadImage(file) {
		var formData = new FormData();
		formData.append('file', file);

		abp.ui.setBusy($('#imageUploadArea'));

		$.ajax({
			url: abp.appPath + 'UploadFile/UploadImageReturnUrl',
			type: 'POST',
			data: formData,
			processData: false,
			contentType: false,
			success: function (response) {
				if (response.success && response.result) {
					_uploadedImages.push(response.result);
					addImagePreview(response.result);
					updateImageUrlsInput();
					abp.notify.success('Tải ảnh lên thành công');
				} else {
					abp.notify.error('Tải ảnh lên thất bại');
				}
			},
			error: function () {
				abp.notify.error('Có lỗi xảy ra khi tải ảnh lên');
			},
			complete: function () {
				abp.ui.clearBusy($('#imageUploadArea'));
				$('#RatingImages').val(''); // Reset file input
			}
		});
	}

	function addImagePreview(imageUrl) {
		var $preview = $(`
            <div class="image-preview-item">
                <img src="${imageUrl}" alt="Preview" />
                <button type="button" class="remove-image" data-url="${imageUrl}">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `);

		$preview.find('.remove-image').on('click', function () {
			var url = $(this).data('url');
			removeImage(url);
		});

		$('#imagePreviewContainer').append($preview);
	}

	function removeImage(imageUrl) {
		_uploadedImages = _uploadedImages.filter(url => url !== imageUrl);
		$(`[data-url="${imageUrl}"]`).closest('.image-preview-item').fadeOut(300, function () {
			$(this).remove();
		});
		updateImageUrlsInput();
	}

	function updateImageUrlsInput() {
		$('#ImageUrls').val(_uploadedImages.join(','));
	}

	// ========== GỬI ĐÁNH GIÁ ==========
	function submitRating() {
		var rating = parseInt($('#RatingValue').val());

		if (rating === 0 || isNaN(rating)) {
			abp.notify.warn('Vui lòng chọn số sao đánh giá');
			return;
		}

		var input = {
			productVariantId: parseInt(_productVariantId),
			rating: rating,
			title: $('#RatingTitle').val().trim(),
			reviewText: $('#ReviewText').val().trim(),
			imageUrls: $('#ImageUrls').val()
		};

		var isEdit = _currentRatingId != null;

		if (isEdit) {
			input.id = _currentRatingId;
			updateRating(input);
		} else {
			createRating(input);
		}
	}

	function createRating(input) {
		abp.ui.setBusy($('#ratingForm'));

		_ratingService.createRating(input)
			.done(function (result) {
				abp.notify.success('Đã gửi đánh giá thành công!');
				resetRatingForm();
				$('#ratingFormContainer').slideUp();

				// Tải lại đánh giá
				loadRatings();
			})
			.fail(function (error) {
				abp.notify.error(error.message || 'Có lỗi xảy ra khi gửi đánh giá');
			})
			.always(function () {
				abp.ui.clearBusy($('#ratingForm'));
			});
	}

	function updateRating(input) {
		abp.ui.setBusy($('#ratingForm'));

		_ratingService.updateRating(input)
			.done(function (result) {
				abp.notify.success('Đã cập nhật đánh giá!');
				resetRatingForm();
				$('#ratingFormContainer').slideUp();

				// Tải lại đánh giá
				loadRatings();
			})
			.fail(function (error) {
				abp.notify.error(error.message || 'Có lỗi khi cập nhật đánh giá');
			})
			.always(function () {
				abp.ui.clearBusy($('#ratingForm'));
			});
	}

	// ========== HÀNH ĐỘNG TRÊN ĐÁNH GIÁ ==========
	function initializeRatingActions() {
		// Sửa đánh giá
		$(document).on('click', '.btn-edit-rating', function () {
			var ratingId = $(this).data('rating-id');
			loadRatingForEdit(ratingId);
		});

		// Xóa đánh giá
		$(document).on('click', '.btn-delete-rating', function () {
			var ratingId = $(this).data('rating-id');
			deleteRating(ratingId);
		});

		// Bầu hữu ích
		$(document).on('click', '.btn-vote-helpful', function () {
			var ratingId = $(this).data('rating-id');
			var isHelpful = $(this).data('helpful') === 'true' || $(this).data('helpful') === true;
			voteRating(ratingId, isHelpful);
		});
	}

	function loadRatingForEdit(ratingId) {
		// Tìm rating trong danh sách hiện tại hoặc load từ server
		var $ratingItem = $(`.rating-item[data-rating-id="${ratingId}"]`);

		// Hiện tại chỉ mở form với ID
		// Trong môi trường production nên load đầy đủ dữ liệu đánh giá từ server
		_currentRatingId = ratingId;

		$('#ratingFormContainer').slideDown();
		$('html, body').animate({
			scrollTop: $('#ratingFormContainer').offset().top - 100
		}, 500);
	}

	function deleteRating(ratingId) {
		abp.message.confirm(
			'Bạn có chắc chắn muốn xóa đánh giá này?',
			'Xác nhận xóa',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy($('#ratings-list'));

					_ratingService.deleteRating(ratingId)
						.done(function () {
							abp.notify.success('Đã xóa đánh giá');
							loadRatings();
						})
						.fail(function (error) {
							abp.notify.error(error.message || 'Có lỗi khi xóa đánh giá');
						})
						.always(function () {
							abp.ui.clearBusy($('#ratings-list'));
						});
				}
			}
		);
	}

	function voteRating(ratingId, isHelpful) {
		var input = {
			productRatingId: ratingId,
			isHelpful: isHelpful
		};

		_ratingService.voteRatingHelpful(input)
			.done(function (result) {
				// Cập nhật số lượt vote trong UI
				var $ratingItem = $(`.rating-item[data-rating-id="${ratingId}"]`);
				$ratingItem.find('.helpful-count').text(result.helpfulCount);
				$ratingItem.find('.not-helpful-count').text(result.notHelpfulCount);

				// Cập nhật trạng thái active
				$ratingItem.find('.btn-vote-helpful').removeClass('active');
				if (result.currentUserVote === true) {
					$ratingItem.find('.btn-vote-helpful[data-helpful="true"]').addClass('active');
				} else if (result.currentUserVote === false) {
					$ratingItem.find('.btn-vote-helpful[data-helpful="false"]').addClass('active');
				}

				abp.notify.success('Cảm ơn đánh giá của bạn!');
			})
			.fail(function (error) {
				abp.notify.error(error.message || 'Có lỗi xảy ra');
			});
	}

	// ========== TAB LỌC ==========
	function initializeFilterTabs() {
		$('#ratingFilterTabs button').on('click', function () {
			var filter = $(this).data('filter');
			filterRatings(filter);
		});
	}

	function filterRatings(filter) {
		var input = {
			productVariantId: parseInt(_productVariantId),
			maxResultCount: 10,
			skipCount: 0
		};

		if (filter === 'verified') {
			input.isVerifiedPurchase = true;
		} else if (filter !== 'all') {
			input.rating = parseInt(filter);
		}

		abp.ui.setBusy($('#ratings-list'));

		_ratingService.getAllRatings(input)
			.done(function (result) {
				renderRatings(result.items);
			})
			.fail(function (error) {
				abp.notify.error('Có lỗi khi tải đánh giá');
			})
			.always(function () {
				abp.ui.clearBusy($('#ratings-list'));
			});
	}

	// ========== TẢI & RENDER ĐÁNH GIÁ ==========
	function loadRatings() {
		// Tải lại trang hiện tại
		location.reload();
	}

	function renderRatings(ratings) {
		var $list = $('#ratings-list');
		$list.empty();

		if (ratings && ratings.length > 0) {
			// Trong môi trường production, render HTML đánh giá tại đây
			// Hiện tại tạm thời reload trang
			location.reload();
		} else {
			$list.html(`
                <div class="text-center text-muted py-5">
                    <i class="fas fa-star-half-alt fa-3x mb-3"></i>
                    <p>Không có đánh giá nào phù hợp với bộ lọc</p>
                </div>
            `);
		}
	}


	// ========== HÀM HỖ TRỢ ==========
	function resetRatingForm() {
		_currentRatingId = null;
		$('#ratingForm')[0].reset();
		$('#RatingValue').val(0);
		highlightStars(0);
		updateRatingText(0);
		$('#titleCharCount').text(0);
		$('#reviewCharCount').text(0);
		_uploadedImages = [];
		$('#imagePreviewContainer').empty();
		$('#ImageUrls').val('');
	}

})(jQuery);
