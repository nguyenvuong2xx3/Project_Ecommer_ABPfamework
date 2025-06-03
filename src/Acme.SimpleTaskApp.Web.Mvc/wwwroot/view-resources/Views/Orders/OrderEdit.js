(function ($) {
	var _orderService = abp.services.app.order,
		l = abp.localization.getSource('MyProject'),
		_$modal = $('#OrderEditModal'),
		_$form = _$modal.find('form');


	function save() {
		if (!_$form.valid()) {
			return;
		}

		var order = _$form.serializeFormToObject(); // chuyển form thành object để gửi API

		abp.ui.setBusy(_$form); // hiển thị loading (disable form)
			_orderService.updateOrder(order).done(function () {
			_$modal.modal('hide'); // đóng modal sau khi lưu thành công
			abp.notify.info(l('SavedSuccessfully')); // hiển thị thông báo lưu thành công
			abp.event.trigger('order.edited', order); // kích hoạt sự kiện product.edited
		}).always(function () {
			abp.ui.clearBusy(_$form); // xóa trạng thái loading
		});

		//alway() sau khi API chạy xong( thành công hay thất bại) ==> xóa trạng thái loading
	}

	_$form.closest('div.modal-content').find(".save-button").click(function (e) {
		e.preventDefault(); // ngăn chặn reload trang
		save(); // gọi hàm save() để cập nhật sản phẩm khi bấm lưu
	});

	_$form.find('input').on('keypress', function (e) {
		if (e.which === 13) { // nếu nhấn enter mã 13
			e.preventDefault();
			save();
		}
	});

	//_$modal.on('shown.bs.modal', function () {
	//	_$form.find('input[type=text]:first').focus();
	//});

})(jQuery);
