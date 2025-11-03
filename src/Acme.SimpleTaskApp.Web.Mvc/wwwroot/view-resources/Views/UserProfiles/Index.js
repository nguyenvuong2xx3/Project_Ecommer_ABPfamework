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
    // Xử lý nút Lưu - SỬA LẠI
    $('#saveProfile').on('click', function (e) {
      e.preventDefault();
      console.log('Nút Lưu được click');
      updateUserProfile();
    });

    // Xử lý nút hủy
    $('#cancelProfileEdit').on('click', function () {
      console.log('Nút Hủy được click');
      resetProfileForm();
    });
  }

  function updateUserProfile() {
    try {
      var fullName = $('#FullName').val().trim();
      if (!fullName) {
        abp.notify.warn('Vui lòng nhập họ và tên!');
        return;
      }

      // Xử lý tên
      var parts = fullName.split(' ');
      var name = parts.length > 0 ? parts.pop() : '';
      var surname = parts.length > 0 ? parts.join(' ') : '';

      var formData = {
        Id: $('#UserId').val(),
        Name: name,
        Surname: surname,
        PhoneNumber: $('#PhoneNumber').val(),
        GioiTinh: $('input[name="GioiTinh"]:checked').val(),
        TinhThanh: $('#TinhThanh').val(),
        PhuongXa: $('#PhuongXa').val(),
        DiaChiChiTiet: $('#DiaChiChiTiet').val(),
        IsDiaChiMacDinh: $('#IsDiaChiMacDinh').is(':checked') ? 1 : 0,
      };

      console.log('Dữ liệu gửi đi:', formData);

      // Set busy
      var $formArea = $('#updateProfileForm').closest('.info-card');
      abp.ui.setBusy($formArea);

      // Gọi API - SỬA LẠI PHẦN NÀY
      _userService.updateForCustomer(formData).done(function (result) {
        console.log('API thành công:', result);
        abp.notify.success('Cập nhật thông tin thành công!');
        abp.ui.clearBusy($formArea); // Clear busy khi thành công
      }).fail(function (error) {
        console.error('API lỗi:', error);
        abp.notify.error('Cập nhật thông tin thất bại: ' + (error.message || ''));
        abp.ui.clearBusy($formArea); // Clear busy khi lỗi
      });

    } catch (error) {
      console.error('Lỗi trong updateUserProfile:', error);
      abp.notify.error('Có lỗi xảy ra: ' + error.message);
      abp.ui.clearBusy();
    }
  }


  

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