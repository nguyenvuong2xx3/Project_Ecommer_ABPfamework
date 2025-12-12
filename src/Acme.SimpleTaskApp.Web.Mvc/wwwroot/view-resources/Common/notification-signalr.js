var abp = abp || {};
(function () {
    // SignalR Notification Manager
    abp.signalr = abp.signalr || {};
    abp.signalr.hubs = abp.signalr.hubs || {};

    var notificationHub = null;
    var isConnected = false;

    // Initialize notification hub
    abp.signalr.initNotificationHub = function () {
        // Kiểm tra nếu đã kết nối
        if (isConnected) {
            console.log('Notification hub đã kết nối');
            return;
        }

        // Tạo connection
        notificationHub = new signalR.HubConnectionBuilder()
            .withUrl(abp.appPath + 'signalr-notification', {
                accessTokenFactory: function () {
                    // Lấy token nếu dùng JWT authentication
                    return abp.auth.getToken();
                }
            })
            .withAutomaticReconnect() // Tự động reconnect khi bị disconnect
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Xử lý khi nhận notification
        notificationHub.on('getNotification', function (notification) {
            console.log('Đã nhận notification:', notification);

            // Trigger event để các module khác có thể lắng nghe
            abp.event.trigger('abp.notifications.received', notification);

            // Hiển thị notification
            showNotification(notification);

            // Cập nhật badge count
            updateNotificationBadge();
        });

        // Xử lý khi kết nối thành công lại
        notificationHub.onreconnected(function () {
            console.log('Notification hub đã kết nối lại');
            isConnected = true;
            abp.event.trigger('abp.signalr.connected');
        });

        // Xử lý khi bị disconnect
        notificationHub.onclose(function () {
            console.log('Notification hub đã ngắt kết nối');
            isConnected = false;
            abp.event.trigger('abp.signalr.disconnected');

            // Thử kết nối lại sau 5 giây
            setTimeout(function () {
                startConnection();
            }, 5000);
        });

        // Start connection
        startConnection();
    };

    // Start SignalR connection
    function startConnection() {
        notificationHub.start()
            .then(function () {
                console.log('Notification hub đã kết nối thành công');
                isConnected = true;
                abp.event.trigger('abp.signalr.connected');
            })
            .catch(function (err) {
                console.error('Lỗi khi kết nối tới notification hub:', err);
                isConnected = false;

                // Thử kết nối lại sau 5 giây
                setTimeout(function () {
                    startConnection();
                }, 5000);
            });
    }

    // Hiển thị notification
    function showNotification(notification) {
        var severity = notification.severity || 0;
        var title = getNotificationTitle(notification.notificationName);
        var message = getNotificationMessage(notification);

        // Sử dụng ABP notify
        switch (severity) {
            case 0: // Info
                abp.notify.info(message, title);
                break;
            case 1: // Success
                abp.notify.success(message, title);
                break;
            case 2: // Warn
                abp.notify.warn(message, title);
                break;
            case 3: // Error
                abp.notify.error(message, title);
                break;
            case 4: // Fatal
                abp.notify.error(message, title);
                break;
            default:
                abp.notify.info(message, title);
        }

        // Phát âm thanh thông báo (tùy chọn)
        playNotificationSound();
    }

    // Lấy title của notification
    function getNotificationTitle(notificationName) {
        // Map notification names to titles
        var titleMap = {
            'App.NewOrder': 'Đơn hàng mới',
            'App.OrderStatusChanged': 'Trạng thái đơn hàng',
            'App.LowStock': 'Cảnh báo tồn kho',
            'App.OrderApproved': 'Đơn hàng đã được duyệt',
            'App.OrderRejected': 'Đơn hàng bị từ chối',
            'App.OrderCompleted': 'Đơn hàng hoàn thành'
        };

        return titleMap[notificationName] || 'Thông báo';
    }

    // Lấy message từ notification data
    function getNotificationMessage(notification) {
        if (!notification.data) {
            return 'Bạn có một thông báo mới';
        }

        // Parse notification data
        var data = notification.data;

        // Nếu data là string, parse nó
        if (typeof data === 'string') {
            try {
                data = JSON.parse(data);
            } catch (e) {
                return data;
            }
        }

        // Trả về message từ data
        return data.message || data.Message || 'Bạn có một thông báo mới';
    }

    // Cập nhật badge count
    function updateNotificationBadge() {
        // Gọi API để lấy số lượng notification chưa đọc
        abp.services.app.notification.getUserNotifications({
            state: 0, // Unread
            maxResultCount: 1
        }).done(function (result) {
            var unreadCount = result.unreadCount || 0;

            // Cập nhật badge
            var $badge = $('.notification-badge');
            if (unreadCount > 0) {
                $badge.text(unreadCount).show();
            } else {
                $badge.hide();
            }

            // Trigger event
            abp.event.trigger('abp.notifications.badge.updated', unreadCount);
        });
    }

    // Phát âm thanh thông báo
    function playNotificationSound() {
        try {
            // Tạo audio element
            var audio = new Audio(abp.appPath + 'sounds/notification.mp3');
            audio.volume = 0.5;
            audio.play().catch(function(err) {
                console.log('Không thể phát âm thanh thông báo:', err);
            });
        } catch (e) {
            console.log('Âm thanh thông báo không khả dụng');
        }
    }

    // Public methods
    abp.signalr.getNotificationHub = function () {
        return notificationHub;
    };

    abp.signalr.isConnected = function () {
        return isConnected;
    };

    // Auto-initialize khi document ready
    $(function () {
        // Chỉ init nếu user đã login
        if (abp.session.userId) {
            abp.signalr.initNotificationHub();
        }
    });

})();
