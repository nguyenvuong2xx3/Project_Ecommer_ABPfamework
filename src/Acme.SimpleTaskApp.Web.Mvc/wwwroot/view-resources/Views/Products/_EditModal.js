(function ($) {
	app.modals.ProductEditModal = function () {
		var _modalManager;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');

		let newImageFiles = new DataTransfer();   // ảnh mới
		let deletedImageUrls = [];                // ảnh cũ bị xóa
		let isImageUploaderInitialized = false;

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=EditProductForm]');

			// Lấy ProductId từ form hoặc data attribute
			var productId = _$form.find('input[name=Id]').val() || $modal.data('product-id');

			// Validate
			if ($.fn.validate) {
				_$form.validate({
					validClass: 'valid',
					errorClass: 'invalid-feedback',
					highlight: (el) => $(el).addClass('is-invalid').removeClass('is-valid'),
					unhighlight: (el) => $(el).addClass('is-valid').removeClass('is-invalid'),
					rules: {
						Name: { required: true, minlength: 5, maxlength: 256 },
						CategoryId: { required: true },
						Description: { maxlength: 500 }
					},
					messages: {
						Name: { required: 'Tên sản phẩm không được để trống', minlength: 'Tối thiểu 5 ký tự' },
						CategoryId: { required: 'Vui lòng chọn danh mục' },
						Description: { maxlength: 'Mô tả không được vượt quá 500 ký tự' }
					},
					errorPlacement: function (error, element) {
						error.addClass('text-danger').insertAfter(element);
					}
				});
			}

			// Setup uploader
			if (!isImageUploaderInitialized) {
				setupImageUploader($modal);
				isImageUploaderInitialized = true;
			}

			// Focus
			setTimeout(() => _$form.find('input[name=Name]').first().focus(), 250);
		};

		function setupImageUploader($modal) {
			const imageUploader = $modal.find('.image-uploader');
			const imageInput = $modal.find('#ImageFiles');
			const previewContainer = $modal.find('#imagePreviewContainer');

			if (!imageUploader.length || !imageInput.length) {
				console.error('Image uploader elements not found');
				return;
			}

			// Reset lại data
			newImageFiles = new DataTransfer();
			deletedImageUrls = [];

			// Gán event cho ảnh cũ (chỉ xử lý xóa)
			previewContainer.find('.img-preview-wrapper').each(function () {
				const $wrapper = $(this);
				const imgUrl = $wrapper.find('img').attr('src');
				const $removeBtn = $wrapper.find('.remove-img-btn');

				$removeBtn.off('click').on('click', function (e) {
					e.preventDefault();
					e.stopPropagation();

					// Ghi nhớ ảnh bị xóa
					deletedImageUrls.push(imgUrl);
					$wrapper.remove();
				});
			});

			// Xử lý ảnh mới
			imageUploader.off('click dragover dragleave drop');
			imageInput.off('change');

			imageUploader.on('click', function (e) {
				if (!$(e.target).is('input[type="file"]')) {
					e.preventDefault();
					imageInput.trigger('click');
				}
			});

			imageUploader.on('dragover', function (e) {
				e.preventDefault();
				$(this).css('background-color', '#e9ecef');
			});

			imageUploader.on('dragleave drop', function (e) {
				e.preventDefault();
				$(this).css('background-color', 'transparent');
			});

			imageUploader.on('drop', function (e) {
				const files = e.originalEvent.dataTransfer.files;
				handleNewFiles(files, previewContainer);
			});

			imageInput.on('change', function (e) {
				handleNewFiles(e.target.files, previewContainer);
			});
		}

		function handleNewFiles(files, previewContainer) {
			if (!files || files.length === 0) return;

			for (let i = 0; i < files.length; i++) {
				const file = files[i];
				if (!file.type.startsWith('image/')) continue;

				newImageFiles.items.add(file);

				const reader = new FileReader();
				reader.onload = function (e) {
					const preview = createNewImagePreview(e.target.result, file);
					previewContainer.append(preview);
				};
				reader.readAsDataURL(file);
			}
			$('#ImageFiles')[0].files = newImageFiles.files;
		}

		function createNewImagePreview(src, file) {
			const wrapper = $('<div>').addClass('img-preview-wrapper');
			const img = $('<img>').attr('src', src).addClass('img-preview');
			const removeBtn = $('<button>').html('&times;').addClass('remove-img-btn').attr('type', 'button');

			removeBtn.on('click', function (e) {
				e.preventDefault();
				const newFiles = new DataTransfer();
				for (let i = 0; i < newImageFiles.files.length; i++) {
					if (newImageFiles.files[i] !== file) {
						newFiles.items.add(newImageFiles.files[i]);
					}
				}
				newImageFiles = newFiles;
				$('#ImageFiles')[0].files = newImageFiles.files;
				wrapper.remove();
			});

			wrapper.append(img, removeBtn);
			return wrapper;
		}

		this.save = function () {
			if (!_$form || _$form.length === 0) {
				console.error("Form không tồn tại trong modal!");
				return;
			}

			// Kiểm tra validator có hoạt động không
			if ($.fn.validate) {
				_$form.validate().form(); // <- ép chạy validate cho toàn form
				if (!_$form.valid()) {
					console.warn("Form không hợp lệ, dừng lại!");
					return;
				}
			}

			_modalManager.setBusy(true);

			const formData = new FormData(_$form[0]);
			const productId = _$form.find('input[name=Id]').val();
			if (productId) formData.set('Id', productId);

			// Gắn ảnh mới
			Array.from(newImageFiles.files).forEach((file, i) => {
				formData.append(`Images[${i}]`, file);
			});

			// Gắn url ảnh bị xóa
			deletedImageUrls.forEach((url, i) => {
				formData.append(`DeletedImageUrls[${i}]`, url);
			});

			console.log(">>> Dữ liệu gửi đi:", Array.from(formData.entries()));

			$.ajax({
				url: abp.appPath + 'Products/EditProduct',
				type: 'POST',
				processData: false,
				contentType: false,
				data: formData,
				success: function (res) {
					console.log("Phản hồi thành công:", res);
					_modalManager.setBusy(false);
					abp.notify.info(l('Cập nhật thành công'));
					_modalManager.close();
					setTimeout(() => window.location.href = abp.appPath + 'Products', 500);
				},
				error: function (xhr) {
					_modalManager.setBusy(false);
					console.error("Lỗi AJAX:", xhr);
					const msg = xhr.responseJSON?.error?.message || "Có lỗi xảy ra khi cập nhật sản phẩm.";
					abp.message.error(msg);
				}
			});
		};

	};
})(jQuery);
