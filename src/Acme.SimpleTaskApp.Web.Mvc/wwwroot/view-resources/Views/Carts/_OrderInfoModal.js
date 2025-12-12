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
      initializeValidation();
      bindEvents();
    };

    function initializeValidation() {
      if ($.fn.validate) {
        // Custom validation cho số điện thoại Việt Nam
        $.validator.addMethod("vietnamPhone", function (value, element) {
          if (!value) return true;
          return /^(0[3|5|7|8|9])+([0-9]{8})$/.test(value);
        }, "Số điện thoại không hợp lệ");

        // Custom validation cho dropdown select
        $.validator.addMethod("selectRequired", function (value, element) {
          return value !== null && value !== "";
        }, "Vui lòng chọn một giá trị");

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
            FullName: {
              required: true,
              minlength: 2,
              maxlength: 100
            },
            PhoneNumber: {
              required: true,
              vietnamPhone: true
            },
            TinhThanh: {
              selectRequired: true
            },
            PhuongXa: {
              selectRequired: true
            }
          },
          messages: {
            FullName: {
              required: 'Họ tên không được để trống',
              minlength: 'Họ tên phải có ít nhất 2 ký tự',
              maxlength: 'Họ tên không được quá 100 ký tự'
            },
            PhoneNumber: {
              required: 'Số điện thoại không được để trống',
              vietnamPhone: 'Số điện thoại không hợp lệ (VD: 0912345678)'
            },
            TinhThanh: {
              selectRequired: 'Vui lòng chọn Tỉnh/Thành phố'
            },
            PhuongXa: {
              selectRequired: 'Vui lòng chọn Phường/Xã'
            }
          },
          errorPlacement: function (error, element) {
            error.addClass('text-danger');

            if (element.closest('.input-group').length) {
              error.insertAfter(element.closest('.input-group'));
            } else if (element.is('select')) {
              error.insertAfter(element);
            } else {
              error.insertAfter(element);
            }
          },
          success: function (label, element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            label.remove();
          }
        });
      }
    }

    // Validate giới tính thủ công
    function validateGioiTinh() {
      var gioiTinh = $('input[name="GioiTinh"]:checked').val();
      var $gioiTinhContainer = $('input[name="GioiTinh"]').closest('.mb-3');
      var $errorMsg = $gioiTinhContainer.find('.gender-error');

      if (gioiTinh === undefined) {
        // Thêm error message nếu chưa có
        if ($errorMsg.length === 0) {
          $gioiTinhContainer.append('<div class="gender-error text-danger mt-1">Vui lòng chọn giới tính</div>');
        }
        return false;
      } else {
        // Xóa error message nếu có
        $errorMsg.remove();
        return true;
      }
    }

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
      var $phuongXa = $('#phuongXa');
      $phuongXa.html('<option value="">Chọn Phường/Xã</option>');

      if (!tinhThanhCode) {
        $phuongXa.prop('disabled', true);
        return;
      }
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
        // Reset và revalidate phường xã khi thay đổi tỉnh thành
        $('#phuongXa').val('').removeClass('is-valid is-invalid');

        if (selectedValue) {
          populatePhuongXa(selectedValue);
        } else {
          $('#phuongXa').html('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);
        }

        // Trigger validation cho tỉnh thành
        if (_$form.data('validator')) {
          _$form.validate().element('#tinhThanh');
        }
      });

      $('#phuongXa').on('change', function () {
        // Trigger validation khi chọn phường xã
        if (_$form.data('validator')) {
          _$form.validate().element('#phuongXa');
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

    // Validate địa chỉ chi tiết (optional nhưng nếu có phải >= 5 ký tự)
    function validateAddressDetail() {
      var address = $('#addressDetail').val().trim();
      if (address && address.length < 5) {
        return {
          valid: false,
          message: 'Địa chỉ chi tiết phải có ít nhất 5 ký tự'
        };
      }
      return { valid: true };
    }

    this.save = function () {
      // Validate form trước khi save


      // Validate giới tính riêng
      var isGioiTinhValid = validateGioiTinh();

      if (!_$form.valid() || !isGioiTinhValid) {
        abp.notify.warn('Vui lòng kiểm tra lại thông tin!');
        return;
      }

      // Validate thêm địa chỉ chi tiết
      var addressValidation = validateAddressDetail();
      if (!addressValidation.valid) {
        $('#addressDetail').addClass('is-invalid');
        abp.notify.warn(addressValidation.message);
        return;
      }

      var fullName = $('#fullName').val().trim();
      var phoneNumber = $('#phoneNumber').val().trim();

      // Regex: chỉ số và đúng 10 ký tự
      var phoneRegex = /^[0-9]{10}$/;

      if (!phoneRegex.test(phoneNumber)) {
        abp.notify.error("Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số.");
        return;
      }
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
      _$form[0].reset();
      _$form.find('. is-valid, .is-invalid').removeClass('is-valid is-invalid');
      _$form.find('.invalid-feedback').remove();
      $('#tinhThanh').val('').trigger('change');
      $('#addressDetail').val('');
      $('#otherReceiver').prop('checked', false);
      $('.delivery-option').removeClass('active');
      $('#homeDelivery').addClass('active');
    };

    // Hàm public để set dữ liệu mặc định
    this.setDefaultData = function (userData) {
      if (userData) {
        $('#fullName').val(userData.fullName || '');
        $('#phoneNumber').val(userData.phoneNumber || '');

        if (userData.tinhThanh) {
          $('#tinhThanh').data('current-value', userData.tinhThanh);
        }

        if (userData.phuongXa) {
          $('#phuongXa').data('current-value', userData.phuongXa);
        }

        if (userData.diaChiChiTiet) {
          $('#addressDetail').val(userData.diaChiChiTiet);
        }
      }
    };
  };
})();