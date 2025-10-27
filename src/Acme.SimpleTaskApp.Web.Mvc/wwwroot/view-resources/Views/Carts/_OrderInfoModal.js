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
        populateProvinces();
      }).catch(function (error) {
        console.error('Lỗi khi tải dữ liệu địa phương:', error);
        abp.notify.error('Không thể tải dữ liệu địa phương!');
      });
    }

    function populateProvinces() {
      var $province = $('#province');
      $province.html('<option selected value="">Chọn Tỉnh/Thành phố</option>');

      $.each(_locations, function (index, province) {
        $province.append($('<option>', {
          value: province.matinhTMS,
          text: province.tentinhmoi,
          'data-phuongxa': JSON.stringify(province.phuongxa || [])
        }));
      });
    }

    function populateWards(provinceCode) {
      var $ward = $('#ward');
      $ward.html('<option selected value="">Chọn Phường/Xã</option>');

      if (!provinceCode) {
        $ward.prop('disabled', true);
        return;
      }

      var selectedProvince = _locations.find(function (p) {
        return p.matinhTMS === provinceCode;
      });

      if (selectedProvince && selectedProvince.phuongxa && selectedProvince.phuongxa.length > 0) {
        $.each(selectedProvince.phuongxa, function (index, ward) {
          $ward.append($('<option>', {
            value: ward.maphuongxa,
            text: ward.tenphuongxa
          }));
        });
        $ward.prop('disabled', false);
      } else {
        $ward.html('<option selected value="">Không có dữ liệu phường/xã</option>');
        $ward.prop('disabled', true);
      }
    }

    function bindEvents() {
      // Xử lý khi chọn tỉnh/thành phố
      $('#province').on('change', function () {
        var selectedValue = $(this).val();
        if (selectedValue) {
          populateWards(selectedValue);
          updateSelectedAddress();
        } else {
          $('#ward').html('<option selected value="">Chọn Phường/Xã</option>').prop('disabled', true);
          updateSelectedAddress();
        }
      });

      // Xử lý khi chọn phường/xã
      $('#ward').on('change', updateSelectedAddress);

      // Xử lý khi nhập địa chỉ chi tiết
      $('#addressDetail').on('input', updateSelectedAddress);

      // Xử lý khi thay đổi địa chỉ khác
      $('#changeAddress').on('input', function () {
        var newAddress = $(this).val();
        if (newAddress) {
          $('.address-selected').html(
            '<i class="fas fa-check-circle text-success me-2"></i>Địa chỉ đang chọn: ' + newAddress
          );
        } else {
          updateSelectedAddress();
        }
      });
    }

    function updateSelectedAddress() {
      var provinceText = $('#province option:selected').text();
      var wardText = $('#ward option:selected').text();
      var addressDetail = $('#addressDetail').val();

      // Nếu đang nhập địa chỉ mới thì không cập nhật
      if ($('#changeAddress').val()) {
        return;
      }

      if (provinceText && provinceText !== 'Chọn Tỉnh/Thành phố') {
        var addressParts = [];
        if (addressDetail) addressParts.push(addressDetail);
        if (wardText && wardText !== 'Chọn Phường/Xã' && wardText !== 'Không có dữ liệu phường/xã') {
          addressParts.push(wardText);
        }
        addressParts.push(provinceText);

        var fullAddress = addressParts.join(', ');
        $('.address-selected').html(
          '<i class="fas fa-check-circle text-success me-2"></i>Địa chỉ đang chọn: ' + fullAddress
        );
      } else {
        $('.address-selected').html(
          '<i class="fas fa-check-circle text-success me-2"></i>Địa chỉ đang chọn: Chưa chọn địa chỉ'
        );
      }
    }

    function getSelectedAddress() {
      var $province = $('#province option:selected');
      var $ward = $('#ward option:selected');

      return {
        province: {
          code: $('#province').val(),
          name: $province.text(),
          matinhBNV: $province.data('matinhBNV')
        },
        ward: {
          code: $('#ward').val(),
          name: $ward.text()
        },
        addressDetail: $('#addressDetail').val(),
        zipcode: $('#zipcode').val(),
        otherReceiver: $('#otherReceiver').is(':checked'),
        fullAddress: $('.address-selected').text().replace('Địa chỉ đang chọn: ', '')
      };
    }

    this.save = function () {
      var addressData = getSelectedAddress();

      // Validate dữ liệu
      if (!addressData.province.code) {
        abp.notify.warn('Vui lòng chọn Tỉnh/Thành phố!');
        return;
      }

      if (!addressData.ward.code) {
        abp.notify.warn('Vui lòng chọn Phường/Xã!');
        return;
      }

      if (!addressData.addressDetail) {
        abp.notify.warn('Vui lòng nhập địa chỉ chi tiết!');
        return;
      }

      // Gửi dữ liệu đi
      _modalManager.setResult(addressData);
      _modalManager.close();

      abp.notify.success('Đã lưu địa chỉ giao hàng!');
    };

    // Hàm public để reset form nếu cần
    this.resetForm = function () {
      $('#province').val('').trigger('change');
      $('#addressDetail').val('');
      $('#zipcode').val('');
      $('#otherReceiver').prop('checked', false);
      $('#changeAddress').val('');
      updateSelectedAddress();
    };
  };
})();