(function ($) {
    var _categoryService = abp.services.app.category, // Đảm bảo service name đúng
        l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#CategoryEditModal'),
        _$form = _$modal.find('form');

      _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
          return;
        }

        var category = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
        var formData = new FormData(_$form[0]);

        abp.ui.setBusy(_$modal);
        abp.ui.setBusy(_$form);

        _categoryService.updateCategory(category)
          .done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('category.edited', category);

            // Reset form sau khi cập nhật thành công
            _$form[0].reset();
            abp.notify.info(l('Lưu thành công'));
            abp.event.trigger('category.edited', category);
          })
          .always(function () {
            abp.ui.clearBusy(_$form);
            abp.ui.clearBusy(_$modal);
          });
      });



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
})(jQuery);