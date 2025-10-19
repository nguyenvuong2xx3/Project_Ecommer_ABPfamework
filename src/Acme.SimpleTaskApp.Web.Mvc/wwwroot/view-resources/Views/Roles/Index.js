(function ($) {
	var _roleService = abp.services.app.role,
		l = abp.localization.getSource('SimpleTaskApp'),
		_$modal = $('#RoleCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#RolesTable');


	var _createModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Roles/CreateModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Roles/_CreateModal.js',
		modalClass: 'CreateModal',
		cssClass: 'scrollable-modal',
	});

	var _editModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Roles/EditModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Roles/_EditModal.js',
		modalClass: 'EditModal',
		cssClass: 'scrollable-modal',
	});

	$('#CreateNewButton').click(function () {
		_createModal.open();
	});

	var _$rolesTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _roleService.getPagedRoles,
			inputFilter: function () {
				return $('#RolesSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$rolesTable.draw(false)
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
				className: 'control',
				defaultContent: '',
			},
			{
				targets: 1,
				data: 'name',
				sortable: false
			},
			{
				targets: 2,
				data: 'displayName',
				sortable: false
			},
			{
				targets: 3,
				data: null,
				sortable: false,
				autoWidth: false,
				defaultContent: '',
				render: (data, type, row, meta) => {
					var buttons = [];
					if (!data.isStatic) {
						buttons.push(
							`<button type="button" class="btn btn-sm bg-secondary mr-1 edit-role" data-role-id="${row.id}">`,
							`    <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
							`</button>`
						);
						buttons.push(
							`<button type="button" class="btn btn-sm bg-danger delete-role" data-role-id="${row.id}" data-role-name="${row.name}">`,
							`    <i class="fas fa-trash"></i> ${l('Delete')}`,
							`</button>`
						);
					}
					else {
						buttons.push(
							`<span class="text-muted">${'Vai trò mặc định'}</span>`
						);
					}
					return buttons.join('');
				}
			}
		]
	});

	$(document).on('click', '.delete-role', function () {
		var roleId = $(this).attr("data-role-id");
		var roleName = $(this).attr('data-role-name');

		deleteRole(roleId, roleName);
	});

	$(document).on('click', '.edit-role', function () {
		var roleId = $(this).attr("data-role-id");
		_editModal.open({ roleId: roleId });
	});

	// Reload table khi role được tạo mới
	abp.event.on('role.created', (data) => {
		_$rolesTable.ajax.reload();
	});

	// Reload table khi role được chỉnh sửa
	abp.event.on('role.edited', (data) => {
		_$rolesTable.ajax.reload();
	});

	function deleteRole(roleId, roleName) {
		abp.message.confirm(
			abp.utils.formatString(
				l('AreYouSureWantToDelete'),
				roleName),
			null,
			(isConfirmed) => {
				if (isConfirmed) {
					_roleService.delete({
						id: roleId
					}).done(() => {
						abp.notify.info(l('SuccessfullyDeleted'));
						_$rolesTable.ajax.reload();
					});
				}
			}
		);
	}

	_$modal.on('shown.bs.modal', () => {
		_$modal.find('input:not([type=hidden]):first').focus();
	}).on('hidden.bs.modal', () => {
		_$form.clearForm();
	});

	$('.btn-search').on('click', (e) => {
		_$rolesTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$rolesTable.ajax.reload();
			return false;
		}
	});
})(jQuery);