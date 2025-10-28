(function () {
  var _orderService = abp.services.app.orders;
  var _locationService = abp.services.app.location;
  var _userService = abp.services.app.user; // Thêm service user
  var l = abp.localization.getSource('SimpleTaskApp');
  var _locations = [];

  var _selectedDateRange = {
    StartTime: null,
    EndTime: null
  };

  $(document).ready(function () {
    initializeDateRangePicker();
    bindEvents();
    initializeLocationOnClick();
    initializeProfileForms();
  });

  function initializeProfileForms() {
    // Xử lý form cập nhật thông tin cá nhân
    $('#updateProfileForm').on('submit', function (e) {
      e.preventDefault();
      updateUserProfile();
    });

    // Xử lý form cập nhật địa chỉ
    //$('#updateAddressForm').on('submit', function (e) {
    //  e.preventDefault();
    //  updateUserAddress();
    //});

    // Xử lý nút hủy
    $('#cancelProfileEdit').on('click', function () {
      resetProfileForm();
    });
  }

  function updateUserProfile() {
    var formData = {
      Id: $('#UserId').val(),
      FullName: $('#FullName').val(),
      PhoneNumber: $('#PhoneNumber').val(),
      Gender: $('input[name="Gender"]:checked').val(),
      TinhThanh: $('#TinhThanh').val(),
      PhuongXa: $('#PhuongXa').val(),
      DiaChiChiTiet: $('#DiaChiChiTiet').val(),
      IsDefault: $('#IsDefault').is(':checked'),
    };

    abp.ui.setBusy($('#updateProfileForm'), true);

    _userService.updateForCustomer(formData)
      .then(function () {
        abp.notify.success('Cập nhật thông tin thành công!');
        abp.ui.setBusy($('#updateProfileForm'), false);
      })
      .catch(function (error) {
        console.error('Lỗi khi cập nhật thông tin:', error);
        abp.notify.error('Cập nhật thông tin thất bại!');
        abp.ui.setBusy($('#updateProfileForm'), false);
      });
  }

  //function updateUserAddress() {
  //  var formData = {
  //    Id: $('#AddressId').val() || 0,
  //    TinhThanh: $('#TinhThanh').val(),
  //    PhuongXa: $('#PhuongXa').val(),
  //    DiaChiChiTiet: $('#DiaChiChiTiet').val(),
  //    IsDefault: $('#IsDefault').is(':checked'),
  //    UserId: $('#UserId').val()
  //  };

  //  // Validate dữ liệu
  //  if (!formData.TinhThanh || !formData.PhuongXa || !formData.DiaChiChiTiet) {
  //    abp.notify.warn('Vui lòng nhập đầy đủ thông tin địa chỉ!');
  //    return;
  //  }

  //  abp.ui.setBusy($('#updateAddressForm'), true);

  //  _userService.updateAddressAsync(formData)
  //    .then(function () {
  //      abp.notify.success('Cập nhật địa chỉ thành công!');
  //      abp.ui.setBusy($('#updateAddressForm'), false);
  //    })
  //    .catch(function (error) {
  //      console.error('Lỗi khi cập nhật địa chỉ:', error);
  //      abp.notify.error('Cập nhật địa chỉ thất bại!');
  //      abp.ui.setBusy($('#updateAddressForm'), false);
  //    });
  //}

  function resetProfileForm() {
    // Reset form về giá trị ban đầu (có thể load lại từ server nếu cần)
    $('#FullName').val('@Model.User.FullName');
    $('#PhoneNumber').val('@Model.User.PhoneNumber');

    var gender = '@Model.User.Gender';
    if (gender === 'Male') {
      $('#genderMale').prop('checked', true);
    } else if (gender === 'Female') {
      $('#genderFemale').prop('checked', true);
    }
  }

  // Các hàm hiện có giữ nguyên
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

    $('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
      $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
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
  });

  function initializeLocationData() {
    _locationService.getAllDonViHanhChinh().then(function (result) {
      _locations = result;
      populateTinhThanh(); // Đổi từ populateProvinces sang populateTinhThanh
    }).catch(function (error) {
      console.error('Lỗi khi tải dữ liệu địa phương:', error);
      abp.notify.error('Không thể tải dữ liệu địa phương!');
    });
  }

  function populateTinhThanh() {
    var $tinhThanh = $('#TinhThanh');
    var currentValue = $tinhThanh.val();
    $tinhThanh.html('<option value="">Chọn Tỉnh/Thành phố</option>');

    $.each(_locations, function (index, tinhThanh) {
      $tinhThanh.append($('<option>', {
        value: tinhThanh.matinhTMS,
        text: tinhThanh.tentinhmoi,
        'data-matinhBNV': tinhThanh.matinhBNV
      }));
    });

    // Khôi phục giá trị đã chọn
    if (currentValue) {
      $tinhThanh.val(currentValue);
      populatePhuongXa(currentValue);
    }
  }

  function populatePhuongXa(tinhThanhCode) {
    var $phuongXa = $('#PhuongXa');
    var currentValue = $phuongXa.val();
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

      // Khôi phục giá trị đã chọn
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
  }


  function initializeLocationOnClick() {
    $("#TinhThanh").on('click', function () {
      if (_locations.length === 0) {
        initializeLocationData();
      }
    });
  }

})();