(function ($) {
  var _categoryService = abp.services.app.category,
    l = abp.localization.getSource('SimpleTaskApp'),
    $tree = $('#categoryTree'),
    $details = $('#categoryDetails'),
    selectedNode = null;

  var _permissions = {
    create: abp.auth.hasPermission('Pages.category.create'),
    edit: abp.auth.hasPermission('Pages.category.edit'),
    delete: abp.auth.hasPermission('Pages.category.delete'),
  };

  function convertToJsTreeFormat(categories) {
    return categories.map(function (category) {
      var node = {
        id: category.id.toString(),
        text: category.name,
        data: category
      };

      if (category.children && category.children.length > 0) {
        node.children = convertToJsTreeFormat(category.children);
      }

      return node;
    });
  }

  function loadTree() {
    _categoryService.getAllCategoriesTree({}).done(function (result) {
      var data = convertToJsTreeFormat(result || []);
      $tree.jstree('destroy');
      $tree.jstree({
        core: {
          data: data,
          themes: { responsive: true }
        }
      });
      // Khi cây đã sẵn sàng, mở toàn bộ node
      $tree.on('ready.jstree', function () {
        $tree.jstree('open_all');
      });

      $tree.on('select_node.jstree', function (e, data) {
        var categoryId = data.node.id;
        loadCategoryDetails(categoryId);
      });
    }).fail(function () {
      abp.notify.error(l('CouldNotLoadCategories'));
    });
  }

  function loadCategoryDetails(categoryId) {
    _categoryService.getByIdCategory({ id: categoryId }).done(function (category) {
      showDetails(category);
    }).fail(function () {
      // Nếu không có API getById, sử dụng data từ node
      var nodeData = $tree.jstree(true).get_node(categoryId);
      if (nodeData && nodeData.data) {
        showDetails(nodeData.data);
      }
    });
  }

  function showDetails(category) {
    if (!category) {
      clearDetails();
      return;
    }

    var html = `
    <h4>Tên danh mục: ${category.name || ''}</h4>
    <p>Mô tả: ${category.description || ''}</p>
    <p><strong>${l('CreationTime')}:</strong> ${moment(category.creationTime).format('L LT')}</p>
    <p><strong>Danh Mục cha:</strong> ${category.parent && category.parent.name ? category.parent.name : 'None'}</p>
    <p><strong>Thứ tự:</strong> ${category.order || '0'}</p>
    <input type="hidden" id="selectedCategoryId" value="${category.id}" />
`;
    $details.html(html);
    $('#editSelected').show();
    $('#deleteSelected').show();
  }

	function clearDetails() {
		$details.html('<p class="text-muted">' + 'Chọn một danh mục trong cây danh mục' + '</p>');
		$('#editSelected').hide();
		$('#deleteSelected').hide();
		selectedNode = null;
	}

	// Modal create sử dụng ModalManager
	var _createModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Categories/CreateModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Categories/_CreateModal.js',
		modalClass: 'CategoryCreateModal',
		modalSize: 'modal-lg'
	});

	$('#CreateNewButton').click(function () {
		_createModal.open();
	});

	// Modal edit sử dụng ModalManager
	var _editModal = new app.ModalManager({
		viewUrl: abp.appPath + 'Categories/EditModal',
		scriptUrl: abp.appPath + 'view-resources/Views/Categories/_EditModal.js',
		modalClass: 'CategoryEditModal',
		modalSize: 'modal-lg'
	});

	$('#editSelected').on('click', function () {
		var categoryId = $('#selectedCategoryId').val();
		// Mở modal và truyền id vào
		_editModal.open({ categoryId: categoryId });
	});


	$('#deleteSelected').on('click', function () {
		var categoryId = $('#selectedCategoryId').val();
		var categoryName = $details.find('h4').text();

		if (!categoryId) return;

		abp.message.confirm(
			abp.utils.formatString(l('AreYouSureWantToDelete'), categoryName),
			null,
			function (isConfirmed) {
				if (isConfirmed) {
					_categoryService.deleteCategory({ id: categoryId }).done(function () {
						abp.notify.info(l('SuccessfullyDeleted'));
						loadTree();
						clearDetails();
					}).fail(function () { abp.notify.error(l('CouldNotDelete')); });
				}
			}
		);
	});

	// Refresh tree on create/edit events
	abp.event.on('category.created', function () {
		loadTree();
	});
	abp.event.on('category.edited', function () {
		loadTree();
	});

	// init
	$(function () {
		clearDetails();
		loadTree();
	});
})(jQuery);