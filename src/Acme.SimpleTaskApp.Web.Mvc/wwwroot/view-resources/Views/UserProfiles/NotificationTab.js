(function () {
  var _notificationService = abp.services.app.notification;
  var _state = {
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    stateFilter: null,
    isLoaded: false
  };

  // --- HELPER FUNCTION: Lấy container đúng cho cả 2 trường hợp ---
  function getContainer() {
    // Ưu tiên tìm class chung (Admin view dùng cái này)
    var $container = $('.notification-page-container').first();
    // Nếu không thấy, tìm theo ID của tab (User/Customer view dùng cái này)
    if ($container.length === 0) {
      $container = $('#notification-tab-pane');
    }
    return $container;
  }

  $(document).ready(function () {
    // Kiểm tra xem có tab thông báo không (trang customer)
    if ($('#notification-tab').length > 0) {
      // Đây là trang customer, sử dụng sự kiện tab
      bindEvents();
    } else {
      // Đây là trang admin, load ngay
      initializeNotificationPage();
    }
  });

  function initializeNotificationPage() {
    bindEvents();
    loadNotifications();
  }

  function loadNotifications() {
    var input = {
      state: _state.stateFilter,
      skipCount: (_state.currentPage - 1) * _state.pageSize,
      maxResultCount: _state.pageSize
    };

    // Dùng hàm getContainer để lấy đúng vùng hiển thị
    var $container = getContainer();
    
    // Nếu không tìm thấy container nào thì return luôn để tránh lỗi
    if ($container.length === 0) return;

    abp.ui.setBusy($container);

    _notificationService.getUserNotifications(input)
      .done(function (result) {
        _state.totalCount = result.totalCount;
        _state.isLoaded = true;
        renderNotifications(result.items);
        renderPagination();
      })
      .fail(function (error) {
        console.error('Lỗi khi tải thông báo:', error);
        abp.notify.error('Không thể tải danh sách thông báo!');
      })
      .always(function () {
        // SỬA: Clear busy đúng container
        abp.ui.clearBusy(getContainer());
      });
  }

  function renderNotifications(notifications) {
    // SỬA: Dùng hàm getContainer thay vì hardcode ID #notification-tab-pane
    var $container = getContainer();
    
    var $emptyContainer = $container.find('.empty-notifications-container');
    var $notificationsList = $container.find('.notifications-list-container');

    // Ẩn/Hiện phân trang
    // Lưu ý: ID #NotificationPagination nằm ngoài container list một chút ở View Admin, 
    // nhưng selector này dùng ID toàn cục nên vẫn ổn.
    var $pagination = $('#NotificationPagination').closest('.notification-pagination');

    if (!notifications || notifications.length === 0) {
      $emptyContainer.show();
      $notificationsList.hide().empty();
      $pagination.hide();
      return;
    }

    $emptyContainer.hide();
    $pagination.show();

    var html = '';
    notifications.forEach(function (notification) {
      html += createNotificationItem(notification);
    });

    $notificationsList.html(html).show();
  }

  function createNotificationItem(userNotification) {
    var notification = userNotification.notification;
    var isUnread = userNotification.state === 0;
    var unreadClass = isUnread ? 'unread' : '';

    var data = notification.data || {};
    var properties = data.properties || {};
    // Fallback message an toàn hơn
    var message = properties.Message || properties.message || notification.data.message || 'Thông báo mới';
    var notificationName = notification.notificationName || '';

    var iconClass = getNotificationIconClass(notification.severity, notificationName);
    var iconColorClass = getNotificationIconColorClass(notification.severity);

    var creationTime = moment(notification.creationTime).format('DD/MM/YYYY HH:mm');
    var timeAgo = moment(notification.creationTime).fromNow();

    var url = properties.Url || properties.url || '';

    var markAsReadBtn = isUnread ?
      '<button class="btn btn-outline-primary btn-sm mark-as-read-btn" data-notification-id="' + userNotification.id + '">' +
      '<i class="fas fa-check"></i> Đánh dấu đã đọc' +
      '</button>' : '';

    var viewDetailBtn = url ?
      '<a href="' + url + '" class="btn btn-outline-info btn-sm">' +
      '<i class="fas fa-eye"></i> Xem chi tiết' +
      '</a>' : '';

    return '<div class="notification-item d-flex align-items-start ' + unreadClass + '" data-notification-id="' + userNotification.id + '">' +
      '<div class="notification-icon ' + iconColorClass + '">' +
      '<i class="' + iconClass + '"></i>' +
      '</div>' +
      '<div class="notification-content flex-grow-1">' +
      '<div class="notification-title">' + getNotificationTitle(notificationName) + '</div>' +
      '<div class="notification-message">' + message + '</div>' +
      '<div class="notification-time">' +
      '<i class="fas fa-clock"></i> ' + timeAgo + ' (' + creationTime + ')' +
      '</div>' +
      '</div>' +
      '<div class="notification-actions">' +
      viewDetailBtn +
      markAsReadBtn +
      '<button class="btn btn-outline-danger btn-sm delete-notification-btn" data-notification-id="' + userNotification.id + '">' +
      '<i class="fas fa-trash"></i>' +
      '</button>' +
      '</div>' +
      '</div>';
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
    // Kiểm tra null/undefined cho notificationName
    if (!notificationName) return 'fas fa-bell';
    
    if (notificationName.includes('Order')) return 'fas fa-shopping-cart';
    if (notificationName.includes('Comment')) return 'fas fa-comment';
    if (notificationName.includes('Stock')) return 'fas fa-warehouse';
    if (notificationName.includes('Product')) return 'fas fa-box';

    switch (severity) {
      case 0: return 'fas fa-info-circle';
      case 1: return 'fas fa-check-circle';
      case 2: return 'fas fa-exclamation-triangle';
      case 3: return 'fas fa-times-circle';
      case 4: return 'fas fa-skull-crossbones';
      default: return 'fas fa-bell';
    }
  }

  function getNotificationIconColorClass(severity) {
    switch (severity) {
      case 0: return 'info';
      case 1: return 'success';
      case 2: return 'warning';
      case 3: return 'error';
      case 4: return 'error';
      default: return 'info';
    }
  }

  function renderPagination() {
    var totalPages = Math.ceil(_state.totalCount / _state.pageSize);
    var currentPage = _state.currentPage;
    var $paginationContainer = $('#NotificationPagination');

    if (totalPages <= 1) {
      $paginationContainer.html('');
      return;
    }

    var html = '';

    // Previous button
    html += '<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '">' +
      '<a class="page-link" href="#" data-page="' + (currentPage - 1) + '">' +
      '<i class="fas fa-chevron-left"></i>' +
      '</a>' +
      '</li>';

    // Page numbers
    var startPage = Math.max(1, currentPage - 2);
    var endPage = Math.min(totalPages, currentPage + 2);

    if (startPage > 1) {
      html += '<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>';
      if (startPage > 2) {
        html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
      }
    }

    for (var i = startPage; i <= endPage; i++) {
      html += '<li class="page-item ' + (i === currentPage ? 'active' : '') + '">' +
        '<a class="page-link" href="#" data-page="' + i + '">' + i + '</a>' +
        '</li>';
    }

    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
      }
      html += '<li class="page-item"><a class="page-link" href="#" data-page="' + totalPages + '">' + totalPages + '</a></li>';
    }

    // Next button
    html += '<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '">' +
      '<a class="page-link" href="#" data-page="' + (currentPage + 1) + '">' +
      '<i class="fas fa-chevron-right"></i>' +
      '</a>' +
      '</li>';

    $paginationContainer.html(html);
  }

  function markNotificationAsRead(notificationId) {
    _notificationService.setNotificationAsRead({ id: notificationId })
      .done(function (result) {
        // Không cần check result.success vì ABP throw error nếu fail, nhưng check cũng tốt
        var $item = $('.notification-item[data-notification-id="' + notificationId + '"]');
        $item.removeClass('unread');
        $item.find('.mark-as-read-btn').remove();
        abp.notify.success('Đã đánh dấu đã đọc');
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

  function bindEvents() {
    // Load notifications khi click vào tab (Chỉ dành cho trang User có tab)
    $('#notification-tab').on('click', function () {
      if (!_state.isLoaded) {
        loadNotifications();
      }
    });

    // Filter by state
    $('#NotificationStateFilter').on('change', function () {
      var value = $(this).val();
      _state.stateFilter = value === '' ? null : parseInt(value);
      _state.currentPage = 1;
      loadNotifications();
    });

    // Mark all as read
    $('#MarkAllAsRead').on('click', function () {
      markAllNotificationsAsRead();
    });

    // Mark as read (dùng delegation vì element sinh ra động)
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
      // Chuyển đổi sang số để so sánh chính xác
      page = parseInt(page);
      if (page && page !== _state.currentPage) {
        _state.currentPage = page;
        loadNotifications();
      }
    });
  }

})();