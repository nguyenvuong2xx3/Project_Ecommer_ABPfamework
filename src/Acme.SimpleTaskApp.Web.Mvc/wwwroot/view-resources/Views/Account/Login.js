(function () {
  $('#ReturnUrlHash').val(location.hash);

  var _$form = $('#LoginForm');

  _$form.submit(function (e) {
    e.preventDefault();

    if (!_$form.valid()) {
      return;
    }

    abp.ui.setBusy(
      $('body'),

      abp.ajax({
        contentType: 'application/x-www-form-urlencoded',
        url: _$form.attr('action'),
        data: _$form.serialize()
      }).done(function (response) {
        if (response && response.targetUrl) {
          // Chuyển trang về TargetUrl và reload để đồng bộ cookie
          window.location.href = response.targetUrl;
          setTimeout(function () {
            window.location.reload();
          }, 200);
        }
      }).fail(function () {
        abp.message.error('Đăng nhập thất bại. Vui lòng thử lại.');
      })
    );
  });
})();
