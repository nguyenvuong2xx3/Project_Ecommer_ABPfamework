(function ($) {
	var _bannerService = abp.services.app.banner,
		l = abp.localization.getSource('SimpleTaskApp'),
		_$table = $('#BannersTable');


	var _permissions = {
		create: abp.auth.hasPermission('Pages.Banners.Create'),
		edit: abp.auth.hasPermission('Pages.Banners.Edit.Edit'),
		delete: abp.auth.hasPermission('Pages.Banners.Delete'),
	};

	// Modal Managers
	var _createModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Banners/CreateModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Banners/_CreateModal.js',
		modalClass: 'BannerCreateModal',
		modalSize: 'modal-lg'
	});

	var _editModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Banners/EditModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Banners/_EditModal.js',
		modalClass: 'BannerEditModal',
		modalSize: 'modal-lg'
	});

	var _detailModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Banners/DetailModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Banners/_DetailModal.js',
		modalClass: 'BannerDetailModal',
		modalSize: 'modal-lg'
	});

	// Button Events
	$('#CreateNewBannerButton').click(function () {
		_createModal.open();
	});

	$(document).on('click', '.edit-banner', function () {
		var bannerId = $(this).attr("data-banner-id");
		_editModal.open({ id: bannerId });
	});

	$(document).on('click', '.detail-banner', function () {
		var bannerId = $(this).attr("data-banner-id");
		_detailModal.open({ id: bannerId });
	});

	// Advanced Filters Toggle
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

	// Reset Filters
	$('#ResetFilters').click(function () {
		$('#BannerSearchForm')[0].reset();
		_$bannersTable.ajax.reload();
	});

	// Position mapping
	function getPositionText(position) {
		const positions = {
			0: 'Trên cùng trang chủ',
			1: 'Giữa trang chủ',
			2: 'Cuối trang chủ',
			3: 'Thanh bên',
			4: 'Phần đầu trang',
			5: 'Phần chân trang',
			6: 'Trang chi tiết sản phẩm',
			7: 'Trang danh mục'
		};
		return positions[position] || 'Không xác định';
	}

	// DataTable
	var _$bannersTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		processing: true,
		listAction: {
			ajaxFunction: _bannerService.getAllBanners,
			inputFilter: function () {
				return $('#BannerSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$bannersTable.draw(false),
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
				data: null,
				sortable: false,
				render: function (data, type, row, meta) {
					return meta.row + meta.settings._iDisplayStart + 1;
				}
			},
			{
				targets: 1,
				data: 'imageUrl',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<img src="${data}" alt="Banner" class="img-thumbnail" width="120" height="60" style="object-fit: cover;">`;
					}
					return '<span class="text-muted">Không có ảnh</span>';
				}
			},
			{
				targets: 2,
				data: 'title',
				sortable: false,
				render: function (data) {
					return data || '<span class="text-muted">Chưa có tiêu đề</span>';
				}
			},
			{
				targets: 3,
				data: 'position',
				sortable: false,
				render: function (data) {
					return `<span class="badge bg-info">${getPositionText(data)}</span>`;
				}
			},
			{
				targets: 4,
				data: 'sortOrder',
				sortable: false,
				render: function (data) {
					return `<span class="badge bg-secondary">${data}</span>`;
				}
			},
			{
				targets: 5,
				data: 'isActive',
				sortable: false,
				render: function (data) {
					if (data) {
						return '<span class="badge bg-success"><i class="fas fa-check"></i> Hoạt động</span>';
					}
					return '<span class="badge bg-danger"><i class="fas fa-times"></i> Tạm dừng</span>';
				}
			},
			{
				targets: 6,
				data: 'creationTime',
				sortable: false,
				render: function (data) {
					if (data) {
						return `<span class="badge bg-info">${moment(data).format('DD/MM/YYYY HH:mm')}</span>`;
					}
					return '<span class="text-muted">N/A</span>';
				}
			},
			{
				targets: 7,
				data: null,
				sortable: false,
				autoWidth: false,
				defaultContent: '',
				render: (data, type, row, meta) => {
					var buttons = [];

					if (_permissions.edit) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-secondary edit-banner" data-banner-id="${row.id}" title="Chỉnh sửa">`,
							`    <i class="fas fa-pencil-alt"></i> Sửa`,
							`</button>`
						);
					}
					buttons.push(
						`<button type="button" class="btn btn-sm bg-info detail-banner" data-banner-id="${row.id}" title="Xem chi tiết">`,
						`    <i class="fas fa-eye"></i> Chi tiết`,
						`</button>`
					);
					if (_permissions.delete) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-danger delete-banner" data-banner-id="${row.id}" data-banner-title="${row.title}" title="Xóa">`,
							`    <i class="fas fa-trash"></i> Xóa`,
							`</button>`
						);
					}
					return buttons.join('');
				}
			}
		],
		language: {
			emptyTable: "Không có dữ liệu banner",
			info: "Hiển thị _START_ đến _END_ của _TOTAL_ banner",
			infoEmpty: "Hiển thị 0 đến 0 của 0 banner",
			infoFiltered: "(lọc từ _MAX_ tổng số banner)",
			lengthMenu: "Hiển thị _MENU_ banner",
			loadingRecords: "Đang tải...",
			processing: "Đang xử lý...",
			search: "Tìm kiếm:",
			zeroRecords: "Không tìm thấy banner phù hợp",
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
		_$bannersTable.ajax.reload();
	});

	// Search
	$('.btn-search').on('click', (e) => {
		_$bannersTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$bannersTable.ajax.reload();
			return false;
		}
	});

	// Delete Banner
	$(document).on('click', '.delete-banner', function () {
		var bannerId = $(this).attr("data-banner-id");
		var bannerTitle = $(this).attr('data-banner-title');
		deleteBanner(bannerId, bannerTitle);
	});

	function deleteBanner(bannerId, bannerTitle) {
		abp.message.confirm(
			`Bạn có chắc chắn muốn xóa banner "${bannerTitle}"?`,
			'Xác nhận xóa',
			(isConfirmed) => {
				if (isConfirmed) {
					_bannerService.deleteBanner(bannerId)
						.done(() => {
							abp.notify.success('Xóa banner thành công!');
							_$bannersTable.ajax.reload();
						})
						.fail((error) => {
							let errorMsg = error?.message || 'Có lỗi xảy ra khi xóa banner';
							abp.notify.error(errorMsg);
						});
				}
			}
		);
	}

	// Event handlers for modal events
	abp.event.on('banner.created', (data) => {
		_$bannersTable.ajax.reload();
	});

	abp.event.on('banner.edited', (data) => {
		_$bannersTable.ajax.reload();
	});

})(jQuery);
