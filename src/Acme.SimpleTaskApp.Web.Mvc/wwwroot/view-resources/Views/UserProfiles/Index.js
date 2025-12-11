(function () {
var _orderService = abp.services.app.orders;
var _locationService = abp.services.app.location;
var _userService = abp.services.app.user;
var _notificationService = abp.services.app.notification;
var l = abp.localization.getSource('SimpleTaskApp');
var _locations = [];
var _currentStatus = null;
var _currentPaymentMethod = null;

var _selectedDateRange = {
  StartTime: null,
  EndTime: null
};

// Notification pagination state
var _notificationState = {
  currentPage: 1,
  pageSize: 10,
  totalCount: 0,
  stateFilter: null,
  isLoaded: false
};

$(document).ready(function () {
  initializeDateRangePicker();
  bindEvents();
  initializeLocationOnClick();
  initializeProfileForms();
  initializeNotificationEvents();
  loadOrders();
});

  function loadOrders() {
    var input = {
      StatusUser: _currentStatus,
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

    // Tạo nút hủy đơn hàng chỉ khi status = 0
    var cancelButton = '';
    if (order.status === 0) {
      cancelButton = `
            <button class="btn btn-outline-danger btn-sm cancel-order ms-2" data-order-id="${order.id}">
                Hủy đơn
            </button>
        `;
    }

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
            ${cancelButton}
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

  function huyUserOrder(orderId) {
    abp.message.confirm(
      'Bạn có chắc chắn muốn hủy đơn hàng này?',
      'Xác nhận hủy đơn hàng',
      function (isConfirmed) {
        if (isConfirmed) {
          abp.ui.setBusy($('#orders-tab-pane'));

          _orderService.huyUserOrder(orderId)
            .done(function (result) {
              console.log('Hủy đơn hàng thành công:', result);
              abp.notify.success('Hủy đơn hàng thành công!');
              loadOrders(); // Tải lại danh sách đơn hàng
              abp.ui.clearBusy($('#orders-tab-pane'));
            })
            .fail(function (error) {
              console.error('Lỗi khi hủy đơn hàng:', error);
              abp.notify.error('Hủy đơn hàng thất bại: ' + (error.message || ''));
              abp.ui.clearBusy($('#orders-tab-pane'));
            });
        }
      }
    );
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
      4: 'Đã hủy bởi hệ thống',
      5: 'Đã hủy'
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
      statusValue = statusValue.toString();

      if (statusValue === 'all') {
        _currentStatus = null; // load tất cả
      } else {
        // Nếu có dấu phẩy → tách thành mảng
        if (statusValue.includes(',')) {
          _currentStatus = statusValue.split(',').map(Number); // [4, 5]
        } else {
          _currentStatus = [parseInt(statusValue)]; // [0], [1], [2], [3], [4]
        }
      }

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
    initializeCancelOrderEvents();
  }
  function initializeCancelOrderEvents() {
    $(document).on('click', '.cancel-order', function () {
      var orderId = $(this).data('order-id');
      huyUserOrder(orderId);
    });
  }
  function initializeLocationOnClick() {
    $("#TinhThanh").on('click', function () {
      if (_locations.length === 0) {
        initializeLocationData();
      }
    });
  }

  // ==================== NOTIFICATION FUNCTIONS ====================

  function initializeNotificationEvents() {
    // Load notifications when tab is clicked
    $('#notification-tab').on('click', function () {
      if (!_notificationState.isLoaded) {
        loadNotifications();
      }
    });

    // Filter by state
    $('#NotificationStateFilter').on('change', function () {
      var value = $(this).val();
      _notificationState.stateFilter = value === '' ? null : parseInt(value);
      _notificationState.currentPage = 1;
      loadNotifications();
    });

    // Mark all as read
    $('#MarkAllAsRead').on('click', function () {
      markAllNotificationsAsRead();
    });

    // Click on notification item to mark as read
    $(document).on('click', '.mark-as-read-btn', function () {
      var notificationId = $(this).data('notification-id');
      markNotificationAsRead(notificationId);
    });

    // Delete notification
    $(document).on('click', '.delete-notification-btn', function () {
      var notificationId = $(this).data('notification-id');
      deleteNotification(notificationId);
    });

    // Pagination click
    $(document).on('click', '#NotificationPagination .page-link', function (e) {
      e.preventDefault();
      var page = $(this).data('page');
      if (page && page !== _notificationState.currentPage) {
        _notificationState.currentPage = page;
        loadNotifications();
      }
    });
  }

  function loadNotifications() {
    var input = {
      state: _notificationState.stateFilter,
      skipCount: (_notificationState.currentPage - 1) * _notificationState.pageSize,
      maxResultCount: _notificationState.pageSize
    };

    abp.ui.setBusy($('#notification-tab-pane'));

    _notificationService.getUserNotifications(input)
      .done(function (result) {
        console.log('Danh sách thông báo:', result);
        _notificationState.totalCount = result.totalCount;
        _notificationState.isLoaded = true;
        renderNotifications(result.items);
        renderNotificationPagination();
        abp.ui.clearBusy($('#notification-tab-pane'));
      })
      .fail(function (error) {
        console.error('Lỗi khi tải thông báo:', error);
        abp.notify.error('Không thể tải danh sách thông báo!');
        abp.ui.clearBusy($('#notification-tab-pane'));
      });
  }

  function renderNotifications(notifications) {
    var $container = $('#notification-tab-pane');
    var $emptyContainer = $container.find('.empty-notifications-container');
    var $notificationsList = $container.find('.notifications-list-container');

    if (!notifications || notifications.length === 0) {
      $emptyContainer.show();
      $notificationsList.hide().empty();
      $('.notification-pagination').hide();
      return;
    }

    $emptyContainer.hide();
    $('.notification-pagination').show();

    var html = '';
    notifications.forEach(function (notification) {
      html += createNotificationItem(notification);
    });

    $notificationsList.html(html).show();
  }

  function createNotificationItem(userNotification) {
    var notification = userNotification.notification;
    var isUnread = userNotification.state === 0; // 0 = Unread, 1 = Read
    var unreadClass = isUnread ? 'unread' : '';
    
    // Get notification data
    var data = notification.data || {};
    var properties = data.properties || {};
    var message = properties.Message || properties.message || 'Thông báo mới';
    var notificationName = notification.notificationName || '';
    
    // Determine icon and color based on severity or notification type
    var iconClass = getNotificationIconClass(notification.severity, notificationName);
    var iconColorClass = getNotificationIconColorClass(notification.severity);
    
    // Format time
    var creationTime = moment(notification.creationTime).format('DD/MM/YYYY HH:mm');
    var timeAgo = moment(notification.creationTime).fromNow();
    
    // Get URL if available
    var url = properties.Url || properties.url || '';
    
    // Action buttons
    var markAsReadBtn = isUnread ? 
      `<button class="btn btn-outline-primary btn-sm mark-as-read-btn" data-notification-id="${userNotification.id}">
        <i class="fas fa-check"></i> Đánh dấu đã đọc
      </button>` : '';
    
    var viewDetailBtn = url ? 
      `<a href="${url}" class="btn btn-outline-info btn-sm">
        <i class="fas fa-eye"></i> Xem chi tiết
      </a>` : '';

    return `
      <div class="notification-item d-flex align-items-start ${unreadClass}" data-notification-id="${userNotification.id}">
        <div class="notification-icon ${iconColorClass}">
          <i class="${iconClass}"></i>
        </div>
        <div class="notification-content flex-grow-1">
          <div class="notification-title">${getNotificationTitle(notificationName)}</div>
          <div class="notification-message">${message}</div>
          <div class="notification-time">
            <i class="fas fa-clock"></i> ${timeAgo} (${creationTime})
          </div>
        </div>
        <div class="notification-actions">
          ${viewDetailBtn}
          ${markAsReadBtn}
          <button class="btn btn-outline-danger btn-sm delete-notification-btn" data-notification-id="${userNotification.id}">
            <i class="fas fa-trash"></i>
          </button>
        </div>
      </div>
    `;
  }

  function getNotificationTitle(notificationName) {
    var titles = {
      'App.NewOrder': 'Đơn hàng mới',
      'App.OrderStatusChanged': 'Cập nhật đơn hàng',
      'App.OrderApproved': 'Đơn hàng đã duyệt',
      'App.OrderRejected': 'Đơn hàng bị từ chối',
      'App.OrderCompleted': 'Đơn hàng hoàn thành',
      'App.NewProductComment': 'Bình luận mới',
      'App.CommentReply': 'Phản hồi bình luận',
      'App.LowStock': 'Cảnh báo tồn kho',
      'App.NewProduct': 'Sản phẩm mới'
    };
    return titles[notificationName] || 'Thông báo';
  }

  function getNotificationIconClass(severity, notificationName) {
    // First check by notification name
    if (notificationName.includes('Order')) return 'fas fa-shopping-cart';
    if (notificationName.includes('Comment')) return 'fas fa-comment';
    if (notificationName.includes('Stock')) return 'fas fa-warehouse';
    if (notificationName.includes('Product')) return 'fas fa-box';
    
    // Then by severity
    switch (severity) {
      case 0: return 'fas fa-info-circle'; // Info
      case 1: return 'fas fa-check-circle'; // Success
      case 2: return 'fas fa-exclamation-triangle'; // Warn
      case 3: return 'fas fa-times-circle'; // Error
      case 4: return 'fas fa-skull-crossbones'; // Fatal
      default: return 'fas fa-bell';
    }
  }

  function getNotificationIconColorClass(severity) {
    switch (severity) {
      case 0: return 'info'; // Info
      case 1: return 'success'; // Success
      case 2: return 'warning'; // Warn
      case 3: return 'error'; // Error
      case 4: return 'error'; // Fatal
      default: return 'info';
    }
  }

  function renderNotificationPagination() {
    var totalPages = Math.ceil(_notificationState.totalCount / _notificationState.pageSize);
    var currentPage = _notificationState.currentPage;
    
    if (totalPages <= 1) {
      $('#NotificationPagination').html('');
      return;
    }

    var html = '';
    
    // Previous button
    html += `
      <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage - 1}">
          <i class="fas fa-chevron-left"></i>
        </a>
      </li>
    `;
    
    // Page numbers
    var startPage = Math.max(1, currentPage - 2);
    var endPage = Math.min(totalPages, currentPage + 2);
    
    if (startPage > 1) {
      html += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
      if (startPage > 2) {
        html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
      }
    }
    
    for (var i = startPage; i <= endPage; i++) {
      html += `
        <li class="page-item ${i === currentPage ? 'active' : ''}">
          <a class="page-link" href="#" data-page="${i}">${i}</a>
        </li>
      `;
    }
    
    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
      }
      html += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
    }
    
    // Next button
    html += `
      <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage + 1}">
          <i class="fas fa-chevron-right"></i>
        </a>
      </li>
    `;
    
    $('#NotificationPagination').html(html);
  }

  function markNotificationAsRead(notificationId) {
    _notificationService.setNotificationAsRead({ id: notificationId })
      .done(function (result) {
        if (result.success) {
          // Update UI
          var $item = $(`.notification-item[data-notification-id="${notificationId}"]`);
          $item.removeClass('unread');
          $item.find('.mark-as-read-btn').remove();
          abp.notify.success('Đã đánh dấu đã đọc');
        }
      })
      .fail(function (error) {
        console.error('Lỗi khi đánh dấu đã đọc:', error);
        abp.notify.error('Không thể đánh dấu đã đọc!');
      });
  }

  function markAllNotificationsAsRead() {
    abp.message.confirm(
      'Bạn có chắc chắn muốn đánh dấu tất cả thông báo là đã đọc?',
      'Xác nhận',
      function (isConfirmed) {
        if (isConfirmed) {
          _notificationService.setAllNotificationsAsRead()
            .done(function () {
              abp.notify.success('Đã đánh dấu tất cả đã đọc');
              loadNotifications();
            })
            .fail(function (error) {
              console.error('Lỗi:', error);
              abp.notify.error('Không thể đánh dấu tất cả đã đọc!');
            });
        }
      }
    );
  }

  function deleteNotification(notificationId) {
    abp.message.confirm(
      'Bạn có chắc chắn muốn xóa thông báo này?',
      'Xác nhận xóa',
      function (isConfirmed) {
        if (isConfirmed) {
          _notificationService.deleteNotification({ id: notificationId })
            .done(function () {
              abp.notify.success('Đã xóa thông báo');
              loadNotifications();
            })
            .fail(function (error) {
              console.error('Lỗi khi xóa thông báo:', error);
              abp.notify.error('Không thể xóa thông báo!');
            });
        }
      }
    );
  }

})();