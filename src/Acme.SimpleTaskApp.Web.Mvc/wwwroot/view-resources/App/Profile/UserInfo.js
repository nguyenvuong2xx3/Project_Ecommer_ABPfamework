(function () {
	$(function () {
		var _$rolesTable = $('#RolesTable');
		var _roleService = abp.services.app.role;
		var _userService = abp.services.app.user;

		var _entityTypeFullName = 'BBK.SaaS.Authorization.Roles.Role';

		var _$numberOfFilteredPermission = $('#NumberOfFilteredPermission');

		var _selectedPermissionNames = [];

		var _permissions = {
			create: abp.auth.hasPermission('Pages.Administration.Roles.Create'),
			edit: abp.auth.hasPermission('Pages.Administration.Roles.Edit'),
			delete: abp.auth.hasPermission('Pages.Administration.Roles.Delete'),
		};

		var _createOrEditModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Roles/CreateOrEditModal',
			scriptUrl: abp.appPath + 'view-resources/App/Roles/_CreateOrEditModal.js',
			modalClass: 'CreateOrEditRoleModal',
			cssClass: 'scrollable-modal',
		});

		var _serviceLookupModal = app.modals.LookupModal.create({
			title: app.localize('SelectAService'),
			serviceMethod: abp.services.app.dataLookup.findServices,
		});


		$('#AddServiceButton').click(function () {
			_serviceLookupModal.open(
				{
					extraFilters: {
						//tenantId: data.record.id,
					},
					//title: app.localize('SelectAUser'),
				},
				function (selectedItem) {
					console.log(selectedItem);
					abp.services.app.dataLookup
						.getServiceInfo(selectedItem)
						.done(function (result) {
							console.log(result);
							abp.notify.info(app.localize('SavedSuccessfully'));
						})
						.always(function () {
						});

				}
			);
		});

		$('#RefreshRolesButton').click(function (e) {
			e.preventDefault();
			getRoles();
		});

		function getRoles() {
			dataTable.ajax.reload();
		}

		abp.event.on('app.createOrEditRoleModalSaved', function () {
			getRoles();
		});
	});
})();
