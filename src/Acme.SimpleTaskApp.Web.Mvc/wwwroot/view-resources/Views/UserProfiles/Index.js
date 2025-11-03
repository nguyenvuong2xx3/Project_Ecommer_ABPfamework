(function () {
  var _orderService = abp.services.app.orders;
  var _locationService = abp.services.app.location;
  var _userService = abp.services.app.user;
  var l = abp.localization.getSource('SimpleTaskApp');
  var _locations = [];
  var _currentStatus = null;
  var _currentPaymentMethod = null;

  var _selectedDateRange = {
    StartTime: null,
    EndTime: null
  };

  $(document).ready(function () {
    initializeDateRangePicker();
    bindEvents();
    initializeLocationOnClick();
    initializeProfileForms();
    loadOrders();
  });

  function loadOrders() {
    var input = {
      Status: _currentStatus,
      PaymentMethod: _currentPaymentMethod,
      StartTime: _selectedDateRange.StartTime,
      EndTime: _selectedDateRange.EndTime,
      Sorting: "CreationTime DESC"
    };

    abp.ui.setBusy($('#orders-tab-pane'));
		console.log('Tải đơn hàng với bộ lọc:', input);
    _orderService.getOrderByCurrentUser(input)
      .done(function (result) {
        console.log('Danh sách đơn hàng:', result);
        renderOrders(result);
        abp.ui.clearBusy($('#orders-tab-pane'));
      })
      .fail(function (error) {
        console.error('Lỗi khi tải đơn hàng:', error);
        abp.notify.error('Không thể tải danh sách đơn hàng!');
        abp.ui.clearBusy($('#orders-tab-pane'));
      });
  }

  function renderOrders(orders) {
    var $container = $('#orders-tab-pane');
    var $emptyContainer = $container.find('.empty-orders-container');
    var $ordersList = $container.find('.orders-list-container');

    if ($ordersList.length === 0) {
      $emptyContainer.after('<div class="orders-list-container"></div>');
      $ordersList = $container.find('.orders-list-container');
    }

    if (!orders || orders.length === 0) {
      $emptyContainer.show();
      $ordersList.hide().empty();
      return;
    }

    $emptyContainer.hide();

    var html = '';
    orders.forEach(function (order) {
      html += createOrderCard(order);
    });

    $ordersList.html(html).show();
  }

  function createOrderCard(order) {
    var statusText = getStatusText(order.status);
    var statusClass = getStatusClass(order.status);
    var totalAmount = formatCurrency(order.totalAmount || order.totalPrice);
    var creationTime = formatDate(order.creationTime);

    // Lấy số lượng sản phẩm và tên sản phẩm đầu tiên
    var productCount = order.orderDetails ? order.orderDetails.length : 0;
    var firstProductName = 'Sản phẩm';
    if (order.orderDetails && order.orderDetails.length > 0) {
      var firstDetail = order.orderDetails[0];
      firstProductName = firstDetail.productVariant ? firstDetail.productVariant.productName : 'Sản phẩm';
      if (productCount > 1) {
        firstProductName += ` và ${productCount - 1} sản phẩm khác`;
      }
    }

    // Lấy thông tin người nhận và địa chỉ
    var recipientName = order.fullName || 'Không có thông tin';
    var address = getFullAddress(order);

    return `
  <div class="order-card card mb-3" data-order-id="${order.id}">
    <div class="card-body p-3">
      <div class="d-flex justify-content-between align-items-start">
        <!-- Thông tin bên trái -->
        <div class="flex-grow-1">
          <div class="d-flex align-items-center mb-1">
            <span class="fw-bold me-2">#${order.code || order.id}</span>
            <span class="badge ${statusClass}">${statusText}</span>
          </div>
          <div class="text-muted small mb-1">
            ${firstProductName} • ${creationTime}
          </div>
          <div class="small">
            <strong>Người nhận:</strong> ${recipientName}
          </div>
          <div class="small text-muted">
            <strong>Địa chỉ:</strong> ${address}
          </div>
        </div>
        
        <!-- Thông tin bên phải -->
        <div class="text-end">
          <div class="fw-bold text-primary mb-2">${totalAmount}</div>
          <div>
            <button class="btn btn-outline-primary btn-sm view-order-detail" data-order-id="${order.id}">
              Chi tiết
            </button>
          </div>
        </div>
      </div>
      
      <!-- Chi tiết đơn hàng (ẩn ban đầu) -->
      <div class="order-detail-content mt-3" id="order-detail-${order.id}" style="display: none;">
        ${renderOrderDetails(order)}
      </div>
    </div>
  </div>
`;
  }

  // Hàm render chi tiết đơn hàng - chỉ hiển thị sản phẩm
  function renderOrderDetails(order) {
    if (!order.orderDetails || order.orderDetails.length === 0) {
      return '<div class="text-muted">Không có chi tiết đơn hàng</div>';
    }

    var html = `
    <div class="border-top pt-3">
      <h6 class="mb-3">Chi tiết sản phẩm:</h6>
  `;

    order.orderDetails.forEach(function (detail, index) {
      var productName = detail.productVariant ? detail.productVariant.productName : 'Sản phẩm';
      var quantity = detail.quantity || 1;
      var price = formatCurrency(detail.price || detail.newPrice || 0);
      var totalPrice = formatCurrency((detail.price || detail.newPrice || 0) * quantity);

      html += `
      <div class="order-detail-item d-flex align-items-center mb-2 p-2 border-bottom">
        <div class="me-3 text-muted small">${index + 1}.</div>
        <div class="flex-grow-1">
          <div class="fw-bold">${productName}</div>
          <div class="text-muted small">Số lượng: ${quantity}</div>
        </div>
        <div class="text-end">
          <div class="fw-bold">${price}</div>
          <div class="text-muted small">Thành tiền: ${totalPrice}</div>
        </div>
      </div>
    `;
    });

    html += `</div>`;
    return html;
  }

  // Hàm lấy địa chỉ đầy đủ
  function getFullAddress(order) {
    var addressParts = [];
    if (order.diaChiChiTiet) addressParts.push(order.diaChiChiTiet);
    if (order.phuongXa) addressParts.push(order.phuongXa);
    if (order.tinhThanh) addressParts.push(order.tinhThanh);

    return addressParts.length > 0 ? addressParts.join(', ') : 'Không có địa chỉ';
  }

  // Hàm lấy text phương thức thanh toán
  function getPaymentMethodText(paymentMethod) {
    var paymentMethods = {
      0: 'Thanh toán khi nhận hàng (COD)',
      1: 'Chuyển khoản ngân hàng',
      2: 'Ví điện tử',
      3: 'Thẻ tín dụng'
    };
    return paymentMethods[paymentMethod] || 'Không xác định';
  }

  // Xử lý sự kiện xem chi tiết đơn hàng
  function initializeOrderDetailEvents() {
    $(document).on('click', '.view-order-detail', function () {
      var orderId = $(this).data('order-id');
      var $detailContent = $('#order-detail-' + orderId);
      var $button = $(this);

      if ($detailContent.is(':visible')) {
        $detailContent.hide();
        $button.text('Chi tiết');
      } else {
        $detailContent.show();
        $button.text('Ẩn chi tiết');
      }
    });
  }

  function getStatusText(status) {
    var statusMap = {
      0: 'Chờ xác nhận',
      1: 'Đang xử lý',
      2: 'Đang giao hàng',
      3: 'Thành công',
      4: 'Đã hủy',
      5: 'Hoàn trả'
    };
    return statusMap[status] || 'Không xác định';
  }

  function getStatusClass(status) {
    var classMap = {
      0: 'bg-warning text-dark',
      1: 'bg-info text-white',
      2: 'bg-primary text-white',
      3: 'bg-success text-white',
      4: 'bg-danger text-white',
      5: 'bg-secondary text-white'
    };
    return classMap[status] || 'bg-light text-dark';
  }

  function formatCurrency(amount) {
    if (!amount) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  }

  function formatDate(dateString) {
    if (!dateString) return '';
    var date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Xử lý bộ lọc trạng thái
  function initializeOrderFilters() {
    $('.order-status-filters .nav-link').on('click', function (e) {
      e.preventDefault();

      $('.order-status-filters .nav-link').removeClass('active');
      $(this).addClass('active');

      var statusValue = $(this).data('status');
      _currentStatus = statusValue === 'all' ? null : parseInt(statusValue);

      loadOrders();
    });
  }

  // Xử lý bộ lọc phương thức thanh toán
  function initializePaymentMethodFilter() {
    $('#PaymentMethod').on('change', function () {
      var paymentMethodValue = $(this).val();
      _currentPaymentMethod = paymentMethodValue === '' ? null : parseInt(paymentMethodValue);
      loadOrders();
    });
  }

  // Reset bộ lọc
  function initializeResetFilter() {
    $('#ResetFilters').click(function () {
      _selectedDateRange.StartTime = null;
      _selectedDateRange.EndTime = null;
      _currentPaymentMethod = null;
      $('#StartEndRange').val('');
      $('#PaymentMethod').val('');
      $('.order-status-filters .nav-link').removeClass('active');
      $('.order-status-filters .nav-link[data-status="all"]').addClass('active');
      _currentStatus = null;
      loadOrders();
    });
  }

  function initializeProfileForms() {
    $('#saveProfile').on('click', function (e) {
      e.preventDefault();
      console.log('Nút Lưu được click');
      updateUserProfile();
    });

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

      var $formArea = $('#updateProfileForm').closest('.info-card');
      abp.ui.setBusy($formArea);

      _userService.updateForCustomer(formData).done(function (result) {
        console.log('API thành công:', result);
        abp.notify.success('Cập nhật thông tin thành công!');
        abp.ui.clearBusy($formArea);
      }).fail(function (error) {
        console.error('API lỗi:', error);
        abp.notify.error('Cập nhật thông tin thất bại: ' + (error.message || ''));
        abp.ui.clearBusy($formArea);
      });

    } catch (error) {
      console.error('Lỗi trong updateUserProfile:', error);
      abp.notify.error('Có lỗi xảy ra: ' + error.message);
      abp.ui.clearBusy();
    }
  }

  function resetProfileForm() {
    $('#FullName').val('@Model.User.FullName');
    $('#PhoneNumber').val('@Model.User.PhoneNumber');

    var gender = '@Model.User.Gender';
    if (gender === 'Male') {
      $('#genderMale').prop('checked', true);
    } else if (gender === 'Female') {
      $('#genderFemale').prop('checked', true);
    }
  }

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
      loadOrders();
    });

    $('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
      $(this).val('');
      _selectedDateRange.StartTime = null;
      _selectedDateRange.EndTime = null;
      loadOrders();
    });
  }

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
    var currentValue = $tinhThanh.val();
    $tinhThanh.html('<option value="">Chọn Tỉnh/Thành phố</option>');

    $.each(_locations, function (index, tinhThanh) {
      $tinhThanh.append($('<option>', {
        value: tinhThanh.matinhTMS,
        text: tinhThanh.tentinhmoi,
        'data-matinhBNV': tinhThanh.matinhBNV
      }));
    });

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

    initializeOrderFilters();
    initializeOrderDetailEvents();
    initializePaymentMethodFilter();
    initializeResetFilter();
  }

  function initializeLocationOnClick() {
    $("#TinhThanh").on('click', function () {
      if (_locations.length === 0) {
        initializeLocationData();
      }
    });
  }

})();