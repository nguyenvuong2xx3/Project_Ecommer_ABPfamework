(function () {
	app.modals.ProductExportModal = function () {
		var _modalManager;
		var _productImportExportService = abp.services.app.productImportExport;
		var _$form = null;
		var _filterData;
		var _$dateRangePicker = null;
		var _dateRangePickerInitialized = false;


		var _selectedDateRange = {
			StartTime: null,
			EndTime: null
		};


		this.init = function (modalManager) {
			_modalManager = modalManager;
			_filterData = _modalManager.getArgs().filter;

			_$form = _modalManager.getModal().find('form[name=ExportForm]');

			_$dateRangePicker = $('#StartEndRangeExport');

			_$form.validate({
				validClass: "valid",
				errorClass: "invalid-feedback",
				highlight: function (element, errorClass, validClass) {
					$(element).addClass('is-invalid').removeClass('is-valid');
				},
				unhighlight: function (element, errorClass, validClass) {
					$(element).addClass('is-valid').removeClass('is-invalid');
				},
				rules: {
					SelectFileFormat: {
						required: true,
					},
				},
				messages: {
					SelectFileFormat: {
						required: 'Định dạng file phải được chọn',
					},
				},
			});
			initDateRangePicker()

		};

		function initDateRangePicker() {
			if (_$dateRangePicker.length && !_$dateRangePicker.data('daterangepicker')) {
				_$dateRangePicker.daterangepicker({
					autoApply: false,
					autoUpdateInput: false,
					opens: 'left',
					locale: {
						format: 'DD/MM/YYYY',
						applyLabel: 'Áp dụng',
						cancelLabel: 'Hủy bỏ',
						fromLabel: 'Từ',
						toLabel: 'Đến',
						customRangeLabel: 'Phạm vi tùy chỉnh',
						firstDay: 1
					}
				});

				// Sự kiện apply
				_$dateRangePicker.on('apply.daterangepicker', function (ev, picker) {
					$(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
					_selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
					_selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
				});

				_$dateRangePicker.on('cancel.daterangepicker', function (ev, picker) {
					$(this).val('');
					_selectedDateRange.StartTime = null;
					_selectedDateRange.EndTime = null;
				});
			}
		}

		this.save = function () {
			if (!_$form.valid()) {
				return;
			}

			var filter = function () {
				var datafilter = {};
				debugger
				//var categoryVal = $("#CategoryFilter").val();
				var categoryVal = _$form.find("#CategoryFilter").val();
				datafilter.categoryId = categoryVal ? parseInt(categoryVal) : null;

				// Sử dụng _selectedDateRange đã được cập nhật từ date range picker
				datafilter.creationTimeStart = _selectedDateRange.StartTime;
				datafilter.creationTimeEnd = _selectedDateRange.EndTime;

				datafilter.exportFileType = $("#SelectFileFormat").val();

				return datafilter;
			};

			_modalManager.setBusy(true);
			_productImportExportService
				.exportProducts(filter())
				.done(function (result) {
					if (result.isSuccess) {
						// 1. Lấy các thuộc tính từ kết quả trả về
						// Lưu ý: C# "FileContent" khi qua JSON có thể thành "fileContent" (camelCase)
						var b64Data = result.fileContent;
						var contentType = result.contentType;
						var fileName = result.fileName || 'exported_file';

						// 2. Tạo Data URL từ chuỗi Base64
						// Cú pháp: 'data:[<mime_type>][;base64],<data>'
						var dataUrl = 'data:' + contentType + ';base64,' + b64Data;

						// 3. Tạo link và gán Data URL
						var link = document.createElement('a');
						link.href = dataUrl; // Sử dụng Data URL
						link.download = fileName; // Sử dụng tên file từ result
						document.body.appendChild(link);
						link.click();
						document.body.removeChild(link);

						abp.notify.success('Xuất dữ liệu thành công');
					} else {
						abp.notify.error('Xuất dữ liệu thất bại vui lòng thử lại');
					}
					_modalManager.close();
				})
		};
	};
})();