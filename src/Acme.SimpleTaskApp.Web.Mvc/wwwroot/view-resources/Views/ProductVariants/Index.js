(function ($) {
	var _productVariantService = abp.services.app.productVariant,
		l = abp.localization.getSource('SimpleTaskApp'),
		_$modal = $('#ProductVariantCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#ProductVariantsTable');


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

	var _$productvariantsTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _productVariantService.getAllProductVariant, 
			inputFilter: function () {
				return $('#ProductSearchForm').serializeFormToObject(true);
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
				sortable: false
			},
			{
				targets: 6,
				data: 'price',
				sortable: false
			},
			{
				targets: 7,
				data: 'creationTime',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<span class="badge bg-info">${data}</span>`;
					}
				}

			},
			{
				targets: 8,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					return [
						`   <button type="button" class="btn btn-sm bg-secondary edit-productvariant" data-productvariant-id="${row.id}" data-toggle="modal" data-target="#ProductEditModal">`,
						`       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-danger delete-productvariant" data-productvariant-id="${row.id}" data-product-name="${row.name}">`,
						`       <i class="fas fa-trash"></i> ${l('Delete')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-info detail-productvariant" style="margin-top: 5px;" data-productvariant-id="${row.id}" data-toggle="modal" >`,
						`       <i class="fas fa-eye"></i> ${l('Details')}`,
						'   </button>'

					].join('');
				}
			}
		]
	});

	// refresh
	$(document).on('click', '.buttons-refresh', function () {
		_$productvariantsTable.ajax.reload();
	});

	$(document).on('click', '.delete-productvariant', function () {
		var productvariantId = $(this).attr("data-productvariant-id");
		var productvariantName = $(this).attr('data-productvariant-name');

		deleteProductVariant(productvariantId, productvariantName);
	});
	function deleteProductVariant(productvariantId, productvariantName) {
		abp.message.confirm(
			abp.utils.formatString(
				l('Bạn có muốn xóa'),
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
					});
				}
			}
		);
	}
	$('.btn-search').on('click', (e) => {
		_$productvariantsTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$productvariantsTable.ajax.reload();
			return false;
		}
	});
})(jQuery);
