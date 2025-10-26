(function () {
	var _orderService = abp.services.app.orders,
		l = abp.localization.getSource('SimpleTaskApp')


	var _selectedDateRange = {
		StartTime: null,
		EndTime: null
	};

	$('#StartEndRange').daterangepicker({
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

	// Sự kiện apply - QUAN TRỌNG: phải dùng 'apply.daterangepicker'
	$('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
		$(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));

		// CẬP NHẬT _selectedDateRange - SỬA ĐỊNH DẠNG
		_selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
		_selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
	});

	$('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
		$(this).val('');
		_selectedDateRange.StartTime = null;
		_selectedDateRange.EndTime = null;
	});

	$('#ResetFilters').click(function () {
		$('#ProductSearchForm')[0].reset();
		_selectedDateRange.StartTime = null;
		_selectedDateRange.EndTime = null;
		$('#StartEndRange').val('');
		_$productsTable.ajax.reload();
	});

})();
