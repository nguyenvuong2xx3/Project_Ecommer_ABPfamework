(function ($) {
	app.modals.ProductVariantCreateModal = function () {
		var _modalManager;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');

		let newImageFiles = [];        // Mảng File object

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=CreateProductVariantForm]');

			// Thêm custom validator cho giá có format
			$.validator.addMethod("formattedNumber", function (value, element) {
				// Loại bỏ dấu chấm và kiểm tra xem có phải số hợp lệ không
				const unformatted = value.replace(/\./g, '');
				return this.optional(element) || /^\d+$/.test(unformatted);
			}, "Giá bán phải là số hợp lệ");

			$.validator.addMethod("formattedMin", function (value, element, param) {
				// Loại bỏ dấu chấm và so sánh với giá trị tối thiểu
				const unformatted = parseFloat(value.replace(/\./g, '')) || 0;
				return this.optional(element) || unformatted >= param;
			}, "Giá bán phải lớn hơn hoặc bằng {0}");

			// Validate
			_$form.validate({
				validClass: 'valid',
				errorClass: 'invalid-feedback',
				highlight: (el) => $(el).addClass('is-invalid').removeClass('is-valid'),
				unhighlight: (el) => $(el).addClass('is-valid').removeClass('is-invalid'),
				rules: {
					ProductId: {
						required: true
					},
					Ram: {
						maxlength: 50
					},
					Color: {
						maxlength: 50
					},
					Price: {
						required: true,
						formattedNumber: true,
						formattedMin: 0
					},
					StockQuantity: {
						required: true,
						digits: true,
						min: 0
					}
				},
				messages: {
					ProductId: {
						required: 'Vui lòng chọn sản phẩm'
					},
					Ram: {
						maxlength: 'RAM không được vượt quá 50 ký tự'
					},
					Color: {
						maxlength: 'Màu sắc không được vượt quá 50 ký tự'
					},
					Price: {
						required: 'Giá bán không được để trống',
						formattedNumber: 'Giá bán phải là số hợp lệ',
						formattedMin: 'Giá bán phải lớn hơn hoặc bằng 0'
					},
					StockQuantity: {
						required: 'Số lượng tồn kho không được để trống',
						digits: 'Số lượng phải là số hợp lệ',
						min: 'Số lượng phải lớn hơn hoặc bằng 0'
					}
				},
				errorPlacement: (error, element) => error.addClass('text-danger').insertAfter(element)
			});

			// Setup price formatter
			setupPriceFormatter($modal);

			// Setup uploader
			setupImageUploader($modal);

			// Focus
			setTimeout(() => _$form.find('input[name=SKU]').first().focus(), 250);
		};

		// Format giá với dấu chấm phân cách hàng nghìn
		function formatPrice(value) {
			// Loại bỏ tất cả ký tự không phải số
			const numberValue = value.replace(/[^\d]/g, '');

			if (!numberValue) return '';

			// Thêm dấu chấm phân cách hàng nghìn
			return numberValue.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
		}

		// Loại bỏ format để lấy số thuần
		function unformatPrice(value) {
			return value.replace(/\./g, '');
		}

		// Setup price input formatter
		function setupPriceFormatter($modal) {
			const $priceInput = $modal.find('input[name=Price]');

			// Format khi người dùng nhập
			$priceInput.on('input', function () {
				const cursorPosition = this.selectionStart;
				const oldValue = $(this).val();
				const oldLength = oldValue.length;

				// Format giá trị
				const formattedValue = formatPrice(oldValue);
				$(this).val(formattedValue);

				// Tính toán lại vị trí con trỏ sau khi format
				const newLength = formattedValue.length;
				const lengthDiff = newLength - oldLength;
				const newCursorPosition = cursorPosition + lengthDiff;

				// Đặt lại vị trí con trỏ
				this.setSelectionRange(newCursorPosition, newCursorPosition);

				// Trigger validation sau khi format
				$(this).valid();
			});

			// Format khi focus out
			$priceInput.on('blur', function () {
				const value = $(this).val();
				if (value) {
					$(this).val(formatPrice(value));
				}
				// Validate lại
				$(this).valid();
			});

			// Cho phép paste
			$priceInput.on('paste', function (e) {
				setTimeout(() => {
					const value = $(this).val();
					$(this).val(formatPrice(value));
					$(this).valid();
				}, 0);
			});

			// Ngăn không cho nhập ký tự không phải số (trừ dấu chấm)
			$priceInput.on('keypress', function (e) {
				// Cho phép: backspace, delete, tab, escape, enter
				if ($.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
					// Cho phép: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
					(e.keyCode === 65 && e.ctrlKey === true) ||
					(e.keyCode === 67 && e.ctrlKey === true) ||
					(e.keyCode === 86 && e.ctrlKey === true) ||
					(e.keyCode === 88 && e.ctrlKey === true) ||
					// Cho phép: home, end, left, right
					(e.keyCode >= 35 && e.keyCode <= 39)) {
					return;
				}
				// Chỉ cho phép số (0-9) và dấu chấm
				if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) &&
					(e.keyCode < 96 || e.keyCode > 105) &&
					e.keyCode !== 190 && e.keyCode !== 110) {
					e.preventDefault();
				}
			});
		}

		function setupImageUploader($modal) {
			const $imageUploader = $modal.find('.image-uploader');
			const $imageInput = $modal.find('#ImageFiles');
			const $previewContainer = $modal.find('#imagePreviewContainer');

			// Reset
			newImageFiles = [];

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
					// Convert các trường số
					if (item.name === 'Price') {
						// Loại bỏ dấu chấm và convert sang decimal/float
						const priceValue = unformatPrice(item.value);
						const price = parseFloat(priceValue) || 0;
						formData.append(item.name, price);
						console.log('Price:', item.value, '->', price);
					}
					else if (item.name === 'StockQuantity') {
						// Convert sang integer
						const stock = parseInt(item.value, 10) || 0;
						formData.append(item.name, stock);
					}
					else if (item.name === 'ProductId') {
						// Convert ID sang integer
						const id = parseInt(item.value, 10);
						formData.append(item.name, id);
					}
					else {
						// Các trường khác giữ nguyên
						formData.append(item.name, item.value);
					}
				}
			});

			// Thêm ảnh mới
			newImageFiles.forEach(file => {
				formData.append('ImageFiles', file);
			});

			console.log(">>> Dữ liệu gửi đi:", Array.from(formData.entries()));

			$.ajax({
				url: abp.appPath + 'ProductVariants/Create',
				type: 'POST',
				processData: false,
				contentType: false,
				data: formData,
				success: function (res) {
					_modalManager.setBusy(false);
					abp.notify.info(l('Tạo biến thể thành công'));
					_modalManager.close();
					setTimeout(() => window.location.href = abp.appPath + 'ProductVariants', 500);
				},
				error: function (xhr) {
					_modalManager.setBusy(false);
					const msg = xhr.responseJSON?.error?.message || "Có lỗi xảy ra khi tạo biến thể sản phẩm.";
					abp.message.error(msg);
				}
			});
		};
	};
})(jQuery);