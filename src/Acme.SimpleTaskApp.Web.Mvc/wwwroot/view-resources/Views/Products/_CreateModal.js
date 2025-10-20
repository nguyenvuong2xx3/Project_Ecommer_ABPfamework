(function ($) {
	// Product Create Modal (ABP ModalManager compatible)
	app.modals.ProductCreateModal = function () {
		var _modalManager;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');

		// Mảng lưu trữ các file ảnh chung
		let generalImageFiles = new DataTransfer();
		let isImageUploaderInitialized = false; // Flag để tránh khởi tạo nhiều lần

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=CreateProductForm]');

			// jQuery Validate config
			if ($.fn.validate) {
				_$form.validate({
					validClass: 'valid',
					errorClass: 'invalid-feedback',
					highlight: function (element) {
						$(element).addClass('is-invalid').removeClass('is-valid');
					},
					unhighlight: function (element) {
						$(element).addClass('is-valid').removeClass('is-invalid');
					},
					rules: {
						Name: {
							required: true,
							minlength: 5,
							maxlength: 256
						},
						CategoryId: {
							required: true
						},
						Screen: {
							maxlength: 1000
						},
						Processor: {
							maxlength: 1000
						},
						CameraSystem: {
							maxlength: 1000
						},
						Battery: {
							maxlength: 1000
						},
						Description: {
							maxlength: 500
						}
					},
					messages: {
						Name: {
							required: 'Tên sản phẩm không được để trống',
							minlength: 'Tên sản phẩm phải có ít nhất 5 ký tự',
							maxlength: 'Tên sản phẩm không được vượt quá 256 ký tự'
						},
						CategoryId: {
							required: 'Vui lòng chọn danh mục'
						},
						Screen: {
							maxlength: 'Thông tin màn hình quá dài'
						},
						Processor: {
							maxlength: 'Thông tin bộ xử lý quá dài'
						},
						CameraSystem: {
							maxlength: 'Thông tin camera quá dài'
						},
						Battery: {
							maxlength: 'Thông tin pin quá dài'
						},
						Description: {
							maxlength: 'Mô tả không được vượt quá 500 ký tự'
						}
					},
					errorPlacement: function (error, element) {
						if (element.closest('.input-group').length) {
							error.addClass('text-danger');
							error.insertAfter(element.closest('.input-group'));
						} else if (element.closest('.form-check').length) {
							error.addClass('text-danger');
							error.appendTo(element.closest('.form-check'));
						} else {
							error.addClass('text-danger');
							error.insertAfter(element);
						}
					},
					success: function (label, element) {
						$(element).next('.invalid-feedback').remove();
					}
				});
			}

			// Khởi tạo upload hình ảnh
			if (!isImageUploaderInitialized) {
				setupGeneralImageUploader($modal);
				isImageUploaderInitialized = true;
			}

			// Focus trường đầu tiên
			setTimeout(function () {
				_$form.find('input[name=Name]').first().trigger('focus');
			}, 250);
		};

		// --- LOGIC XỬ LÝ UPLOAD HÌNH ẢNH CHUNG ---
		function setupGeneralImageUploader($modal) {
			const imageUploader = $modal.find('.image-uploader');
			const imageInput = $modal.find('#ImageFiles');
			const previewContainer = $modal.find('#imagePreviewContainer');

			if (!imageUploader.length || !imageInput.length) {
				console.error('Image uploader elements not found');
				return;
			}

			// Reset generalImageFiles
			generalImageFiles = new DataTransfer();

			// Xóa tất cả event handlers cũ trước khi gán mới (tránh duplicate)
			imageUploader.off('click');
			imageUploader.off('dragover');
			imageUploader.off('dragleave');
			imageUploader.off('drop');
			imageInput.off('change');

			// Click handler - chỉ trigger khi click vào uploader, không phải input
			imageUploader.on('click', function (e) {
				// Kiểm tra xem click có phải từ input không
				if ($(e.target).is('input[type="file"]')) {
					return; // Không làm gì nếu click vào chính input
				}
				e.preventDefault();
				e.stopPropagation();
				imageInput.trigger('click'); // Dùng trigger thay vì click() để tránh đệ quy
			});

			imageUploader.on('dragover', function (e) {
				e.preventDefault();
				e.stopPropagation();
				$(this).css('background-color', '#e9ecef');
			});

			imageUploader.on('dragleave', function (e) {
				e.preventDefault();
				e.stopPropagation();
				$(this).css('background-color', 'transparent');
			});

			imageUploader.on('drop', function (e) {
				e.preventDefault();
				e.stopPropagation();
				$(this).css('background-color', 'transparent');
				const files = e.originalEvent.dataTransfer.files;
				handleFiles(files);
			});

			imageInput.on('change', function (e) {
				const files = e.target.files;
				if (files && files.length > 0) {
					handleFiles(files);
				}
			});

			function handleFiles(files) {
				if (!files || files.length === 0) return;

				for (let i = 0; i < files.length; i++) {
					const file = files[i];
					if (!file.type.startsWith('image/')) {
						console.warn('Skipping non-image file:', file.name);
						continue;
					}

					generalImageFiles.items.add(file);

					const reader = new FileReader();
					reader.onload = function (e) {
						const preview = createPreviewElement(e.target.result, file);
						previewContainer.append(preview);
					};
					reader.readAsDataURL(file);
				}

				// Cập nhật file list cho input gốc
				imageInput[0].files = generalImageFiles.files;
			}

			function createPreviewElement(src, file) {
				const wrapper = $('<div>').addClass('img-preview-wrapper');
				const img = $('<img>').attr('src', src).addClass('img-preview');
				const removeBtn = $('<button>').html('&times;').addClass('remove-img-btn').attr('type', 'button');

				removeBtn.on('click', function (e) {
					e.preventDefault();
					e.stopPropagation();

					// Xóa file khỏi DataTransfer
					const newFiles = new DataTransfer();
					for (let i = 0; i < generalImageFiles.files.length; i++) {
						if (generalImageFiles.files[i] !== file) {
							newFiles.items.add(generalImageFiles.files[i]);
						}
					}
					generalImageFiles = newFiles;
					imageInput[0].files = generalImageFiles.files;
					wrapper.remove();
				});

				wrapper.append(img, removeBtn);
				return wrapper;
			}
		}

		// ABP sẽ tự gọi hàm save này khi click button .save-button trong modal
		this.save = function () {
			console.log('[ProductCreateModal] save() invoked');

			if (!_$form) {
				console.error('[ProductCreateModal] Form not initialized');
				return;
			}

			if ($.fn.validate && !_$form.valid()) {
				console.log('[ProductCreateModal] form invalid');
				return;
			}

			_modalManager.setBusy(true);

			// Tạo FormData
			var formData = new FormData(_$form[0]);

			// Thêm các file ảnh vào FormData
			Array.from(generalImageFiles.files).forEach(function (file, index) {
				formData.append(`ProductImages[${index}].ImageFiles`, file);
			});

			// Gửi request đến Controller
			$.ajax({
				url: abp.appPath + 'Products/CreateProduct',
				type: 'POST',
				processData: false,
				contentType: false,
				data: formData,
				success: function (response) {
					_modalManager.setBusy(false);
					abp.notify.info(l('Lưu thành công'));
					_modalManager.close();

					// Reload trang hoặc refresh datatable
					setTimeout(function () {
						window.location.href = abp.appPath + 'Products';
					}, 500);
				},
				error: function (xhr, textStatus, errorThrown) {
					_modalManager.setBusy(false);

					var errorMessage;
					if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
						errorMessage = xhr.responseJSON.errors.join("<br/>");
					} else if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
						errorMessage = xhr.responseJSON.error.message;
					} else {
						errorMessage = "Có lỗi xảy ra khi tạo mới sản phẩm (Có thể do upload ảnh không đúng định dạng .jpg, .jpeg, .png, .gif)";
					}

					// Hiển thị error trong modal
					var $errorContainer = _modalManager.getModal().find('#error-message');
					if ($errorContainer.length) {
						$errorContainer.html(errorMessage).show();
					} else {
						abp.message.error(errorMessage);
					}
				}
			});
		};
	};
})(jQuery);