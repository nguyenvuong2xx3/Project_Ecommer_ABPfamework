var abp = abp || {};
(function () {
    // SignalR Notification Manager
    abp.signalr = abp.signalr || {};
    abp.signalr.hubs = abp.signalr.hubs || {};

    var notificationHub = null;
    var isConnected = false;

    // Initialize notification hub
    abp.signalr.initNotificationHub = function () {
        // Ki?m tra n?u ?ã k?t n?i
        if (isConnected) {
            console.log('Notification hub already connected');
            return;
        }

        // T?o connection
        notificationHub = new signalR.HubConnectionBuilder()
            .withUrl(abp.appPath + 'signalr-notification', {
                accessTokenFactory: function () {
                    // L?y token n?u dùng JWT authentication
                    return abp.auth.getToken();
                }
            })
            .withAutomaticReconnect() // T? ??ng reconnect khi b? disconnect
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // X? lý khi nh?n notification
        notificationHub.on('getNotification', function (notification) {
            console.log('Received notification:', notification);
            
            // Trigger event ?? các module khác có th? listen
            abp.event.trigger('abp.notifications.received', notification);
            
            // Hi?n th? notification
            showNotification(notification);
            
            // C?p nh?t badge count
            updateNotificationBadge();
        });

        // X? lý khi k?t n?i thành công
        notificationHub.onreconnected(function () {
            console.log('Notification hub reconnected');
            isConnected = true;
            abp.event.trigger('abp.signalr.connected');
        });

        // X? lý khi b? disconnect
        notificationHub.onclose(function () {
            console.log('Notification hub disconnected');
            isConnected = false;
            abp.event.trigger('abp.signalr.disconnected');
            
            // Th? k?t n?i l?i sau 5 giây
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
                console.log('Notification hub connected successfully');
                isConnected = true;
                abp.event.trigger('abp.signalr.connected');
            })
            .catch(function (err) {
                console.error('Error connecting to notification hub:', err);
                isConnected = false;
                
                // Th? k?t n?i l?i sau 5 giây
                setTimeout(function () {
                    startConnection();
                }, 5000);
            });
    }

    // Hi?n th? notification
    function showNotification(notification) {
        var severity = notification.severity || 0;
        var title = getNotificationTitle(notification.notificationName);
        var message = getNotificationMessage(notification);

        // S? d?ng ABP notify
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

        // Phát âm thanh thông báo (tùy ch?n)
        playNotificationSound();
    }

    // L?y title c?a notification
    function getNotificationTitle(notificationName) {
        // Map notification names to titles
        var titleMap = {
            'App.NewOrder': '??n hàng m?i',
            'App.OrderStatusChanged': 'Tr?ng thái ??n hàng',
            'App.LowStock': 'C?nh báo t?n kho',
            'App.OrderApproved': '??n hàng ???c duy?t',
            'App.OrderRejected': '??n hàng b? t? ch?i',
            'App.OrderCompleted': '??n hàng hoàn thành'
        };

        return titleMap[notificationName] || 'Thông báo';
    }

    // L?y message t? notification data
    function getNotificationMessage(notification) {
        if (!notification.data) {
            return 'B?n có m?t thông báo m?i';
        }

        // Parse notification data
        var data = notification.data;
        
        // N?u data là string, parse nó
        if (typeof data === 'string') {
            try {
                data = JSON.parse(data);
            } catch (e) {
                return data;
            }
        }

        // Tr? v? message t? data
        return data.message || data.Message || 'B?n có m?t thông báo m?i';
    }

    // C?p nh?t badge count
    function updateNotificationBadge() {
        // G?i API ?? l?y s? l??ng notification ch?a ??c
        abp.services.app.notification.getUserNotifications({
            state: 0, // Unread
            maxResultCount: 1
        }).done(function (result) {
            var unreadCount = result.unreadCount || 0;
            
            // C?p nh?t badge
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
            // T?o audio element
            var audio = new Audio(abp.appPath + 'sounds/notification.mp3');
            audio.volume = 0.5;
            audio.play().catch(function(err) {
                console.log('Could not play notification sound:', err);
            });
        } catch (e) {
            console.log('Notification sound not available');
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
        // Ch? init n?u user ?ã login
        if (abp.session.userId) {
            abp.signalr.initNotificationHub();
        }
    });

})();
