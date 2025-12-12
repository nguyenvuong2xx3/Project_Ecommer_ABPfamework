(function ($) {
	$(function () {

		var _permissions = {
			dashboard: abp.auth.hasPermission('Pages.Dashboard'),
		};

		// Back to my account
		$('#UserProfileBackToMyAccountButton').click(function (e) {
			e.preventDefault();
			abp.ajax({
				url: abp.appPath + 'Account/BackToImpersonator',
				success: function () {
					if (!app.supportsTenancyNameInUrl) {
						abp.multiTenancy.setTenantIdCookie(abp.session.impersonatorTenantId);
					}
				},
			});
		});

		// My settings
		var mySettingsModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/MySettingsModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_MySettingsModal.js',
			modalClass: 'MySettingsModal',
		});

		$('#UserProfileMySettingsLink').click(function (e) {
			e.preventDefault();
			mySettingsModal.open();
		});

		$('#UserDownloadCollectedDataLink').click(function (e) {
			e.preventDefault();
			abp.services.app.profile.prepareCollectedData().done(function () {
				abp.message.success(app.localize('GdprDataPrepareStartedNotification'));
			});
		});

		// Change password
		var changePasswordModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/ChangePasswordModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_ChangePasswordModal.js',
			modalClass: 'ChangePasswordModal',
		});

		$('#UserProfileChangePasswordLink').click(function (e) {
			e.preventDefault();
			changePasswordModal.open();
		});

		// Change profile picture
		var changeProfilePictureModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/ChangePictureModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_ChangePictureModal.js',
			modalClass: 'ChangeProfilePictureModal',
		});

		$('#UserProfileChangePictureLink').click(function (e) {
			e.preventDefault();
			changeProfilePictureModal.open();
		});

		// Manage linked accounts
		var _userLinkService = abp.services.app.userLink;

		var manageLinkedAccountsModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/LinkedAccountsModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_LinkedAccountsModal.js',
			modalClass: 'LinkedAccountsModal',
		});

		$('.manage-linked-accounts-link').click(function (e) {
			e.preventDefault();
			manageLinkedAccountsModal.open();
		});

		// Notifications
		var _appUserNotificationHelper = new app.UserNotificationHelper();
		var _cacheService = abp.services.app.caching;
		var _notificationService = abp.services.app.notifi;

		function shouldUserUpdateApp() {
			_notificationService
				.shouldUserUpdateApp().done(result => {
					if (result) {
						abp.message.confirm(
							null,
							app.localize('NewVersionAvailableNotification'),
							function (isConfirmed) {
								if (isConfirmed) {
									abp.ajax({
										url: '/BrowserCacheCleaner/Clear'
									}).done(result => {
										if (result) {
											window.location.reload();
										}
									})
								}
							}
						);
					}
				})
		}

		function renderNotificationTemplate(data) {
			var hasUnread = data.unreadCount && data.unreadCount > 0;

			// Xác định URL theo quyền
			var viewAllUrl = _permissions.dashboard ? "/Notifications" : "/UserProfile";

			var badgeHtml = hasUnread
				? `<span class="position-absolute badge badge-pill badge-danger" style="top: -5px; right: -5px; font-size: 0.6rem; min-width: 18px; height: 18px; line-height: 1.2;">${data.unreadCount}</span>`
				: '';

			var notificationsHtml = '';
			if (data.notifications && data.notifications.length > 0) {
				data.notifications.forEach(function (notification) {
					var clickableClass = notification.url ? 'user-notification-item-clickable' : '';
					var cursorStyle = notification.url ? 'style="cursor: pointer;"' : '';

					notificationsHtml += `
        <div class="notification-item p-2 border-bottom" ${cursorStyle}>
            <div class="d-flex align-items-start ${clickableClass}" 
                 data-url="${notification.url || '#'}" 
                 data-notification-id="${notification.userNotificationId || notification.id}">
                <div class="flex-shrink-0 mt-1">
                    <i class="material-symbols-outlined text-primary" style="font-size: 1rem;">notifications</i>
                </div>
                <div class="flex-grow-1 ml-2" style="font-size: 0.875rem;">
                    <p class="mb-1 text-break" style="font-size: 0.875rem; line-height: 1.3;">${escapeHtml(notification.text || 'Không có nội dung')}</p>
                    <small class="text-muted" style="font-size: 0.75rem;">${escapeHtml(notification.timeAgo || 'Vừa xong')}</small>
                </div>
                ${notification.isUnread ? '<div class="flex-shrink-0"><span class="badge badge-primary rounded-circle" style="width: 6px; height: 6px;"></span></div>' : ''}
            </div>
        </div>`;
				});
			} else {
				notificationsHtml = `
    <div class="text-center p-3 text-muted">
        <i class="material-symbols-outlined mb-2" style="font-size: 2rem;">notifications_off</i>
        <p class="mb-0" style="font-size: 0.875rem;">Không có thông báo</p>
    </div>`;
			}

			return `
<div class="dropdown position-relative">
    <button class="btn btn-light border-0 position-relative" type="button" 
            data-toggle="dropdown" aria-expanded="false"
            style="background: transparent !important;">
        <span class="material-symbols-outlined" style="font-size: 1.5rem;">notifications</span>
        ${badgeHtml}
    </button>

    <div class="dropdown-menu dropdown-menu-right p-0 border-0 shadow notification-dropdown" 
         style="position: absolute !important; width: 320px; max-height: 400px; overflow: hidden; top: 100%; right: 0; left: auto; z-index: 1070 !important; margin-top: 5px; transform: translateX(10px);">

        <div class="dropdown-arrow" style="position: absolute; top: -6px; right: 12px; width: 0; height: 0; border-left: 6px solid transparent; border-right: 6px solid transparent; border-bottom: 6px solid white; z-index: 1071;"></div>
        <div class="dropdown-arrow-border" style="position: absolute; top: -7px; right: 11px; width: 0; height: 0; border-left: 7px solid transparent; border-right: 7px solid transparent; border-bottom: 7px solid #dee2e6; z-index: 1070;"></div>
        
        <div class="d-flex justify-content-between align-items-center p-2 border-bottom bg-light">
            <h6 class="mb-0 font-weight-bold text-dark" style="font-size: 0.9rem;">Thông báo</h6>
            <div>
                ${data.unreadCount > 0 ?
					`<button class="btn btn-sm btn-outline-primary mr-1" id="btnSetAllNotificationsAsRead" style="font-size: 0.75rem; padding: 0.25rem 0.5rem;">Đánh dấu đã đọc</button>`
					: ''}
                <a href="${viewAllUrl}" class="btn btn-sm btn-link text-decoration-none" style="font-size: 0.75rem; padding: 0.25rem 0.5rem;">Xem tất cả</a>
            </div>
        </div>

        <div class="notification-list" style="max-height: 280px; overflow-y: auto;">
            ${notificationsHtml}
        </div>

        ${data.notifications && data.notifications.length > 0 ? `
        <div class="border-top p-1 text-center bg-light">
            <a href="${viewAllUrl}" class="text-primary text-decoration-none font-weight-medium" style="font-size: 0.8rem;">
                Xem tất cả thông báo
            </a>
        </div>` : ''}
    </div>
</div>`;
		}

		//function bindNotificationEvents() {
		//	shouldUserUpdateApp();

		//	// Sự kiện cho nút "Đánh dấu đã đọc"
		//	$(document).off('click', '#btnSetAllNotificationsAsRead').on('click', '#btnSetAllNotificationsAsRead', function (e) {
		//		e.preventDefault();
		//		e.stopPropagation();

		//		_appUserNotificationHelper.setAllAsRead(function () {
		//			loadNotifications();
		//		});
		//	});

		//	// Sự kiện cho từng thông báo
		//	$(document).off('click', '.user-notification-item-clickable').on('click', '.user-notification-item-clickable', function (e) {
		//		e.preventDefault();
		//		e.stopPropagation();

		//		var $this = $(this);
		//		var notificationId = $this.attr('data-notification-id');
		//		var url = $this.attr('data-url');

		//		if (notificationId) {
		//			_appUserNotificationHelper.setAsRead(notificationId, function () {
		//				loadNotifications();
		//			});
		//		}

		//		if (url && url !== '#') {
		//			setTimeout(function () {
		//				document.location.href = url;
		//			}, 300);
		//		}
		//	});

		//	$('#openNotificationSettingsModalLink').click(function (e) {
		//		e.preventDefault();
		//		e.stopPropagation();
		//		_appUserNotificationHelper.openSettingsModal();
		//	});
		//}

		// Helper function to escape HTML
		function escapeHtml(unsafe) {
			if (!unsafe) return '';
			return unsafe
				.toString()
				.replace(/&/g, "&amp;")
				.replace(/</g, "&lt;")
				.replace(/>/g, "&gt;")
				.replace(/"/g, "&quot;")
				.replace(/'/g, "&#039;");
		}

		function bindNotificationEvents() {
			shouldUserUpdateApp();

			// Sự kiện cho nút "Đánh dấu đã đọc"
			$(document).on('click', '#btnSetAllNotificationsAsRead', function (e) {
				e.preventDefault();
				e.stopPropagation();

				_appUserNotificationHelper.setAllAsRead(function () {
					loadNotifications();
				});
			});

			// Sự kiện cho từng thông báo
			$(document).on('click', '.user-notification-item-clickable', function (e) {
				e.preventDefault();
				e.stopPropagation();

				var $this = $(this);
				var notificationId = $this.attr('data-notification-id');
				var url = $this.attr('data-url');

				if (notificationId) {
					_appUserNotificationHelper.setAsRead(notificationId, function () {
						loadNotifications();
					});
				}

				if (url && url !== '#') {
					setTimeout(function () {
						document.location.href = url;
					}, 300);
				}
			});

			$('#openNotificationSettingsModalLink').click(function (e) {
				e.preventDefault();
				e.stopPropagation();
				_appUserNotificationHelper.openSettingsModal();
			});
		}

		function loadNotifications() {
			$('#toast-container').remove();
			// Kiểm tra nếu user chưa đăng nhập thì không làm gì cả
			if (!abp.session.userId) {
				// Hoặc điều kiện kiểm tra khác tùy vào cách bạn lưu trạng thái đăng nhập
				console.log('User is not logged in, skip loading notifications.');
				return;
			}

			_notificationService
				.getUserNotifications({
					maxResultCount: 3,
				})
				.done(function (result) {
					result.notifications = [];
					result.unreadMessageExists = result.unreadCount > 0;

					$.each(result.items, function (index, item) {
						// Format dữ liệu thủ công nếu helper không hoạt động
						var formattedItem = formatNotificationItem(item);
						result.notifications.push(formattedItem);
					});

					var $li = $('#header_notification_bar');
					var rendered = renderNotificationTemplate(result);
					$li.html(rendered);

					bindNotificationEvents();
				});
		}

		// Hàm format thay thế
		function formatNotificationItem(item) {
			var notification = item.notification;
			var data = notification.data || {};
			var properties = data.properties || {};

			return {
				userNotificationId: item.id, // ID của user notification
				text: properties.Message || 'Không có nội dung', // Lấy từ properties.Message
				timeAgo: formatTimeAgo(notification.creationTime), // Định dạng thời gian
				isUnread: item.state === 0, // state = 0 là chưa đọc
				url: generateNotificationUrl(properties) // Tạo URL dựa trên loại thông báo
			};
		}

		// Hàm định dạng thời gian
		function formatTimeAgo(creationTime) {
			if (!creationTime) return 'Vừa xong';

			var created = new Date(creationTime);
			var now = new Date();
			var diffMs = now - created;
			var diffMins = Math.floor(diffMs / 60000);
			var diffHours = Math.floor(diffMs / 3600000);
			var diffDays = Math.floor(diffMs / 86400000);

			if (diffMins < 1) return 'Vừa xong';
			if (diffMins < 60) return diffMins + ' phút trước';
			if (diffHours < 24) return diffHours + ' giờ trước';
			if (diffDays < 7) return diffDays + ' ngày trước';

			return created.toLocaleDateString('vi-VN');
		}

		// Hàm tạo URL cho thông báo
		function generateNotificationUrl(properties) {
			var type = properties.Type;
			var code = properties.Code;

			if (type === 'NewOrder' && code) {
				return '/App/Orders?code=' + encodeURIComponent(code);
			}

			return null; // Không có URL
		}

		abp.event.on('abp.notifications.received', function (userNotification) {
			_appUserNotificationHelper.show(userNotification);
			loadNotifications();
		});

		abp.event.on('app.notifications.refresh', function () {
			loadNotifications();
		});

		abp.event.on('app.notifications.read', function (userNotificationId) {
			loadNotifications();
		});

		// Chat
		//abp.event.on('app.chat.unreadMessageCountChanged', function (messageCount) {
		//	$('#chatIconUnRead .unread-chat-message-count').text(messageCount);

		//	if (messageCount) {
		//		$('#chatIconUnRead').removeClass('d-none');
		//		$('#chatIcon').addClass('d-none');
		//	} else {
		//		$('#chatIconUnRead').addClass('d-none');
		//		$('#chatIcon').removeClass('d-none');
		//	}
		//});

		// User Delegation
		var userDelegationsModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/UserDelegationsModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_UserDelegationsModal.js',
			modalClass: 'UserDelegationsModal',
		});

		$('#ManageUserDelegations').click(function (e) {
			e.preventDefault();
			userDelegationsModal.open();
		});

		$('#ActiveUserDelegationsCombobox').click(function () {
			var $activeUserDelegationsCombobox = $(this);
			var userDelegationId = parseInt($activeUserDelegationsCombobox.val());
			if (userDelegationId <= 0 || !userDelegationId) {
				return;
			}

			var username = $activeUserDelegationsCombobox.children('option:selected').attr('data-username');

			abp.message.confirm(
				app.localize('SwitchToDelegatedUserWarningMessage', username),
				app.localize('AreYouSure'),
				function (isConfirmed) {
					if (isConfirmed) {
						$activeUserDelegationsCombobox.attr('data-value', userDelegationId);
						abp.ajax({
							url: abp.appPath + 'Account/DelegatedImpersonate',
							data: JSON.stringify({
								userDelegationId: userDelegationId,
							}),
						});
					} else {
						$activeUserDelegationsCombobox.val($activeUserDelegationsCombobox.attr('data-value'));
						return false;
					}
				}
			);
		});

		// Init
		function init() {
			loadNotifications();
		}

		init();
	});
})(jQuery);