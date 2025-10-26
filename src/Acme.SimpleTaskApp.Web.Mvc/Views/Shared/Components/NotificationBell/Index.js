(function () {
	var $notificationList = $('#notificationList');
	var $notificationBadge = $('.notification-badge');
	var $notificationContainer = $('#notification-container');

	// Load notifications khi click vào bell icon
	$('#notificationDropdown').on('click', function (e) {
		loadNotifications();
	});

	// Mark all as read
	$('#markAllAsRead').on('click', function (e) {
		e.preventDefault();
		e.stopPropagation();

		abp.services.app.notification.setAllNotificationsAsRead()
			.done(function () {
				loadNotifications();
				$notificationBadge.hide();
				abp.notify.success('?ã ?ánh d?u t?t c? là ?ã ??c');
			});
	});

	// Load notifications
	function loadNotifications() {
		$notificationList.html('<div class="text-center py-3"><i class="fas fa-spinner fa-spin"></i><p class="text-muted mb-0">?ang t?i...</p></div>');

		abp.services.app.notification.getUserNotifications({
			state: null, // L?y t?t c?
			maxResultCount: 10
		}).done(function (result) {
			renderNotifications(result.items);
			updateBadge(result.unreadCount);
		});
	}

	// Render notifications
	function renderNotifications(notifications) {
		if (!notifications || notifications.length === 0) {
			$notificationList.html('<div class="text-center py-3"><p class="text-muted mb-0">Không có thông báo</p></div>');
			return;
		}

		var html = '';
		notifications.forEach(function (notification) {
			html += renderNotificationItem(notification);
		});

		$notificationList.html(html);
	}

	// Render single notification
	function renderNotificationItem(notification) {
		var isUnread = notification.state === 0;
		var severity = notification.notification.severity || 0;
		var data = notification.notification.data || {};
		var message = data.message || data.Message || 'B?n có m?t thông báo m?i';
		var time = moment(notification.notification.creationTime).fromNow();

		var iconClass = 'info';
		var iconName = 'fa-info-circle';

		switch (severity) {
			case 1: // Success
				iconClass = 'success';
				iconName = 'fa-check-circle';
				break;
			case 2: // Warning
				iconClass = 'warning';
				iconName = 'fa-exclamation-triangle';
				break;
			case 3: // Error
			case 4: // Fatal
				iconClass = 'error';
				iconName = 'fa-times-circle';
				break;
		}

		return `
  <div class="notification-item ${isUnread ? 'unread' : ''}" data-id="${notification.id}">
    <div class="d-flex">
      <div class="notification-icon ${iconClass} mr-3">
        <i class="fas ${iconName}"></i>
      </div>
      <div class="flex-grow-1">
        <div class="notification-title">${getNotificationTitle(notification.notification.notificationName)}</div>
        <div class="notification-message">${message}</div>
        <div class="notification-time">${time}</div>
      </div>
      <div class="ml-2">
        <button class="btn btn-sm btn-link notification-delete" data-id="${notification.id}" onclick="deleteNotification('${notification.id}', event)">
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>
  </div>
  `;
	}

	// Get notification title
	function getNotificationTitle(notificationName) {
		var titleMap = {
			'App.NewOrder': '??n hàng m?i',
			'App.OrderStatusChanged': 'Tr?ng thái ??n hàng',
			'App.OrderApproved': '??n hàng ???c duy?t',
			'App.OrderRejected': '??n hàng b? t? ch?i',
			'App.OrderCompleted': '??n hàng hoàn thành',
			'App.LowStock': 'C?nh báo t?n kho',
			'App.NewProduct': 'S?n ph?m m?i'
		};

		return titleMap[notificationName] || 'Thông báo';
	}

	// Update badge
	function updateBadge(count) {
		if (count > 0) {
			$notificationBadge.text(count).show();
		} else {
			$notificationBadge.hide();
		}
	}

	// Mark notification as read when clicked
	$(document).on('click', '.notification-item', function () {
		var notificationId = $(this).data('id');
		var $item = $(this);

		if ($item.hasClass('unread')) {
			abp.services.app.notification.setNotificationAsRead(notificationId)
				.done(function () {
					$item.removeClass('unread');
					var currentCount = parseInt($notificationBadge.text()) || 0;
					updateBadge(Math.max(0, currentCount - 1));
				});
		}
	});

	// Delete notification
	window.deleteNotification = function (notificationId, event) {
		event.stopPropagation();

		abp.message.confirm(
			'B?n có ch?c ch?n mu?n xóa thông báo này?',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.services.app.notification.deleteNotification(notificationId)
						.done(function () {
							loadNotifications();
							abp.notify.success('?ã xóa thông báo');
						});
				}
			}
		);
	};

	// Listen for new notifications from SignalR
	abp.event.on('abp.notifications.received', function (notification) {
		// Reload notification list n?u dropdown ?ang m?
		if ($notificationContainer.hasClass('show')) {
			loadNotifications();
		} else {
			// Ch? c?p nh?t badge
			var currentCount = parseInt($notificationBadge.text()) || 0;
			updateBadge(currentCount + 1);
		}
	});

	// Initial load badge count
	abp.services.app.notification.getUserNotifications({
		state: 0, // Unread only
		maxResultCount: 1
	}).done(function (result) {
		updateBadge(result.unreadCount);
	});
})();
