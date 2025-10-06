(function ($) {
	// --- KHAI BÁO BIẾN & CÀI ĐẶT CHUNG ---
	var l = abp.localization.getSource('SimpleTaskApp');
	var _$form = $('#productCreateForm');

	// Mảng lưu trữ các file ảnh chung
	let generalImageFiles = new DataTransfer();
	// Mảng lưu trữ các file ảnh của biến thể (key: rowIndex, value: File[])
	let variantImageFiles = {};

	// --- LOGIC XỬ LÝ UPLOAD HÌNH ẢNH CHUNG ---
	function setupGeneralImageUploader() {
		const imageUploader = $('.image-uploader');
		const imageInput = $('#ImageFiles');
		const previewContainer = $('#imagePreviewContainer');

		if (!imageUploader.length) return;

		//imageUploader.on('click', () => imageInput.click());
		imageUploader.on('click', function (e) {
			e.preventDefault();
			e.stopPropagation();
			imageInput.click();
		});

		imageUploader.on('dragover', (e) => {
			e.preventDefault();
			imageUploader.css('background-color', '#e9ecef');
		});

		imageUploader.on('dragleave', (e) => {
			e.preventDefault();
			imageUploader.css('background-color', 'transparent');
		});

		imageUploader.on('drop', (e) => {
			e.preventDefault();
			imageUploader.css('background-color', 'transparent');
			const files = e.originalEvent.dataTransfer.files;
			handleFiles(files);
		});

		imageInput.on('change', (e) => {
			handleFiles(e.target.files);
			// Reset input để có thể chọn lại file giống nhau sau khi xóa
			$(e.target).val(''); // Khôi phục dòng này để cho phép chọn lại file sau khi đã chọn
		});

		function handleFiles(files) {
			for (const file of files) {
				if (!file.type.startsWith('image/')) continue;
				generalImageFiles.items.add(file);
				const reader = new FileReader();
				reader.onload = (e) => {
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

			removeBtn.on('click', function () {
				// Xóa file khỏi DataTransfer
				const newFiles = new DataTransfer();
				for (let i = 0; i < generalImageFiles.items.length; i++) {
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

	// --- LOGIC XỬ LÝ TẠO BIẾN THỂ ---
	function setupVariantGenerator() {
		// Khởi tạo Tagify cho các input thuộc tính
		const tagifyInputs = {
			color: new Tagify($('input[name=option_color]')[0]),
			storage: new Tagify($('input[name=option_storage]')[0]),
			ram: new Tagify($('input[name=option_ram]')[0])
		};
		const variantsTableBody = $('#variantsTable tbody');

		$('#generateVariantsBtn').on('click', function () {
			const options = {};
			options.color = tagifyInputs.color.value.map(tag => tag.value);
			options.storage = tagifyInputs.storage.value.map(tag => tag.value);
			options.ram = tagifyInputs.ram.value.map(tag => tag.value);

			const attributes = Object.keys(options).filter(key => options[key].length > 0);

			if (attributes.length === 0) {
				abp.notify.warn('Vui lòng nhập ít nhất một thuộc tính (màu, dung lượng, hoặc RAM).');
				return;
			}

			// Hàm nhân các mảng thuộc tính để tạo tổ hợp
			const cartesian = (...a) => a.reduce((acc, val) => acc.flatMap(d => val.map(e => [d, e].flat())));

			const variants = cartesian(...attributes.map(attr => options[attr]));

			variantsTableBody.empty(); // Xóa các dòng cũ
			variantImageFiles = {}; // Reset ảnh biến thể

			if (variants.length === 0 || (variants.length === 1 && variants[0].length === 0)) {
				variantsTableBody.html($('#variant-placeholder').clone());
				return;
			}

			let variantIndex = 0;
			variants.forEach(variant => {
				const variantData = {};
				const variantNameParts = [];

				// Đảm bảo variant luôn là mảng để xử lý đồng nhất
				const V = Array.isArray(variant) ? variant : [variant];

				let currentOptionIndex = 0;
				attributes.forEach(attr => {
					const value = V[currentOptionIndex];
					variantData[attr] = value;
					variantNameParts.push(value);
					currentOptionIndex++;
				});

				const variantName = variantNameParts.join(' / ');
				const row = createVariantRow(variantIndex, variantName, variantData);
				variantsTableBody.append(row);
				variantIndex++;
			});
		});

		function createVariantRow(index, name, data) {
			const color = data.color || '';
			const storage = data.storage || '';
			const ram = data.ram || '';

			const row = `
				<tr class="variant-row" data-index="${index}">
					<td>
						<strong>${name}</strong>
						<input type="hidden" name="ProductVariants[${index}].Color" value="${color}" />
						<input type="hidden" name="ProductVariants[${index}].Storage" value="${storage}" />
						<input type="hidden" name="ProductVariants[${index}].Ram" value="${ram}" />
					</td>
					<td>
						<div class="variant-image-container">
							<input type="file" name="ProductVariants[${index}].ImageFiles" class="d-none" accept="image/*" multiple>
							<div class="variant-img-placeholder" style="width: 125px;">
								<i class="fas fa-camera"></i>
								<small class="d-block ml-2">Chọn nhiều ảnh</small>
							</div>
							<div class="variant-images-preview-container"></div>
						</div>
					</td>
					<td>
						<input type="number" name="ProductVariants[${index}].Price" class="form-control" placeholder="Giá" required min="0">
					</td>
					<td>
						<input type="number" name="ProductVariants[${index}].StockQuantity" class="form-control" placeholder="Số lượng" required min="0">
					</td>
					<td><button type="button" class="btn btn-danger btn-sm remove-variant-row"><i class="fas fa-trash"></i></button></td>
				</tr>
			`;
			return row;
		}

		// Sự kiện xóa một dòng biến thể
		variantsTableBody.on('click', '.remove-variant-row', function () {
			const rowIndex = $(this).closest('tr').data('index');
			delete variantImageFiles[rowIndex];
			$(this).closest('tr').remove();
		});

		// Sự kiện chọn ảnh cho biến thể
		variantsTableBody.on('click', '.variant-img-placeholder, .variant-images-preview-container', function () {
			$(this).closest('.variant-image-container').find('input[type=file]').click();
		});

		variantsTableBody.on('change', 'input[type=file]', function (e) {
			const files = e.target.files;
			if (!files || files.length === 0) return;

			const container = $(this).closest('.variant-image-container');
			const previewContainer = container.find('.variant-images-preview-container');
			const rowIndex = $(this).closest('tr').data('index');

			// Khởi tạo mảng cho biến thể nếu chưa có
			if (!variantImageFiles[rowIndex]) {
				variantImageFiles[rowIndex] = [];
			}

			// Thêm các file mới vào mảng
			for (let i = 0; i < files.length; i++) {
				const file = files[i];
				variantImageFiles[rowIndex].push(file);

				const reader = new FileReader();
				reader.onload = (event) => {
					const previewWrapper = $('<div>').addClass('variant-img-preview-wrapper');
					const img = $('<img>').addClass('variant-img-preview').attr('src', event.target.result);
					const removeBtn = $('<button>').html('&times;').addClass('remove-variant-img-btn').attr('type', 'button');

					removeBtn.on('click', function () {
						// Xóa file khỏi mảng
						const fileIndex = variantImageFiles[rowIndex].indexOf(file);
						if (fileIndex > -1) {
							variantImageFiles[rowIndex].splice(fileIndex, 1);
						}
						previewWrapper.remove();

						// Ẩn placeholder nếu không còn ảnh nào
						if (previewContainer.children().length === 0) {
							container.find('.variant-img-placeholder').show();
						}
					});

					previewWrapper.append(img, removeBtn);
					previewContainer.append(previewWrapper);
					container.find('.variant-img-placeholder').hide();
				};
				reader.readAsDataURL(file);
			}

			// Reset input để có thể chọn lại file giống nhau
			$(this).val('');
		});

		// Sự kiện xóa ảnh preview của biến thể
		variantsTableBody.on('click', '.remove-variant-img-btn', function (e) {
			e.stopPropagation();
			$(this).closest('.variant-img-preview-wrapper').remove();
		});
	}

	// --- XỬ LÝ SUBMIT FORM ---
	_$form.on('submit', function (e) {
		e.preventDefault();

		if (!_$form.valid()) {
			// Chuyển sang tab có lỗi đầu tiên
			const firstError = $(this).find('.is-invalid').first();
			if (firstError.length) {
				const tabId = firstError.closest('.tab-pane').attr('id');
				$(`a[href="#${tabId}"]`).tab('show');
			}
			return;
		}

		// Sử dụng FormData để gửi cả file và dữ liệu form
		var formData = new FormData(_$form[0]);
		console.log(formData);

		// Thêm các file ảnh của biến thể vào FormData
		for (const index in variantImageFiles) {
			const files = variantImageFiles[index];
			if (files && files.length > 0) {
				files.forEach((file, fileIndex) => {
					formData.append(`ProductVariants[${index}].ImageFiles`, file);
				});
			}
		}

		// Thêm các file ảnh chung vào FormData
		Array.from(generalImageFiles.files).forEach((file, index) => {
			formData.append(`ProductImages[${index}].ImageFiles`, file);
		});

		for (let pair of formData.entries()) {
			console.log(pair[0] + ': ', pair[1]);
		}

		abp.ui.setBusy(_$form);
		console.log('Submitting form with data:', formData);
		
		$.ajax({
			url: abp.appPath + 'Products/CreateProduct',
			type: 'POST',
			processData: false,
			contentType: false,
			data: formData,
			success: function (response) {
				abp.notify.info(l('Lưu thành công'));
				setTimeout(function () {
					window.location.href = abp.appPath + 'Products';
				}, 1000);
			},
			error: function (xhr, textStatus, errorThrown) {
				var errorMessage;
				if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
					errorMessage = xhr.responseJSON.errors.join("<br/>");
				}
				else {
					errorMessage = "Có lỗi xảy ra khi tạo mới sản phẩm (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
				}
				$("#error-message").html(errorMessage).show();
			}
		});
	});

	// --- KHỞI TẠO KHI TRANG ĐƯỢC TẢI ---
	$(document).ready(function () {
		// Khởi tạo validation
		_$form.validate({
			errorElement: 'span',
			errorPlacement: function (error, element) {
				error.addClass('invalid-feedback');
				element.closest('.form-group, td').append(error);
			},
			highlight: function (element, errorClass, validClass) {
				$(element).addClass('is-invalid');
			},
			unhighlight: function (element, errorClass, validClass) {
				$(element).removeClass('is-invalid');
			}
		});

		// Khởi tạo các thành phần
		setupGeneralImageUploader();
		setupVariantGenerator();
	});

})(jQuery);