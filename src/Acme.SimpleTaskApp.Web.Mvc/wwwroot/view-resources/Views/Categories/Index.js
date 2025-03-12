(function ($) {
  var _categoryService = abp.services.app.category,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$modal = $('#CategoryCreateModal'),
    _$form = _$modal.find('form'),
    _$table = $('#CategoriesTable');

  var _$categoriesTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _categoryService.getAllCategories,
      inputFilter: function () {
        return $('#CategoriesSearchForm').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$categoriesTable.draw(false),


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
        data: 'name',
        sortable: false
      },
      {
        targets: 1,
        data: 'description',
        sortable: false
      },
      {
        targets: 2,
        data: 'creationTime',
        sortable: false,
        render: data => new Date(data).toLocaleDateString('vi-VN')
      },
      {
        targets: 3,
        data: null,
        sortable: false,
        autoWidth: true,
        defaultContent: '',
        render: (data, type, row, meta) => {
          return [
            `   <button type="button" class="btn btn-sm bg-secondary edit-category" data-category-id="${row.id}" data-toggle="modal" data-target="#CategoryEditModal">`,
            `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
            '   </button>',
            `   <button type="button" class="btn btn-sm bg-danger delete-category" data-category-id="${row.id}" data-category-name="${row.name}">`,
            `       <i class="fas fa-trash"></i> ${l('Delete')}`,
            '   </button>',
          ].join('');
        }
      }
    ]
  });


  // refresh
  $(document).on('click', '.buttons-refresh', function () {
    _$categoriesTable.ajax.reload();
  });


  // thêm danh mục
  _$form.find('.save-button').on('click', (e) => {
    e.preventDefault();

    if (!_$form.valid()) {
      return;
    }

    var category = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
    abp.ui.setBusy(_$modal);
    _categoryService.createCategory(category).done(function () {
      _$modal.modal('hide');
      _$form[0].reset();
      abp.notify.info(l('Lưu thành công'));
      _$categoriesTable.ajax.reload();
    }).always(function () {
      abp.ui.clearBusy(_$modal);
    });
  });


  /// xóa danh mục
  $(document).on('click', '.delete-category', function () {
    var categoryId = $(this).attr("data-category-id");
    var categoryName = $(this).attr('data-category-name');

    deleteCategory(categoryId, categoryName);
  });  

   function deleteCategory(categoryId, categoryName) {  
     abp.message.confirm(  
       abp.utils.formatString(  
         l('AreYouSureWantToDelete'),  
         categoryName),  
       null,  
       (isConfirmed) => {  
         if (isConfirmed) {  
           _categoryService.deleteCategory({  
             id: categoryId  
           }).done(() => {  
             abp.notify.info(l('SuccessfullyDeleted'));  
             _$categoriesTable.ajax.reload();  
           });  
         }  
       }  
     );  
   }
  // Xử lý sự kiện click nút "Edit" cho Category
  $(document).on('click', '.edit-category', function (e) {
    e.preventDefault(); // Ngăn hành vi mặc định

    // Lấy ID từ thuộc tính data-category-id (đúng tên)
    var categoryId = $(this).data("category-id");

    abp.ajax({
      url: abp.appPath + 'Categories/EditModal?categoryId=' + categoryId, // Sửa đường dẫn đúng với Category
      type: 'POST',
      dataType: 'html',
      success: function (content) {
        // Chèn nội dung vào modal và hiển thị
        $('#CategoryEditModal .modal-content').html(content);
        $('#CategoryEditModal').modal('show');
      },
      error: function (e) {
        abp.notify.error('Không thể tải form chỉnh sửa');
      }
    });
  });



  //$(document).on('click', 'a[data-target="#CategoryEditModal"]', (e) => {
  //  $('.nav-tabs a[href="#Category-edit"]').tab('show')
  //});

  abp.event.on('category.edited', (data) => {
    _$categoriesTable.ajax.reload();
  });

  _$modal.on('shown.bs.modal', () => {
    _$modal.find('input:not([type=hidden]):first').focus();
  }).on('hidden.bs.modal', () => {
    _$form.clearForm();
  });

  $('.btn-search').on('click', (e) => {
    _$productsTable.ajax.reload();
  });

  $('.txt-search').on('keypress', (e) => {
    if (e.which == 13) {
      _$productsTable.ajax.reload();
      return false;
    }
  });


  // validate form
    $(document).ready(function () {
      $("form[name='pCreateForm']").validate({
        rules: {
          Name: {
            required: true,
            minlength: 2,
            maxlength: 256
          },
          Description: {
            required: true,
            minlength: 2,
            maxlength: 500
          }
        },
        messages: {
          Name: {
            required: "Tên danh mục không được để trống",
            minlength: "Tên danh mục phải có ít nhất 2 ký tự",
            maxlength: "Tên danh mục tối đa 256 ký tự"
          },
          Description: {
            required: "Mô tả không được để trống",
            minlength: "Mô tả phải có ít nhất 2 ký tự",
            maxlength: "Mô tả tối đa 500 ký tự"
          }
        },
        errorElement: "div",
        errorClass: "text-danger"
      });
    });

    $(document).ready(function () {
      $("form[name='CategoryEditForm']").validate({
        rules: {
          Name: {
            required: true,
            minlength: 2,
            maxlength: 256
          },
          Description: {
            required: true,
            minlength: 2,
            maxlength: 500
          }
        },
        messages: {
          Name: {
            required: "Tên danh mục không được để trống",
            minlength: "Tên danh mục phải có ít nhất 2 ký tự",
            maxlength: "Tên danh mục tối đa 256 ký tự"
          },
          Description: {
            required: "Mô tả danh mục không được để trống",
            minlength: "Mô tả danh mục phải có ít nhất 2 ký tự",
            maxlength: "Mô tả danh mục tối đa 500 ký tự"
          }
        },
        errorElement: "div",
        errorClass: "text-danger",
        highlight: function (element) {
          $(element).addClass("is-invalid");
        },
        unhighlight: function (element) {
          $(element).removeClass("is-invalid");
        }
      });
    });


})(jQuery);
