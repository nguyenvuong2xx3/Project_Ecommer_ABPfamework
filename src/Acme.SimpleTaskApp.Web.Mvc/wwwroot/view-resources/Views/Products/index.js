(function ($) {  
   var _productService = abp.services.app.product,  
       l = abp.localization.getSource('SimpleTaskApp'),  
       _$modal = $('#ProductCreateModal'),  
       _$form = _$modal.find('form'),  
       _$table = $('#ProductsTable');  

    var _$productsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _productService.searchProducts,
            inputFilter: function () {
                return $('#ProductSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$productsTable.draw(false),

                              
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
                data: 'price',
                sortable: false
            },
            {
                targets: 3,
                data: 'creationTime',
                sortable: false,
                render: data => new Date(data).toLocaleDateString('vi-VN')
            },
            {
                targets: 4,
                data: 'image',
                sortable: false,
                render: function (data, type, row) {
                    if (data) {
                        return `<img src="${data}" alt="Ảnh sản phẩm" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
                    }
                    return '<span class="text-muted">Không có ảnh</span>';
                }

            },
            {
              targets: 5,
              data: 'state',
              sortable: false,
              render: function (data, type, row) {
                switch (data) {
                  case 0: return '<span class="badge bg-success">Còn hàng</span>';
                  case 1: return '<span class="badge bg-warning">Hết hàng</span>';
                }
              }
            },
            {
               targets: 6,
               data: null,
               sortable: false,
               autoWidth: true,
               defaultContent: '',
               render: (data, type, row, meta) => {
                   return [
                       `   <button type="button" class="btn btn-sm bg-secondary edit-product" data-product-id="${row.id}" data-toggle="modal" data-target="#ProductEditModal">`,
                       `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                       '   </button>',
                       `   <button type="button" class="btn btn-sm bg-danger delete-product" data-product-id="${row.id}" data-product-name="${row.name}">`,
                       `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                       '   </button>',
                       `   <button type="button" class="btn btn-sm bg-info detail-product" data-product-id="${row.id}" data-toggle="modal" >`,
                       `       <i class="fas fa-eye"></i> ${l('Details')}`,
                       '   </button>'
                   ].join('');
               }
            }
       ]
   });  


   // refresh
    $(document).on('click', '.buttons-refresh', function () {
        _$productsTable.ajax.reload();
    });


    // lưu sản phẩm
    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var product = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
        var formData = new FormData(_$form[0]);
        abp.ui.setBusy(_$modal);
        $.ajax({

            url: abp.appPath + 'Products/Create', // Đường dẫn đến phương thức trong controller
            type: 'POST',
            processData: false, // Important! Không xử lý dữ liệu
            contentType: false, // Important!  Không đặt kiểu dữ liệu
            data: formData,
            error: function (xhr, textStatus, errorThrown) {
                var errorMessage;
                if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
                    errorMessage = xhr.responseJSON.errors.join("<br/>");
                }
                else {
                    errorMessage = "Có lỗi xảy ra khi tạo mới sản phẩm (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
                }
                $("#error-message").html(errorMessage).show();
            }
        }).done(function () {
            /*resetDefaultImage();*/
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('Lưu thành công'));
            _$productsTable.ajax.reload();

        }).always(function () {

            abp.ui.clearBusy(_$modal);

        });
    });


    /// xóa sản phẩm
   $(document).on('click', '.delete-product', function () {  
       var productId = $(this).attr("data-product-id");  
       var productName = $(this).attr('data-product-name');  

       deleteProduct(productId, productName);  
   });  
    function deleteProduct(productId, productName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('Bạn có muốn xóa'),
                productName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    $.ajax({
                        url: '/Products/Delete',
                        type: 'POST',
                        data: { id: productId }
                    }).done(() => {
                        abp.notify.info(l('Xoá thành công'));
                        _$productsTable.ajax.reload();
                    });
                }
            }
        );
    }

    // Xử lý sự kiện click nút "Edit"
    $(document).on('click', '.edit-product', function () {
        var productId = $(this).data("product-id");

        abp.ajax({
            url: abp.appPath + 'Products/EditModal?productId=' + productId,
            type: 'POST',
            processData: false, // Important! Không xử lý dữ liệu
            contentType: false, // Important!  Không đặt kiểu dữ liệu
            dataType: 'html',
            success: function (content) {
                // Chèn nội dung vào modal-content
                $('#ProductEditModal .modal-content').html(content);
                // Hiển thị modal
                $('#ProductEditModal').modal('show');
            },
            error: function (e) {
                abp.notify.error('Could not load edit form');
            }
        });
    });

    // xem chi tiết sản phẩm
    $(document).on('click', '.detail-product', function () {
        var productId = $(this).data("product-id");
        abp.ajax({
            url: abp.appPath + 'Products/DetailModal?productId='+ productId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                // Chèn nội dung vào modal-content
                $('#ProductDetailModal .modal-content').html(content);
                // Hiển thị modal
                $('#ProductDetailModal').modal('show');
            },
            error: function (e) {
                abp.notify.error('Could not load detail form');
            }
        });
    });

   $(document).on('click', 'a[data-target="#ProductCreateModal"]', (e) => {  
       $('.nav-tabs a[href="#product-details"]').tab('show')  
   });  

   abp.event.on('product.edited', (data) => {  
       _$productsTable.ajax.reload();  
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

   //validate
  $(document).ready(function () {
    $("form[name='productCreateForm']").validate({
      rules: {
        Name: {
          required: true,
          minlength: 5,
          maxlength: 128
        },
        Description: {
          required: true,
          minlength: 10,
          maxlength: 256
        },
        Price: {
          required: true,
          number: true,
          min: 1000
        },
        ImageFile: {
          required: true,
          //accept: "jpg|jpeg|png|gif"
        },
        State: {
          required: true
        },
        CategoryId: {
          required: true
        }
      },
      messages: {
        Name: {
          required: "Tên sản phẩm không được để trống",
          minlength: "Tên sản phẩm phải có ít nhất 5 ký tự",
          maxlength: "Tên sản phẩm tối đa 128 ký tự"
        },
        Description: {
          required: "Mô tả không được để trống",
          minlength: "Mô tả phải có ít nhất 10 ký tự",
          maxlength: "Mô tả tối đa 256 ký tự"
        },
        Price: {
          required: "Giá sản phẩm không được để trống",
          min: "Giá phải lớn hơn 1000",
        },
        ImageFile: {
          required: "Vui lòng chọn ảnh sản phẩm",
          extension: "Ảnh phải có định dạng .jpg, .jpeg, .png, .gif"
        },
      },
      errorElement: "div",
      errorClass: "text-danger"
    });
  });

  
})(jQuery);
