(function () {
  app.modals.OrderInfoModal = function () {
    var _modalManager;
    var _locationService = abp.services.app.location;
    var _locations = [];
     var _$form = null;


    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$form = $modal.find('form[name=InfoOrder]');
      initializeLocationData();
      bindEvents();
    };

    function initializeLocationData() {
      _locationService.getAllDonViHanhChinh().then(function (result) {
        _locations = result;
        populateTinhThanh();
      }).catch(function (error) {
        abp.notify.error('Không thể tải dữ liệu địa phương!');
      });
    }

    function populateTinhThanh() {
      var $tinhThanh = $('#tinhThanh');
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
      debugger
      var $phuongXa = $('#phuongXa');
      $phuongXa.html('<option value="">Chọn Phường/Xã</option>');

      if (!tinhThanhCode) {
        $phuongXa.prop('disabled', true);
        return;
      }
			console.log(_locations);
      var selectedTinhThanh = _locations.find(function (p) {
        return p.matinhTMS === String(tinhThanhCode);
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
      $('#tinhThanh').on('change', function () {
        var selectedValue = $(this).val();
        if (selectedValue) {
          populatePhuongXa(selectedValue);
        } else {
          $('#phuongXa').html('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);
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
      var fullName = $('#fullName').val().trim();
      var phoneNumber = $('#phoneNumber').val().trim();
      var diaChiChiTiet = $('#addressDetail').val().trim();
      var tinhThanhCode = $('#tinhThanh').val();
      var phuongXaCode = $('#phuongXa').val();
      var gioiTinh = $('input[name="GioiTinh"]:checked').val();
      var deliveryMethod = $('.delivery-option.active').attr('id');
      var otherReceiver = $('#otherReceiver').is(':checked');

      // Lấy thông tin hiển thị
      var tinhThanhName = $('#tinhThanh option:selected').text();
      var phuongXaName = $('#phuongXa option:selected').text();

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