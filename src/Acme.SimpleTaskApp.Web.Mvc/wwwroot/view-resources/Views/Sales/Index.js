(function ($) {
	var _saleService = abp.services.app.sale;
	var _$salesTable = $('#SalesTable');
	var _$searchForm = $('#SaleSearchForm');
	console.log('Sales Index script loaded.');
	// Modal managers
	var _createModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Sales/CreateModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Sales/_CreateModal.js',
		modalClass: 'SaleCreateModal',
		modalSize: 'modal-xl'
	});

	var _editModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Sales/EditModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Sales/_EditModal.js',
		modalClass: 'SaleEditModal',
		modalSize: 'modal-xl'
	});

	var _detailModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Sales/DetailModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Sales/_DetailModal.js',
		modalClass: 'SaleDetailModal',
		modalSize: 'modal-lg'
	});

	// Initialize DataTable
	var _dataTable = _$salesTable.DataTable({
		paging: true,
		serverSide: true,
		processing: true,
		listAction: {
			ajaxFunction: _saleService.getAllSales,
			inputFilter: function () {
				return createRequestParams();
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _dataTable.ajax.reload(),
			}
		],
		columnDefs: [
			{
				targets: 0,
				data: null,
				orderable: false,
				autoWidth: false,
				defaultContent: '',
				render: function (data, type, row, meta) {
					return meta.row + meta.settings._iDisplayStart + 1;
				}
			},
			{
				targets: 1,
				data: 'name',
				orderable: true,
			},
			{
				targets: 2,
				data: 'discountTypeName',
				orderable: false,
				render: function (data, type, row) {
					if (row.discountType === 0) {
						return '<span class="badge badge-success"><i class="fas fa-bolt"></i> ' + data + '</span>';
					} else {
						return '<span class="badge badge-primary"><i class="fas fa-ticket-alt"></i> ' + data + '</span>';
					}
				}
			},
			{
				targets: 3,
				data: 'voucherCode',
				orderable: false,
				render: function (data, type, row) {
					if (data) {
						return '<code class="bg-light p-1">' + data + '</code>';
					}
					return '<span class="text-muted">-</span>';
				}
			},
			{
				targets: 4,
				data: 'discountPercentage',
				orderable: true,
				render: function (data, type, row) {
					return '<span class="badge badge-danger font-weight-bold" style="font-size: 1em">' + data + '%</span>';
				}
			},
			{
				targets: 5,
				data: 'applyToName',
				orderable: false,
				render: function (data, type, row) {
					var badgeClass = 'badge-secondary';
					var icon = 'fa-globe';

					switch (row.applyTo) {
						case 0: // EntireOrder
							badgeClass = 'badge-info';
							icon = 'fa-shopping-cart';
							break;
						case 1: // Categories
							badgeClass = 'badge-warning';
							icon = 'fa-folder';
							break;
						case 2: // Products
							badgeClass = 'badge-success';
							icon = 'fa-box';
							break;
						case 3: // Variants
							badgeClass = 'badge-primary';
							icon = 'fa-cubes';
							break;
					}

					return '<span class="badge ' + badgeClass + '"><i class="fas ' + icon + '"></i> ' + data + '</span>';
				}
			},
			{
				targets: 6,
				data: null,
				orderable: false,
				render: function (data, type, row) {
					var start = moment(row.startDate).format('DD/MM/YYYY HH:mm');
					var end = moment(row.endDate).format('DD/MM/YYYY HH:mm');
					return '<small><i class="fas fa-calendar-alt text-success"></i> ' + start + '<br>' +
						'<i class="fas fa-calendar-check text-danger"></i> ' + end + '</small>';
				}
			},
			{
				targets: 7,
				data: null,
				orderable: false,
				render: function (data, type, row) {
					var html = '<small><strong>' + row.usedCount + '</strong>';

					if (row.usageLimit) {
						html += ' / ' + row.usageLimit;
						if (row.remainingUsage !== null) {
							html += '<br><span class="text-muted">Còn: ' + row.remainingUsage + '</span>';
						}
					} else {
						html += '<br><span class="text-muted">Không giới hạn</span>';
					}

					html += '</small>';
					return html;
				}
			},
			{
				targets: 8,
				data: 'status',
				orderable: false,
				render: function (data, type, row) {
					var badgeClass = 'badge-secondary';
					var icon = 'fa-question-circle';

					switch (data) {
						case 'Đang hoạt động':
							badgeClass = 'badge-success';
							icon = 'fa-check-circle';
							break;
						case 'Sắp diễn ra':
							badgeClass = 'badge-info';
							icon = 'fa-clock';
							break;
						case 'Đã kết thúc':
							badgeClass = 'badge-danger';
							icon = 'fa-times-circle';
							break;
						case 'Đã hết lượt':
							badgeClass = 'badge-warning';
							icon = 'fa-exclamation-circle';
							break;
						case 'Đã tạm dừng':
							badgeClass = 'badge-secondary';
							icon = 'fa-pause-circle';
							break;
					}

					var html = '<span class="badge ' + badgeClass + '"><i class="fas ' + icon + '"></i> ' + data + '</span>';

					if (row.isActive) {
						html += '<br><small class="text-success"><i class="fas fa-toggle-on"></i> Kích hoạt</small>';
					} else {
						html += '<br><small class="text-muted"><i class="fas fa-toggle-off"></i> Tạm dừng</small>';
					}

					return html;
				}
			},
			{
				targets: 9,
				data: null,
				orderable: false,
				autoWidth: false,
				render: function (data, type, row) {
					var html = '<div class="btn-group btn-group-sm" role="group">';

					// Detail button
					html += '<button type="button" class="btn btn-info btn-sm detail-sale" data-sale-id="' + row.id + '" title="Chi tiết">' +
						'<i class="fas fa-info-circle"></i></button>';

					// Edit button
					html += '<button type="button" class="btn btn-warning btn-sm edit-sale" data-sale-id="' + row.id + '" title="Chỉnh sửa">' +
						'<i class="fas fa-edit"></i></button>';

					// Toggle Active button
					if (row.isActive) {
						html += '<button type="button" class="btn btn-secondary btn-sm toggle-active-sale" data-sale-id="' + row.id + '" title="Tạm dừng">' +
							'<i class="fas fa-pause"></i></button>';
					} else {
						html += '<button type="button" class="btn btn-success btn-sm toggle-active-sale" data-sale-id="' + row.id + '" title="Kích hoạt">' +
							'<i class="fas fa-play"></i></button>';
					}

					// Delete button (only if not used)
					if (row.usedCount === 0) {
						html += '<button type="button" class="btn btn-danger btn-sm delete-sale" data-sale-id="' + row.id + '" data-sale-name="' + row.name + '" title="Xóa">' +
							'<i class="fas fa-trash"></i></button>';
					}

					html += '</div>';
					return html;
				}
			}
		],
		language: {
			emptyTable: "Không có dữ liệu",
			info: "Hiển thị _START_ đến _END_ trong _TOTAL_ chương trình giảm giá",
			infoEmpty: "Hiển thị 0 đến 0 của 0 chương trình giảm giá",
			infoFiltered: "(lọc từ _MAX_ tổng số chương trình giảm giá)",
			lengthMenu: "Hiển thị _MENU_ chương trình giảm giá",
			loadingRecords: "Đang tải...",
			processing: "Đang xử lý...",
			search: "Tìm kiếm:",
			zeroRecords: "Không tìm thấy sale phù hợp",
			paginate: {
				first: "Đầu",
				last: "Cuối",
				next: "Tiếp",
				previous: "Trước"
			}
		}
	});

	// Create request params from form
	function createRequestParams() {
		var params = _$searchForm.serializeFormToObject();

		// Convert boolean strings
		if (params.IsActive === 'true') params.IsActive = true;
		else if (params.IsActive === 'false') params.IsActive = false;
		else params.IsActive = null;

		// Convert enum values
		if (params.DiscountType !== '') {
			params.DiscountType = parseInt(params.DiscountType);
		} else {
			params.DiscountType = null;
		}

		if (params.ApplyTo !== '') {
			params.ApplyTo = parseInt(params.ApplyTo);
		} else {
			params.ApplyTo = null;
		}

		// Convert number values
		if (params.MinDiscountPercentage) {
			params.MinDiscountPercentage = parseFloat(params.MinDiscountPercentage);
		}

		if (params.MaxDiscountPercentage) {
			params.MaxDiscountPercentage = parseFloat(params.MaxDiscountPercentage);
		}

		// Handle checkbox
		params.HasUsageLimitReached = $('#HasUsageLimitReached').is(':checked') || null;

		return params;
	}

	// Show/Hide advanced filters
	$('#ShowAdvancedFiltersSpan').click(function () {
		$('#ShowAdvancedFiltersSpan').hide();
		$('#HideAdvancedFiltersSpan').show();
		$('#AdvancedFiltersArea').slideDown();
	});

	$('#HideAdvancedFiltersSpan').click(function () {
		$('#HideAdvancedFiltersSpan').hide();
		$('#ShowAdvancedFiltersSpan').show();
		$('#AdvancedFiltersArea').slideUp();
	});

	// Search button click
	$('.btn-search').on('click', function (e) {
		e.preventDefault();
		_dataTable.ajax.reload();
	});

	// Search on enter
	$('.txt-search').on('keypress', function (e) {
		if (e.which === 13) {
			e.preventDefault();
			_dataTable.ajax.reload();
		}
	});

	// Reset filters
	$('#ResetFilters').click(function () {
		_$searchForm[0].reset();
		$('#HasUsageLimitReached').prop('checked', false);
		_dataTable.ajax.reload();
	});

	// Create new sale
	$('#CreateNewSaleButton').click(function () {
		_createModal.open();
	});

	// Detail sale
	$(document).on('click', '.detail-sale', function () {
		var saleId = $(this).attr('data-sale-id');
		_detailModal.open({ id: saleId });
	});

	// Edit sale
	$(document).on('click', '.edit-sale', function () {
		var saleId = $(this).attr('data-sale-id');
		_editModal.open({ id: saleId });
	});

	// Toggle active status
	$(document).on('click', '.toggle-active-sale', function () {
		var saleId = $(this).attr('data-sale-id');

		abp.message.confirm(
			'Bạn có chắc chắn muốn thay đổi trạng thái kích hoạt của chương trình giảm giá này?',
			'Xác nhận',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy();

					_saleService.activeOrInActive(saleId)
						.done(function () {
							abp.notify.success('Cập nhật chương trình giảm giá thành công!');
							_dataTable.ajax.reload();
						})
						.fail(function (error) {
							abp.notify.error(error.message || 'Có lỗi xảy ra khi cập nhật chương trình giảm giá ');
						})
						.always(function () {
							abp.ui.clearBusy();
						});
				}
			}
		);
	});

	// Delete sale
	$(document).on('click', '.delete-sale', function () {
		var saleId = $(this).attr('data-sale-id');
		var saleName = $(this).attr('data-sale-name');

		deleteSale(saleId, saleName);
	});

	function deleteSale(saleId, saleName) {
		abp.message.confirm(
			'Bạn có chắc chắn muốn xóa chương trình giảm giá "' + saleName,
			'Xác nhận xóa',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy();

					_saleService.deleteSale(saleId)
						.done(function () {
							abp.notify.success('Xóa chương trình giảm giá thành công!');
							_dataTable.ajax.reload();
						})
						.fail(function (error) {
							abp.notify.error(error.message || 'Có lỗi xảy ra khi xóa chương trình giảm giá ');
						})
						.always(function () {
							abp.ui.clearBusy();
						});
				}
			}
		);
	}

	// Event handlers for modal events
	abp.event.on('sale.created', function () {
		_dataTable.ajax.reload();
	});

	abp.event.on('sale.updated', function () {
		_dataTable.ajax.reload();
	});

})(jQuery);
