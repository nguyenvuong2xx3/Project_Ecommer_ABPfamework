(function ($) {
	// Role Edit Modal (ABP ModalManager compatible)
	app.modals.EditModal = function () {
		var _modalManager;
		var _roleService = abp.services.app.role;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');
		var _permissionTree = null;
		var _isStaticRole = false;

		console.log('[EditModal] Script loaded');

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=roleEditForm]');

			console.log('[EditModal] init called. form exists:', _$form.length);

			if (!_roleService) {
				console.error('[EditModal] role service not found');
			}

			// jQuery Validate config
			if ($.fn.validate) {
				_$form.validate({
					validClass: 'valid',
					errorClass: 'invalid-feedback',
					highlight: function (element) {
						$(element).addClass('is-invalid').removeClass('is-valid');
					},
					unhighlight: function (element) {
						$(element).addClass('is-valid').removeClass('is-invalid');
					},
					rules: {
						Name: { required: true, minlength: 2, maxlength: 32 },
						DisplayName: { required: true, minlength: 2, maxlength: 32 },
						Description: { maxlength: 500 }
					},
					messages: {
						Name: {
							required: 'Tên vai trò không được để trống',
							minlength: 'Tên quá ngắn',
							maxlength: 'Tên quá dài'
						},
						DisplayName: {
							required: 'Tên hiển thị không được để trống',
							minlength: 'Tên hiển thị quá ngắn',
							maxlength: 'Tên hiển thị quá dài'
						},
						Description: {
							maxlength: 'Mô tả quá dài'
						}
					},
					errorPlacement: function (error, element) {
						if (element.closest('.input-group').length) {
							error.addClass('text-danger');
							error.insertAfter(element.closest('.input-group'));
						} else if (element.closest('.form-check').length) {
							error.addClass('text-danger');
							error.appendTo(element.closest('.form-check'));
						} else {
							error.addClass('text-danger');
							error.insertAfter(element);
						}
					},
					success: function (label, element) {
						$(element).next('.invalid-feedback').remove();
					}
				});
			}

			// Focus trường đầu tiên
			setTimeout(function () {
				_$form.find('input[name=Name]').first().trigger('focus');
			}, 250);

			// Load permissions khi tab permissions được click
			$modal.off('shown.bs.tab.permissions').on('shown.bs.tab.permissions', '#edit-permissions-tab', function () {
				loadPermissions();
			});

			// Tự động load permissions nếu đang ở tab permissions
			if ($modal.find('#edit-permissions-tab').hasClass('active')) {
				loadPermissions();
			}

			// Setup search functionality
			setupSearch();

			// Setup select all/deselect all
			setupSelectAll();
		};

		// Chuyển dữ liệu từ API sang dạng jsTree
		function buildTreeData(permissions, grantedPermissionNames) {
			function convert(node) {
				// So sánh node.name với các tên trong grantedPermissionNames array
				var isGranted = grantedPermissionNames.indexOf(node.name) !== -1;

				return {
					id: node.name,
					text: node.displayName,
					children: (node.children || []).map(convert),
					icon: node.children && node.children.length > 0 ? "fas fa-folder" : "fas fa-key",
					state: {
						opened: true,
						selected: isGranted,
						disabled: _isStaticRole // Disable nếu là static role     
					}
				};
			}
			return permissions.map(convert);
		}

		// Hiển thị cây permission với jsTree và check permissions đã được granted
		function renderPermissionTree(permissions, grantedPermissionNames) {
			var treeData = buildTreeData(permissions, grantedPermissionNames);
			var $container = $('#editPermissionTree');
			var $loading = $('.permission-tree-loading');

			$loading.hide();
			$container.show();

			// Destroy existing tree if any
			if (_permissionTree) {
				$container.jstree('destroy');
				_permissionTree = null;
			}

			// Hiển thị thông báo nếu là static role
			if (_isStaticRole) {
				var $warning = $container.parent().find('.static-role-warning');
				if ($warning.length === 0) {
					$container.before('<div class="alert alert-warning static-role-warning"><i class="fas fa-exclamation-triangle"></i> Đây là vai trò mặc định, không thể chỉnh sửa quyền.</div>');
				}
			}

			$container.jstree({
				'core': {
					'data': treeData,
					'themes': {
						'dots': true,
						'icons': true,
						'responsive': true
					},
					'check_callback': true
				},
				'plugins': ['checkbox', 'search'],
				'checkbox': {
					'three_state': true,
					'cascade': 'up+down', // Cascade lên cha và xuống con
					'tie_selection': false
				}
			}).on('ready.jstree', function (e, data) {
				data.instance.open_all();
				_permissionTree = data.instance;

				// Auto-check các permissions đã được granted
				grantedPermissionNames.forEach(function (permissionName) {
					var node = _permissionTree.get_node(permissionName);
					if (node) {
						_permissionTree.check_node(node);
					}
				});

				// Disable tree nếu là static role
				if (_isStaticRole) {
					_permissionTree.disable_all();
				}

				updateSelectedCount();
			}).on('check_node.jstree', function (e, data) {
				if (_isStaticRole) return;

				// Khi check một node, tự động check tất cả children
				if (data.node.children && data.node.children.length > 0) {
					data.node.children.forEach(function (childId) {
						_permissionTree.check_node(childId);
					});
				}

				// Tự động check parent nếu có
				if (data.node.parent && data.node.parent !== '#') {
					_permissionTree.check_node(data.node.parent);
				}

				updateSelectedCount();
			}).on('uncheck_node.jstree', function (e, data) {
				if (_isStaticRole) return;

				// Khi uncheck một node cha, tự động uncheck tất cả children
				if (data.node.children && data.node.children.length > 0) {
					data.node.children.forEach(function (childId) {
						_permissionTree.uncheck_node(childId);
					});
				}

				updateSelectedCount();
			}).on('changed.jstree', function (e, data) {
				updateSelectedCount();
			});
		}

		// Cập nhật số lượng permissions đã chọn
		function updateSelectedCount() {
			if (_permissionTree) {
				var selected = _permissionTree.get_checked();
				var leafNodes = selected.filter(function (nodeId) {
					var node = _permissionTree.get_node(nodeId);
					return node.children.length === 0;
				});
				$('#editSelectedPermissionsCount').text(leafNodes.length);
			}
		}

		// Tìm kiếm permissions
		function setupSearch() {
			var $modal = _modalManager.getModal();
			$modal.off('input.permissionSearch').on('input.permissionSearch', '#editPermissionSearch', function () {
				var searchString = $(this).val();
				if (_permissionTree) {
					_permissionTree.search(searchString);
				}
			});
		}

		// Chọn/Bỏ chọn tất cả
		function setupSelectAll() {
			var $modal = _modalManager.getModal();
			$modal.off('click.selectAll').on('click.selectAll', '#editSelectAllPermissions', function () {
				if (_permissionTree && !_isStaticRole) {
					_permissionTree.check_all();
					updateSelectedCount();
				}
			});

			$modal.off('click.deselectAll').on('click.deselectAll', '#editDeselectAllPermissions', function () {
				if (_permissionTree && !_isStaticRole) {
					_permissionTree.uncheck_all();
					updateSelectedCount();
				}
			});
		}

		// Flatten permissions tree thành danh sách tất cả permission names
		function getAllPermissionNames(permissions) {
			var allNames = [];

			function traverse(nodes) {
				nodes.forEach(function (node) {
					allNames.push(node.name);
					if (node.children && node.children.length > 0) {
						traverse(node.children);
					}
				});
			}

			traverse(permissions);
			return allNames;
		}

		// Load dữ liệu permission và granted permissions
		function loadPermissions() {
			var $modal = _modalManager.getModal();
			var $loading = $modal.find('.permission-tree-loading');
			var $tree = $modal.find('#editPermissionTree');
			var roleId = $modal.find('input[name="Id"]').val();

			$loading.show();
			$tree.hide();

			// Xóa warning cũ nếu có
			$modal.find('.static-role-warning').remove();

			// Gọi API để lấy tất cả permissions và granted permissions
			abp.services.app.role.getPermissionAndGranted({ id: roleId })
				.done(function (response) {
					console.log('[EditModal] API response:', response);

					// Response trả về là array với 1 phần tử
					if (response && Array.isArray(response) && response.length > 0) {
						var data = response[0];

						console.log('[EditModal] Extracted data:', data);

						if (data && data.permissions && data.grantedPermissionNames && data.role) {
							// Kiểm tra isStatic từ role
							_isStaticRole = data.role.isStatic === true;

							var grantedPermissionNames = data.grantedPermissionNames;

							// Nếu là static role, tự động grant tất cả permissions
							if (_isStaticRole) {
								grantedPermissionNames = getAllPermissionNames(data.permissions);
								console.log('[EditModal] Static role - auto granted all permissions');
							}

							console.log('[EditModal] Role:', data.role.name);
							console.log('[EditModal] isStatic:', _isStaticRole);
							console.log('[EditModal] Permissions count:', data.permissions.length);
							console.log('[EditModal] Granted permissions:', grantedPermissionNames);

							renderPermissionTree(data.permissions, grantedPermissionNames);
						} else {
							console.error('[EditModal] Invalid data structure:', data);
							$loading.html('<div class="alert alert-warning">Không có dữ liệu quyền.</div>');
						}
					} else {
						console.error('[EditModal] Response is not an array or empty:', response);
						$loading.html('<div class="alert alert-warning">Không có dữ liệu quyền.</div>');
					}
				})
				.fail(function (error) {
					console.error('[EditModal] Load permissions failed:', error);
					$loading.html('<div class="alert alert-danger">Không thể tải danh sách quyền.</div>');
					abp.notify.error('Không thể tải danh sách quyền.');
				});
		}

		// ABP sẽ tự gọi hàm save này khi click button .save-button trong modal
		this.save = function () {
			console.log('[EditModal] save() invoked');

			// Kiểm tra nếu là static role thì không cho phép save
			if (_isStaticRole) {
				abp.notify.warn('Không thể chỉnh sửa vai trò mặc định!');
				return;
			}

			if (!_$form) {
				console.error('[EditModal] Form not initialized');
				return;
			}

			if ($.fn.validate && !_$form.valid()) {
				console.log('[EditModal] form invalid');
				return;
			}

			// Lấy dữ liệu form
			var role = _$form.serializeFormToObject();

			// Lấy TẤT CẢ permissions đã checked (bao gồm cả cha và con)
			if (_permissionTree) {
				var allChecked = _permissionTree.get_checked();
				// Gửi tất cả permissions đã check (không filter)
				role.grantedPermissions = allChecked;
				//if (_isStaticRole) {
				//	role.grantedPermissions = []; // Không gửi gì nếu là static role
				//}

				console.log('[EditModal] All checked nodes:', allChecked);
			} else {
				// Fallback nếu không có tree
				role.grantedPermissions = [];
				var $permissionCheckboxes = _$form.find("input[name='permission']:checked");
				if ($permissionCheckboxes.length) {
					$permissionCheckboxes.each(function () {
						role.grantedPermissions.push($(this).val());
					});
				}
			}

			console.log('[EditModal] payload:', role);

			_modalManager.setBusy(true);
			_roleService.update(role)
				.done(function () {
					abp.notify.info('Cập nhật vai trò thành công!');
					abp.event.trigger('role.edited');
					_modalManager.close();
				})
				.fail(function (err) {
					console.error('[EditModal] update failed', err);
					var msg = err && err.responseJSON && err.responseJSON.error && err.responseJSON.error.message
						? err.responseJSON.error.message
						: 'Cập nhật vai trò thất bại';
					abp.notify.error(msg);
				})
				.always(function () {
					_modalManager.setBusy(false);
				});
		};
	};
})(jQuery);