(function () {
  var _orderService = abp.services.app.orders;
  var _locationService = abp.services.app.location;
  var l = abp.localization.getSource('SimpleTaskApp');
  var _locations = []; // Thêm biến _locations bị thiếu

  var _selectedDateRange = {
    StartTime: null,
    EndTime: null
  };

  // Khởi tạo khi document ready
  $(document).ready(function () {
    initializeDateRangePicker();
    bindEvents();
    initializeLocationOnClick();
  });

  function initializeDateRangePicker() {
    $('#StartEndRange').daterangepicker({
      autoUpdateInput: false,
      opens: 'left',
      locale: {
        format: 'DD/MM/YYYY',
        applyLabel: 'Áp dụng',
        cancelLabel: 'Hủy bỏ',
        fromLabel: 'Từ',
        toLabel: 'Đến',
        customRangeLabel: 'Phạm vi tùy chỉnh',
        firstDay: 1
      }
    });

    // Sự kiện apply - QUAN TRỌNG: phải dùng 'apply.daterangepicker'
    $('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
      $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));

      // CẬP NHẬT _selectedDateRange - SỬA ĐỊNH DẠNG
      _selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
      _selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
    });

    $('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
      $(this).val('');
      _selectedDateRange.StartTime = null;
      _selectedDateRange.EndTime = null;
    });
  }

  $('#ResetFilters').click(function () {
    $('#ProductSearchForm')[0].reset();
    _selectedDateRange.StartTime = null;
    _selectedDateRange.EndTime = null;
    $('#StartEndRange').val('');
    // _$productsTable.ajax.reload(); // Comment lại nếu không cần
  });

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
        'data-matinhBNV': province.matinhBNV // Sửa từ 'data-phuongxa' thành 'data-matinhBNV'
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

  function initializeLocationOnClick() {
    $("#province").on('click', function () {
      if (_locations.length === 0) {
        initializeLocationData();
      }
    });
  }

  // Public functions để sử dụng bên ngoài
  window.orderApp = {
    getSelectedAddress: getSelectedAddress,
    getSelectedDateRange: function () {
      return _selectedDateRange;
    },
    resetForm: function () {
      $('#province').val('').trigger('change');
      $('#addressDetail').val('');
      $('#zipcode').val('');
      $('#otherReceiver').prop('checked', false);
      $('#changeAddress').val('');
      updateSelectedAddress();
    }
  };

})();