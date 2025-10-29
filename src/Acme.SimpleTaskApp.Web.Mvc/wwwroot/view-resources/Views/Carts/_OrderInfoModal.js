(function () {
  app.modals.OrderInfoModal = function () {
    var _modalManager;
    var _locationService = abp.services.app.location;
    var _locations = [];
    _$form = _$modal.find('form'),


    this.init = function (modalManager) {
      _modalManager = modalManager;
      initializeLocationData();
      bindEvents();
    };

    function initializeLocationData() {
      _locationService.getAllDonViHanhChinh().then(function (result) {
        _locations = result;
        populateTinhThanh();
      }).catch(function (error) {
        console.error('Lỗi khi tải dữ liệu địa phương:', error);
        abp.notify.error('Không thể tải dữ liệu địa phương!');
      });
    }

    function populateTinhThanh() {
      var $tinhThanh = $('#TinhThanh');
      $tinhThanh.html('<option value="">Chọn Tỉnh/Thành phố</option>');

      $.each(_locations, function (index, tinhThanh) {
        $tinhThanh.append($('<option>', {
          value: tinhThanh.matinhTMS,
          text: tinhThanh.tentinhmoi,
          'data-matinhBNV': tinhThanh.matinhBNV
        }));
      });

      var currentValue = $tinhThanh.data('current-value');
      if (currentValue) {
        $tinhThanh.val(currentValue);
        populatePhuongXa(currentValue);
      }
    }

    function populatePhuongXa(tinhThanhCode) {
      var $phuongXa = $('#PhuongXa');
      $phuongXa.html('<option value="">Chọn Phường/Xã</option>');

      if (!tinhThanhCode) {
        $phuongXa.prop('disabled', true);
        return;
      }

      var selectedTinhThanh = _locations.find(function (p) {
        return p.matinhTMS === tinhThanhCode;
      });

      if (selectedTinhThanh && selectedTinhThanh.phuongxa && selectedTinhThanh.phuongxa.length > 0) {
        $.each(selectedTinhThanh.phuongxa, function (index, phuongXa) {
          $phuongXa.append($('<option>', {
            value: phuongXa.maphuongxa,
            text: phuongXa.tenphuongxa
          }));
        });
        $phuongXa.prop('disabled', false);

        var currentValue = $phuongXa.data('current-value');
        if (currentValue) {
          $phuongXa.val(currentValue);
        }
      } else {
        $phuongXa.html('<option value="">Không có dữ liệu phường/xã</option>');
        $phuongXa.prop('disabled', true);
      }
    }

    function bindEvents() {
      $('#TinhThanh').on('change', function () {
        var selectedValue = $(this).val();
        if (selectedValue) {
          populatePhuongXa(selectedValue);
        } else {
          $('#PhuongXa').html('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);
        }
      });

      $('.delivery-option').on('click', function () {
        $('.delivery-option').removeClass('active');
        $(this).addClass('active');
      });

      $('#otherReceiver').on('change', function () {
        if ($(this).is(':checked')) {
          console.log('Người khác nhận hàng được chọn');
        }
      });
    }

    this.save = function () {
      var fullName = $('#FullName').val().trim();
      var phoneNumber = $('#PhoneNumber').val().trim();
      var diaChiChiTiet = $('#addressDetail').val().trim();
      var tinhThanhCode = $('#TinhThanh').val();
      var phuongXaCode = $('#PhuongXa').val();
      var gioiTinh = $('input[name="GioiTinh"]:checked').val();
      var deliveryMethod = $('.delivery-option.active').attr('id');
      var otherReceiver = $('#otherReceiver').is(':checked');

      // Validate dữ liệu
      if (!fullName) {
        abp.notify.warn('Vui lòng nhập họ và tên!');
        return;
      }

      if (!phoneNumber) {
        abp.notify.warn('Vui lòng nhập số điện thoại!');
        return;
      }

      if (!tinhThanhCode) {
        abp.notify.warn('Vui lòng chọn Tỉnh/Thành phố!');
        return;
      }

      if (!phuongXaCode) {
        abp.notify.warn('Vui lòng chọn Phường/Xã!');
        return;
      }

      if (!diaChiChiTiet) {
        abp.notify.warn('Vui lòng nhập địa chỉ chi tiết!');
        return;
      }

      // Lấy thông tin hiển thị
      var tinhThanhName = $('#TinhThanh option:selected').text();
      var phuongXaName = $('#PhuongXa option:selected').text();

      // Tách họ và tên
      var parts = fullName.split(' ');
      var name = parts.length > 0 ? parts.pop() : '';
      var surname = parts.length > 0 ? parts.join(' ') : '';

      // Trả về kết quả
      _modalManager.setResult({
        userInfo: {
          gioiTinh: gioiTinh,
          name: name,
          surname: surname,
          fullName: fullName,
          phoneNumber: phoneNumber
        },
        deliveryMethod: deliveryMethod,
        address: {
          tinhThanh: {
            code: tinhThanhCode,
            name: tinhThanhName
          },
          phuongXa: {
            code: phuongXaCode,
            name: phuongXaName
          },
          diaChiChiTiet: diaChiChiTiet,
          otherReceiver: otherReceiver
        },
        submittedAt: new Date().toISOString()
      });

      _modalManager.close();
    };

    // Hàm public để reset form
    this.resetForm = function () {
      $('#deliveryForm')[0].reset();
      $('#TinhThanh').val('').trigger('change');
      $('#addressDetail').val('');
      $('#otherReceiver').prop('checked', false);
      $('.delivery-option').removeClass('active');
      $('#homeDelivery').addClass('active');
    };

    // Hàm public để set dữ liệu mặc định
    this.setDefaultData = function (userData) {
      if (userData) {
        $('#FullName').val(userData.fullName || '');
        $('#PhoneNumber').val(userData.phoneNumber || '');

        if (userData.tinhThanh) {
          $('#TinhThanh').data('current-value', userData.tinhThanh);
        }

        if (userData.phuongXa) {
          $('#PhuongXa').data('current-value', userData.phuongXa);
        }

        if (userData.diaChiChiTiet) {
          $('#addressDetail').val(userData.diaChiChiTiet);
        }
      }
    };
  };
})();