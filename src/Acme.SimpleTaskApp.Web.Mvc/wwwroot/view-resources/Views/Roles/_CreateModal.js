(function ($) {
	// Role Create Modal (ABP ModalManager compatible)
	app.modals.CreateModal = function () {
		var _modalManager;
		var _roleService = abp.services.app.role;
		var _$form = null;
		var l = abp.localization.getSource('SimpleTaskApp');
		var _permissionTree = null;

		console.log('[CreateModal] Script loaded');

		this.init = function (modalManager) {
			_modalManager = modalManager;
			var $modal = _modalManager.getModal();
			_$form = $modal.find('form[name=roleCreateForm]');

			console.log('[CreateModal] init called. form exists:', _$form.length);

			if (!_roleService) {
				console.error('[CreateModal] role service not found');
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
			$modal.off('shown.bs.tab.permissions').on('shown.bs.tab.permissions', '#permissions-tab', function () {
				loadPermissions();
			});

			// Tự động load permissions nếu đang ở tab permissions
			if ($modal.find('#permissions-tab').hasClass('active')) {
				loadPermissions();
			}

			// Setup search functionality
			setupSearch();

			// Setup select all/deselect all
			setupSelectAll();
		};

		// Chuyển dữ liệu từ API sang dạng jsTree
		function buildTreeData(items) {
			function convert(node) {
				return {
					id: node.name,
					text: node.displayName,
					children: (node.children || []).map(convert),
					icon: node.children && node.children.length > 0 ? "fas fa-folder" : "fas fa-key",
					state: {
						opened: true,
						selected: false
					}
				};
			}
			return items.map(convert);
		}

		// Hiển thị cây permission với jsTree
		function renderPermissionTree(items) {
			var treeData = buildTreeData(items);
			var $container = $('#permissionTree');
			var $loading = $('.permission-tree-loading');

			$loading.hide();
			$container.show();

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
					'three_state': true, // BẬT chế độ three_state
					'cascade': 'up+down+undetermined', // BẬT cascade cả lên và xuống
					'tie_selection': false
				}
			}).on('ready.jstree', function (e, data) {
				data.instance.open_all();
				_permissionTree = data.instance;
				updateSelectedCount();
			}).on('changed.jstree', function (e, data) {
				updateSelectedCount();

				// Xử lý cascade selection manually nếu cần
				if (data.action === 'select_node' || data.action === 'deselect_node') {
					handleCascadeSelection(data);
				}
			});
		}

		// Xử lý cascade selection manually để đảm bảo logic
		function handleCascadeSelection(data) {
			var selected = data.selected;
			var deselected = data.deselected;

			if (data.action === 'select_node') {
				// Khi chọn node, chọn tất cả con của nó
				selected.forEach(function (nodeId) {
					var node = _permissionTree.get_node(nodeId);
					if (node.children && node.children.length > 0) {
						_permissionTree.check_node(node.children_d);
					}
				});
			} else if (data.action === 'deselect_node') {
				// Khi bỏ chọn node, bỏ chọn tất cả con của nó
				deselected.forEach(function (nodeId) {
					var node = _permissionTree.get_node(nodeId);
					if (node.children && node.children.length > 0) {
						_permissionTree.uncheck_node(node.children_d);
					}
				});
			}
		}

		// Cập nhật số lượng permissions đã chọn
		function updateSelectedCount() {
			if (_permissionTree) {
				var selected = _permissionTree.get_checked(); // Lấy tất cả nodes được checked
				var leafNodes = selected.filter(function (nodeId) {
					var node = _permissionTree.get_node(nodeId);
					return node.children.length === 0; // Chỉ đếm leaf nodes (permissions thực tế)
				});
				$('#selectedPermissionsCount').text(leafNodes.length);
			}
		}

		// Tìm kiếm permissions
		function setupSearch() {
			var $modal = _modalManager.getModal();
			$modal.off('input.permissionSearch').on('input.permissionSearch', '#permissionSearch', function () {
				var searchString = $(this).val();
				if (_permissionTree) {
					_permissionTree.search(searchString);
				}
			});
		}

		// Chọn/Bỏ chọn tất cả
		function setupSelectAll() {
			var $modal = _modalManager.getModal();
			$modal.off('click.selectAll').on('click.selectAll', '#selectAllPermissions', function () {
				if (_permissionTree) {
					_permissionTree.check_all();
					updateSelectedCount();
				}
			});

			$modal.off('click.deselectAll').on('click.deselectAll', '#deselectAllPermissions', function () {
				if (_permissionTree) {
					_permissionTree.uncheck_all();
					updateSelectedCount();
				}
			});
		}

		// Load dữ liệu permission và render cây
		function loadPermissions() {
			var $modal = _modalManager.getModal();
			var $loading = $modal.find('.permission-tree-loading');
			var $tree = $modal.find('#permissionTree');

			$loading.show();
			$tree.hide();

			abp.services.app.role.getAllTreePermissions()
				.done(function (result) {
					if (result && result.items) {
						renderPermissionTree(result.items);
					} else {
						$loading.html('<div class="alert alert-warning">Không có dữ liệu quyền.</div>');
					}
				})
				.fail(function (error) {
					console.error('[CreateModal] Load permissions failed:', error);
					$loading.html('<div class="alert alert-danger">Không thể tải danh sách quyền.</div>');
					abp.notify.error('Không thể tải danh sách quyền.');
				});
		}

		// ABP sẽ tự gọi hàm save này khi click button .save-button trong modal
		this.save = function () {
			console.log('[CreateModal] save() invoked');
			if (!_$form) {
				console.error('[CreateModal] Form not initialized');
				return;
			}

			if ($.fn.validate && !_$form.valid()) {
				console.log('[CreateModal] form invalid');
				return;
			}

			// Lấy dữ liệu form
			var role = _$form.serializeFormToObject();

			// Lấy permissions từ jsTree - chỉ lấy leaf nodes (permissions thực tế)
			if (_permissionTree) {
				var allChecked = _permissionTree.get_checked();
				role.grantedPermissions = allChecked.filter(function (nodeId) {
					var node = _permissionTree.get_node(nodeId);
					return node.children.length === 0; // Chỉ lấy permissions thực tế, không lấy group
				});
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

			console.log('[CreateModal] payload:', role);

			_modalManager.setBusy(true);
			_roleService.create(role)
				.done(function () {
					abp.notify.info(l('SavedSuccessfully'));
					abp.event.trigger('role.created');
					_modalManager.close();
				})
				.fail(function (error) {
					console.error('[CreateModal] create failed', error);
					var msg = 'Vui lòng nhập đầy đủ thông tin và thử lại.';
					abp.notify.error(msg);
				})
				.always(function () {
					_modalManager.setBusy(false);
				});
		};
	};
})(jQuery);