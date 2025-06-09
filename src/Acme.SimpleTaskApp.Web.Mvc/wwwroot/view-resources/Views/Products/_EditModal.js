(function ($) {
    // Sửa từ "_userService" sang "_productService"
    var _productService = abp.services.app.product, // Đảm bảo service name đúng
        l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#ProductEditModal'),
        _$form = _$modal.find('form');


  // Đảm bảo chỉ gắn sự kiện 1 lần
  $(document).off('change', '#productImage');
  $(document).on('change', '#productImage', function (event) {
    const input = event.target;
    const $preview = $('#newProductImagePreview');
    if (input.files && input.files[0]) {
      const reader = new FileReader();
      reader.onload = function (e) {
        $preview.attr('src', e.target.result).show();
      };
      reader.readAsDataURL(input.files[0]);
    } else {
      $preview.attr('src', '').hide();
    }
  });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var product = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
        var formData = new FormData(_$form[0]);
        abp.ui.setBusy(_$modal);

        $.ajax({

            url: abp.appPath + 'Products/Update', // Đường dẫn đến phương thức trong controller
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
            abp.event.trigger('product.edited', product);

        }).always(function () {

            abp.ui.clearBusy(_$modal);

        });
    });



    //function save() {
    //    if (!_$form.valid()) {
    //        return;
    //    }

    //    // Lấy dữ liệu từ form
    //    var product = _$form.serializeFormToObject();
    //    var formData = new FormData(_$form[0]);
    //    abp.ui.setBusy(_$form);

    //    // Gọi service update PRODUCT
    //    _productService.updateProducts(product).done(function () {
    //        _$modal.modal('hide');
    //        abp.notify.info(l('SavedSuccessfully'));
    //        abp.event.trigger('product.edited'); // Kích hoạt sự kiện reload bảng
    //    }).always(function () {
    //        abp.ui.clearBusy(_$form);
    //    });
    //}

    // Xử lý sự kiện click nút Save
    //_$form.closest('div.modal-content').find(".save-button").click(function (e) {
    //    e.preventDefault();
    //    save();
    //});

    // Xử lý phím Enter
    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });

    // Focus vào trường đầu tiên khi modal hiển thị
    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });


  //validate
  $(document).ready(function () {
    $("form[name='ProductEditForm']").validate({
      rules: {
        Name: {
          required: true,
          minlength: 5,
          maxlength: 256
        },
        Description: {
          required: true,
          minlength: 10,
          maxlength: 500
        },
        Price: {
          required: true,
          number: true,
          min: 1000,
        },
        ImageFile: {
        }
      },
      messages: {
        Name: {
          required: "Tên sản phẩm không được để trống",
          minlength: "Tên sản phẩm phải có ít nhất 5 ký tự",
          maxlength: "Tên sản phẩm tối đa 256 ký tự"
        },
        Description: {
          required: "Mô tả không được để trống",
          minlength: "Mô tả phải có ít nhất 10 ký tự",
          maxlength: "Mô tả tối đa 500 ký tự"
        },
        Price: {
          required: "Giá sản phẩm không được để trống",
          min: "Giá không thể nhỏ hơn 1000",
        },
        ImageFile: {
          extension: "Chỉ chấp nhận tệp ảnh (.jpg, .jpeg, .png, .gif)"
        }
      },
      errorElement: "div",
      errorClass: "text-danger"
    });
  });
})(jQuery);