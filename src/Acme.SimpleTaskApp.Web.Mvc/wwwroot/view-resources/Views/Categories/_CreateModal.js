(function ($) {
    // Category Create Modal (ABP ModalManager compatible)
    app.modals.CategoryCreateModal = function () {
        var _modalManager;
        var _categoryService = abp.services.app.category;
        var _$form = null;
        var l = abp.localization.getSource('SimpleTaskApp');

        console.log('[CategoryCreateModal] Script loaded');

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var $modal = _modalManager.getModal();
            _$form = $modal.find('form[name=CreateForm]');

            console.log('[CategoryCreateModal] init called. form exists:', _$form.length);

            if (!_categoryService) {
                console.error('[CategoryCreateModal] category service not found');
            }

            // jQuery Validate config (giống pattern file HolidaySetting mẫu)
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
                        Name: { required: true, minlength: 2, maxlength: 256 },
                        Description: { required: true, minlength: 2, maxlength: 500 },
                        Order: { required: true, digits: true, min: 0, max: 999999 }
                    },
                    messages: {
                        Name: {
                            required: 'Tên danh mục không được để trống',
                            minlength: 'Tên quá ngắn',
                            maxlength: 'Tên quá dài'
                        },
                        Description: {
                            required: 'Mô tả không được để trống',
                            minlength: 'Mô tả quá ngắn',
                            maxlength: 'Mô tả quá dài'
                        },
                        Order: {
                            required: 'Vui lòng nhập thứ tự',
                            digits: 'Chỉ cho phép số nguyên',
                            min: 'Không được âm',
                            max: 'Quá lớn'
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

            // Chặn ký tự không phải số cho Order
            $modal.off('keypress.categoryOrder').on('keypress.categoryOrder', 'input[name=Order]', function (e) {
                var k = e.which;
                if (k < 48 || k > 57) {
                    e.preventDefault();
                }
            });
        };

        // ABP sẽ tự gọi hàm save này khi click button .save-button trong modal
        this.save = function () {
            console.log('[CategoryCreateModal] save() invoked');
            if (!_$form) {
                console.error('[CategoryCreateModal] Form not initialized');
                return;
            }

            if ($.fn.validate && !_$form.valid()) {
                console.log('[CategoryCreateModal] form invalid');
                return;
            }

            // Lấy dữ liệu form
            var category = _$form.serializeFormToObject();
            // Chuẩn hóa dữ liệu số
            if (category.ParentId === '') category.ParentId = null;
            if (category.Order !== undefined) category.Order = parseInt(category.Order || 0, 10);

            console.log('[CategoryCreateModal] payload:', category);

            _modalManager.setBusy(true);
            _categoryService.createCategory(category)
                .done(function () {
                    abp.notify.info(l('SavedSuccessfully'));
                    abp.event.trigger('category.created');
                    _modalManager.close();
                })
                .fail(function (err) {
                    console.error('[CategoryCreateModal] create failed', err);
                    var msg = err && err.responseJSON && err.responseJSON.error && err.responseJSON.error.message
                        ? err.responseJSON.error.message
                        : 'Tạo danh mục thất bại';
                    abp.notify.error(msg);
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);