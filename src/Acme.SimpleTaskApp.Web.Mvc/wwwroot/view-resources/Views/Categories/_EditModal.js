(function ($) {
    // Edit modal implemented as ABP Modal-compatible modal class
    app.modals.CategoryEditModal = function () {
        var _modalManager = null;
        var _categoryService = abp.services.app.category;
        var l = abp.localization.getSource('SimpleTaskApp');
        var _$modal = null;
        var _$form = null;

        this.init = function (modalManager) {
            // Support being called by ModalManager or used standalone (when partial is loaded manually)
            _modalManager = modalManager || {
                getModal: function () { return $('#CategoryEditModal'); },
                close: function () { $('#CategoryEditModal').modal('hide'); },
                setBusy: function () { abp.ui.setBusy($('#CategoryEditModal')); },
                clearBusy: function () { abp.ui.clearBusy($('#CategoryEditModal')); }
            };

            _$modal = _modalManager.getModal();
            _$form = _$modal.find('form[name=CategoryEditForm]');

            // Ensure form exists
            if (!_$form || _$form.length === 0) {
                console.warn('[CategoryEditModal] Edit form not found');
            }

            // Setup validation (same rules as create)
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
                        Order: { digits: true, min: 0, max: 999999 }
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
                    }
                });
            }

            // Bind save button click to this.save to support direct partial loads where ModalManager isn't used
            _$modal.off('click', '.save-button').on('click', '.save-button', (e) => {
                e.preventDefault();
                this.save();
            });

            // Enter key handling inside the form
            _$form.find('input').off('keypress.keyenter').on('keypress.keyenter', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    // trigger save
                    this.save ? this.save() : null;
                }
            }.bind(this));

            // Focus the first text input when modal shown
            _$modal.off('shown.bs.modal.focus').on('shown.bs.modal.focus', function () {
                _$form.find('input[type=text]:first').focus();
            });
        };

        this.save = function () {
            if (!_$form || _$form.length === 0) {
                console.error('[CategoryEditModal] Form not initialized');
                return;
            }

            if ($.fn.validate && !_$form.valid()) {
                console.log('[CategoryEditModal] form invalid');
                return;
            }

            var category = _$form.serializeFormToObject();
            // Ensure types
            if (category.ParentId === '') category.ParentId = null;
            if (category.Order !== undefined) category.Order = parseInt(category.Order || 0, 10);

            _modalManager.setBusy();
            _categoryService.updateCategory(category)
                .done(function (result) {
                    _modalManager.close();
                    abp.notify.info(l('SavedSuccessfully'));
                    abp.event.trigger('category.edited', category);
                })
                .fail(function (err) {
                    console.error('[CategoryEditModal] update failed', err);
                    var msg = err && err.responseJSON && err.responseJSON.error && err.responseJSON.error.message
                        ? err.responseJSON.error.message
                        : 'Cập nhật danh mục thất bại';
                    abp.notify.error(msg);
                })
                .always(function () {
                    _modalManager.clearBusy();
                });
        };
    };
})(jQuery);