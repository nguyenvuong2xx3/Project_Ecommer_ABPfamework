(function () {
  app.modals.OrderInfoModal = function () {
    var _locationService = abp.services.app.location;
    var _modalManager;
    var _locations = [];

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

      // Khôi phục giá trị đã chọn nếu có
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

        // Khôi phục giá trị đã chọn nếu có
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
      // Xử lý khi chọn tỉnh/thành phố
      $('#TinhThanh').on('change', function () {
        var selectedValue = $(this).val();
        if (selectedValue) {
          populatePhuongXa(selectedValue);
        } else {
          $('#PhuongXa').html('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);
        }
      });

      // Xử lý hình thức giao hàng
      $('.delivery-option').on('click', function () {
        $('.delivery-option').removeClass('active');
        $(this).addClass('active');
      });

      // Xử lý nút xác nhận
      $('.btn-primary').on('click', function () {
        saveOrderInfo();
      });

      // Xử lý checkbox người khác nhận hàng
      $('#otherReceiver').on('change', function () {
        if ($(this).is(':checked')) {
          // Có thể thêm logic hiển thị form nhập thông tin người nhận khác
          console.log('Người khác nhận hàng được chọn');
        }
      });

      // Xử lý phương thức thanh toán
      $('input[name="paymentMethod"]').on('change', function () {
        console.log('Phương thức thanh toán được chọn:', $(this).val());
      });
    }

    function getFormData() {
      var fullName = $('#FullName').val().trim();
      var parts = fullName.split(' ');
      var name = parts.length > 0 ? parts.pop() : '';
      var surname = parts.length > 0 ? parts.join(' ') : '';

      return {
        // Thông tin người đặt
        userInfo: {
          id: $('#UserId').val(),
          gioiTinh: $('input[name="GioiTinh"]:checked').val(),
          name: name,
          surname: surname,
          fullName: fullName,
          phoneNumber: $('#PhoneNumber').val()
        },

        // Hình thức giao hàng
        deliveryMethod: $('.delivery-option.active').attr('id'), // 'homeDelivery' hoặc 'storePickup'

        // Địa chỉ giao hàng
        address: {
          tinhThanh: {
            code: $('#TinhThanh').val(),
            name: $('#TinhThanh option:selected').text()
          },
          phuongXa: {
            code: $('#PhuongXa').val(),
            name: $('#PhuongXa option:selected').text()
          },
          diaChiChiTiet: $('#addressDetail').val(),
          otherReceiver: $('#otherReceiver').is(':checked')
        },

        // Phương thức thanh toán
        paymentMethod: $('input[name="paymentMethod"]:checked').val()
      };
    }

    function validateFormData(formData) {
      // Validate thông tin người đặt
      if (!formData.userInfo.fullName) {
        abp.notify.warn('Vui lòng nhập họ và tên!');
        return false;
      }

      if (!formData.userInfo.phoneNumber) {
        abp.notify.warn('Vui lòng nhập số điện thoại!');
        return false;
      }

      // Validate địa chỉ giao hàng
      if (!formData.address.tinhThanh.code) {
        abp.notify.warn('Vui lòng chọn Tỉnh/Thành phố!');
        return false;
      }

      if (!formData.address.phuongXa.code) {
        abp.notify.warn('Vui lòng chọn Phường/Xã!');
        return false;
      }

      if (!formData.address.diaChiChiTiet) {
        abp.notify.warn('Vui lòng nhập địa chỉ chi tiết!');
        return false;
      }

      return true;
    }

    function saveOrderInfo() {
      var formData = getFormData();

      if (!validateFormData(formData)) {
        return;
      }

      // Hiển thị loading
      var $submitBtn = $('.btn-primary');
      var originalText = $submitBtn.html();
      $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');

      // Gửi dữ liệu đi (cần tích hợp với service thực tế)
      console.log('Dữ liệu đơn hàng:', formData);

      // Giả lập xử lý
      setTimeout(function () {
        // Gửi dữ liệu về modal manager
        _modalManager.setResult(formData);
        _modalManager.close();

        abp.notify.success('Đã lưu thông tin đơn hàng thành công!');

        // Khôi phục trạng thái nút
        $submitBtn.prop('disabled', false).html(originalText);
      }, 1000);
    }

    this.save = function () {
      saveOrderInfo();
    };

    // Hàm public để reset form nếu cần
    this.resetForm = function () {
      $('#deliveryForm')[0].reset();
      $('#TinhThanh').val('').trigger('change');
      $('#addressDetail').val('');
      $('#otherReceiver').prop('checked', false);
      $('.delivery-option').removeClass('active');
      $('#homeDelivery').addClass('active');
      $('input[name="paymentMethod"][value="cash"]').prop('checked', true);
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