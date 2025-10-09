(function ($) {
	app.modals.ProductEditModal = function () {
		var _modalManager;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');

		let newImageFiles = [];        // Mảng File object
		let deletedImageUrls = [];     // URL ảnh cũ bị xóa

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=EditProductForm]');

			// Validate
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
				errorPlacement: (error, element) => error.addClass('text-danger').insertAfter(element)
			});

			// Setup uploader
			setupImageUploader($modal);

			// Focus
			setTimeout(() => _$form.find('input[name=Name]').first().focus(), 250);
		};

		function setupImageUploader($modal) {
			const $imageUploader = $modal.find('.image-uploader');
			const $imageInput = $modal.find('#ImageFiles');
			const $previewContainer = $modal.find('#imagePreviewContainer');

			// Reset
			newImageFiles = [];
			deletedImageUrls = [];

			// Xử lý xóa ảnh cũ (đã có sẵn trên server)
			$previewContainer.find('.img-preview-wrapper').each(function () {
				const $wrapper = $(this);
				const imgUrl = $wrapper.find('img').attr('src');

				$wrapper.find('.remove-img-btn').on('click', function (e) {
					e.preventDefault();
					deletedImageUrls.push(imgUrl);
					$wrapper.remove();
				});
			});

			// Click để chọn file
			$imageUploader.on('click', (e) => {
				if (!$(e.target).is('input[type="file"]')) {
					e.preventDefault();
					$imageInput.trigger('click');
				}
			});

			// Drag & drop
			$imageUploader.on('dragover', (e) => {
				e.preventDefault();
				$(e.currentTarget).css('background-color', '#e9ecef');
			});

			$imageUploader.on('dragleave drop', (e) => {
				e.preventDefault();
				$(e.currentTarget).css('background-color', 'transparent');
			});

			$imageUploader.on('drop', (e) => {
				handleNewFiles(e.originalEvent.dataTransfer.files, $previewContainer);
			});

			// Chọn file từ input
			$imageInput.on('change', (e) => {
				handleNewFiles(e.target.files, $previewContainer);
			});
		}

		function handleNewFiles(files, $previewContainer) {
			if (!files || files.length === 0) return;

			Array.from(files).forEach(file => {
				if (!file.type.startsWith('image/')) return;

				newImageFiles.push(file);

				// Hiển thị preview
				const reader = new FileReader();
				reader.onload = (e) => {
					const $wrapper = $('<div>').addClass('img-preview-wrapper');
					const $img = $('<img>').attr('src', e.target.result).addClass('img-preview');
					const $removeBtn = $('<button>').html('&times;').addClass('remove-img-btn').attr('type', 'button');

					$removeBtn.on('click', function (e) {
						e.preventDefault();
						// Xóa khỏi mảng
						const index = newImageFiles.indexOf(file);
						if (index > -1) newImageFiles.splice(index, 1);
						$wrapper.remove();
					});

					$wrapper.append($img, $removeBtn);
					$previewContainer.append($wrapper);
				};
				reader.readAsDataURL(file);
			});

			// Clear input để có thể chọn lại cùng 1 file
			$('#ImageFiles').val('');
		}

		this.save = function () {
			if (!_$form.valid()) {
				console.warn("Form không hợp lệ!");
				return;
			}

			_modalManager.setBusy(true);

			const formData = new FormData();

			// Thêm các field thông thường (trừ ImageFiles)
			_$form.serializeArray().forEach(item => {
				if (item.name !== 'ImageFiles') {
					formData.append(item.name, item.value);
				}
			});

			// Thêm ảnh mới (chỉ 1 lần)
			newImageFiles.forEach(file => {
				formData.append('Images', file);
			});

			// Thêm danh sách ảnh bị xóa
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
					_modalManager.setBusy(false);
					abp.notify.info(l('Cập nhật thành công'));
					_modalManager.close();
					setTimeout(() => window.location.href = abp.appPath + 'Products', 500);
				},
				error: function (xhr) {
					_modalManager.setBusy(false);
					const msg = xhr.responseJSON?.error?.message || "Có lỗi xảy ra khi cập nhật sản phẩm.";
					abp.message.error(msg);
				}
			});
		};
	};
})(jQuery);