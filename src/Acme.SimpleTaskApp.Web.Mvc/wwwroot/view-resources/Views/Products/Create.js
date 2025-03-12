(function ($) {
    var _productService = abp.services.app.product,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#ProductsCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ProductsTable');



    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        _productService.createProducts()
            .then(() => {
                abp.notify.success(l('SavedSuccessfully'));
                _$modal.modal('hide');
                _$table.DataTable().ajax.reload(); // Reload bảng
            });
    }
}