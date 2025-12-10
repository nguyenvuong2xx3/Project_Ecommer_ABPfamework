(function ($) {
	var _productVariantService = abp.services.app.productVariant,
		l = abp.localization.getSource('SimpleTaskApp'),
		_$modal = $('#ProductVariantCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#ProductVariantsTable');

	var _permissions = {
		view: abp.auth.hasPermission('Pages.ProductVariants.View'),
		create: abp.auth.hasPermission('Pages.ProductVariants.Create'),
		edit: abp.auth.hasPermission('Pages.ProductVariants.Edit'),
		delete: abp.auth.hasPermission('Pages.ProductVariants.Delete'),
	};

	var _createModal = new app.ModalManager({
		viewUrl: abp.appPath + 'ProductVariants/CreateModal',
		scriptUrl: abp.appPath + 'view-resources/Views/ProductVariants/_CreateModal.js',
		modalClass: 'ProductVariantCreateModal',
		modalSize: 'modal-lg'
	});

	$('#CreateNewButton').click(function () {
		_createModal.open();
	});

	var _editModal = new app.ModalManager({
		viewUrl: abp.appPath + 'ProductVariants/EditModal',
		scriptUrl: abp.appPath + 'view-resources/Views/ProductVariants/_EditModal.js',
		modalClass: 'ProductVariantEditModal',
		modalSize: 'modal-lg'
	});

	$(document).on('click', '.edit-productvariant', function () {
		var productvariantId = $(this).attr("data-productvariant-id");
		_editModal.open({ id: productvariantId });
	});

	var _detailModal = new app.ModalManager({
		viewUrl: abp.appPath + 'ProductVariants/DetailModal',
		scriptUrl: abp.appPath + 'view-resources/Views/ProductVariants/_DetailModal.js',
		modalClass: 'DetailProductVariantModal',
		modalSize: 'modal-lg'
	});

	$(document).on('click', '.detail-productvariant', function () {
		var productvariantId = $(this).attr("data-productvariant-id");
		_detailModal.open({ id: productvariantId });
	});

	// Date Range Picker - Tương tự như Product
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

	$('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
		$(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
		_selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
		_selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
	});

	$('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
		$(this).val('');
		_selectedDateRange.StartTime = null;
		_selectedDateRange.EndTime = null;
	});

	// Hiển thị/Ẩn bộ lọc nâng cao
	$('#ShowAdvancedFiltersSpan').click(function () {
		$('#ShowAdvancedFiltersSpan').hide();
		$('#HideAdvancedFiltersSpan').show();
		$('#AdvacedAuditFiltersArea').slideDown();
	});

	$('#HideAdvancedFiltersSpan').click(function () {
		$('#HideAdvancedFiltersSpan').hide();
		$('#ShowAdvancedFiltersSpan').show();
		$('#AdvacedAuditFiltersArea').slideUp();
	});

	// Reset bộ lọc
	$('#ResetFilters').click(function () {
		$('#ProductVariantSearchForm')[0].reset();
		_selectedDateRange.StartTime = null;
		_selectedDateRange.EndTime = null;
		$('#StartEndRange').val('');
		_$productvariantsTable.ajax.reload();
	});

	// DataTable
	var _$productvariantsTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		processing: true,
		listAction: {
			ajaxFunction: _productVariantService.getAllProductVariant,
			inputFilter: function () {
				var formData = $('#ProductVariantSearchForm').serializeFormToObject(true);

				// Đảm bảo tên parameters khớp với Input DTO
				if (_selectedDateRange.StartTime) {
					formData.startTime = moment(_selectedDateRange.StartTime).format('YYYY-MM-DDTHH:mm:ss');
				}
				if (_selectedDateRange.EndTime) {
					formData.endTime = moment(_selectedDateRange.EndTime).format('YYYY-MM-DDTHH:mm:ss');
				}

				// Chuyển đổi kiểu dữ liệu cho số
				if (formData.minPrice) formData.minPrice = parseFloat(formData.minPrice);
				if (formData.maxPrice) formData.maxPrice = parseFloat(formData.maxPrice);
				if (formData.stockQuantity) formData.stockQuantity = parseInt(formData.stockQuantity);

				return formData;
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$productvariantsTable.draw(false),
			}
		],
		responsive: {
			details: {
				type: 'column'
			}
		},
		columnDefs: [
			{
				targets: 0,
				data: 'productName',
				sortable: false
			},
			{
				targets: 1,
				data: 'imageUrl',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<img src="${data}" alt="Ảnh sản phẩm" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
					}
					return '<span class="text-muted">Không có ảnh</span>';
				}
			},
			{
				targets: 2,
				data: 'ram',
				sortable: false
			},
			{
				targets: 3,
				data: 'storage',
				sortable: false
			},
			{
				targets: 4,
				data: 'color',
				sortable: false
			},
			{
				targets: 5,
				data: 'stockQuantity',
				sortable: false,
				render: function (data, type, row) {
					return `<span class="badge ${data > 0 ? 'bg-success' : 'bg-danger'}">${data}</span>`;
				}
			},
			{
				targets: 6,
				data: 'price',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data);
					}
					return '<span class="text-muted">0</span>';
				}
			},
			{
				targets: 7,
				data: 'creationTime',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<span class="badge bg-info">${moment(data).format('DD-MM-YYYY HH:mm')}</span>`;
					}
					return '<span class="text-muted">N/A</span>';
				}
			},
			{
				targets: 8,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					let buttons = [];

					if (_permissions.edit) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-secondary edit-productvariant me-2" data-productvariant-id="${row.id}" data-toggle="modal" data-target="#ProductVariantEditModal">` +
							`   <i class="fas fa-pencil-alt"></i> ${l('Edit')}` +
							'</button>'
						);
					}

					if (_permissions.delete) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-danger delete-productvariant me-2" data-productvariant-id="${row.id}" data-productvariant-name="${row.productName}">` +
							`   <i class="fas fa-trash"></i> ${l('Delete')}` +
							'</button>'
						);
					}

					if (_permissions.view) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-info detail-productvariant" data-productvariant-id="${row.id}" data-toggle="modal">` +
							`   <i class="fas fa-eye"></i> ${l('Details')}` +
							'</button>'
						);
					}

					return `<div class="d-flex justify-content-center">${buttons.join('')}</div>`;
				}
			}
		],
		language: {
			emptyTable: "Không có dữ liệu",
			info: "Hiển thị _START_ đến _END_ của _TOTAL_ bản ghi",
			infoEmpty: "Hiển thị 0 đến 0 của 0 bản ghi",
			infoFiltered: "(lọc từ _MAX_ tổng số bản ghi)",
			lengthMenu: "Hiển thị _MENU_ bản ghi",
			loadingRecords: "Đang tải...",
			processing: "Đang xử lý...",
			search: "Tìm kiếm:",
			zeroRecords: "Không tìm thấy kết quả phù hợp",
			paginate: {
				first: "Đầu",
				last: "Cuối",
				next: "Tiếp",
				previous: "Trước"
			}
		}
	});

	// Refresh table
	$(document).on('click', '.buttons-refresh', function () {
		_$productvariantsTable.ajax.reload();
	});

	// Tìm kiếm
	$('.btn-search').on('click', (e) => {
		updateDateRangeFromPicker();
		_$productvariantsTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			updateDateRangeFromPicker();
			_$productvariantsTable.ajax.reload();
			return false;
		}
	});

	function updateDateRangeFromPicker() {
		var currentValue = $('#StartEndRange').val();
		if (currentValue) {
			var dates = currentValue.split(' - ');
			if (dates.length === 2) {
				var startDate = moment(dates[0], 'DD/MM/YYYY').startOf('day');
				var endDate = moment(dates[1], 'DD/MM/YYYY').endOf('day');

				_selectedDateRange.StartTime = startDate.format('YYYY-MM-DDTHH:mm:ss');
				_selectedDateRange.EndTime = endDate.format('YYYY-MM-DDTHH:mm:ss');
			}
		}
	}

	// Xóa product variant
	$(document).on('click', '.delete-productvariant', function () {
		var productvariantId = $(this).attr("data-productvariant-id");
		var productvariantName = $(this).attr('data-productvariant-name');
		deleteProductVariant(productvariantId, productvariantName);
	});

	function deleteProductVariant(productvariantId, productvariantName) {
		abp.message.confirm(
			abp.utils.formatString(
				l('Bạn có muốn xóa biến thể sản phẩm "{0}"?'),
				productvariantName),
			null,
			(isConfirmed) => {
				if (isConfirmed) {
					$.ajax({
						url: '/ProductVariants/Delete',
						type: 'POST',
						data: { id: productvariantId }
					}).done(() => {
						abp.notify.info(l('Xoá thành công'));
						_$productvariantsTable.ajax.reload();
					}).fail((xhr) => {
						let errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra khi xoá biến thể sản phẩm';
						abp.notify.error(errorMsg);
					});
				}
			}
		);
	}

	// Event khi product variant được edit
	abp.event.on('productvariant.edited', (data) => {
		_$productvariantsTable.ajax.reload();
	});
	abp.event.on('app.productVariantCreatedOrUpdated', function () {
		_$productvariantsTable.ajax.reload();
	});
})(jQuery);