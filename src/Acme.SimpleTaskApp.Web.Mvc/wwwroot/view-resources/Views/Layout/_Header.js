(function ($) {
	$(function () {
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

			var badgeHtml = hasUnread
				? `<span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style="font-size: 0.6rem; min-width: 18px; height: 18px; line-height: 1;">${data.unreadCount}</span>`
				: '';

			var notificationsHtml = '';
			if (data.notifications && data.notifications.length > 0) {
				data.notifications.forEach(function (notification) {
					var clickableClass = notification.url ? 'user-notification-item-clickable' : '';
					var cursorStyle = notification.url ? 'style="cursor: pointer;"' : '';

					notificationsHtml += `
                <div class="notification-item p-3 border-bottom" ${cursorStyle}>
                    <div class="d-flex align-items-start ${clickableClass}" 
                         data-url="${notification.url || '#'}" 
                         data-notification-id="${notification.userNotificationId}">
                        <div class="flex-shrink-0 mt-1">
                            <i class="material-symbols-outlined text-primary" style="font-size: 1.2rem;">notifications</i>
                        </div>
                        <div class="flex-grow-1 ms-3">
                            <p class="mb-1 text-break">${escapeHtml(notification.text)}</p>
                            <small class="text-muted">${escapeHtml(notification.timeAgo)}</small>
                        </div>
                        ${notification.isUnread ? '<div class="flex-shrink-0"><span class="badge bg-primary rounded-circle" style="width: 8px; height: 8px;"></span></div>' : ''}
                    </div>
                </div>`;
				});
			} else {
				notificationsHtml = `
            <div class="text-center p-4 text-muted">
                <i class="material-symbols-outlined mb-2" style="font-size: 3rem;">notifications_off</i>
                <p class="mb-0">Không có thông báo</p>
            </div>`;
			}

			return `
        <div class="dropdown position-relative">
            <button class="btn btn-light border-0 position-relative" type="button" 
                    data-bs-toggle="dropdown" aria-expanded="false"
                    style="background: transparent !important;">
                <span class="material-symbols-outlined" style="font-size: 1.5rem;">notifications</span>
                ${badgeHtml}
            </button>

            <div class="dropdown-menu dropdown-menu-end p-0 border-0 shadow" 
                 style="width: 380px; max-height: 500px; overflow: hidden; left: auto; right: 0; z-index: 1060;">
                <div class="d-flex justify-content-between align-items-center p-3 border-bottom bg-light">
                    <h6 class="mb-0 fw-bold text-dark">Thông báo</h6>
                    <div>
                        ${data.unreadCount > 0 ?
					`<button class="btn btn-sm btn-outline-primary me-2" id="btnSetAllNotificationsAsRead">Đánh dấu đã đọc</button>` :
					''}
                        <a href="/App/Notifications" class="btn btn-sm btn-link text-decoration-none">Xem tất cả</a>
                    </div>
                </div>

                <div class="notification-list" style="max-height: 350px; overflow-y: auto;">
                    ${notificationsHtml}
                </div>

                ${data.notifications && data.notifications.length > 0 ? `
                <div class="border-top p-2 text-center bg-light">
                    <a href="/App/Notifications" class="text-primary text-decoration-none fw-medium">
                        Xem tất cả thông báo
                    </a>
                </div>
                ` : ''}
            </div>
        </div>`;
		}

		function bindNotificationEvents() {
			shouldUserUpdateApp();

			// Sự kiện cho nút "Đánh dấu đã đọc"
			$(document).off('click', '#btnSetAllNotificationsAsRead').on('click', '#btnSetAllNotificationsAsRead', function (e) {
				e.preventDefault();
				e.stopPropagation();

				_appUserNotificationHelper.setAllAsRead(function () {
					loadNotifications();
				});
			});

			// Sự kiện cho từng thông báo
			$(document).off('click', '.user-notification-item-clickable').on('click', '.user-notification-item-clickable', function (e) {
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
			_notificationService
				.getUserNotifications({
					maxResultCount: 3,
				})
				.done(function (result) {
					result.notifications = [];
					result.unreadMessageExists = result.unreadCount > 0;

					$.each(result.items, function (index, item) {
						var formattedItem = _appUserNotificationHelper.format(item);
						result.notifications.push(formattedItem);
					});

					var $li = $('#header_notification_bar');
					var rendered = renderNotificationTemplate(result);
					$li.html(rendered);

					bindNotificationEvents();
				});
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
		abp.event.on('app.chat.unreadMessageCountChanged', function (messageCount) {
			$('#chatIconUnRead .unread-chat-message-count').text(messageCount);

			if (messageCount) {
				$('#chatIconUnRead').removeClass('d-none');
				$('#chatIcon').addClass('d-none');
			} else {
				$('#chatIconUnRead').addClass('d-none');
				$('#chatIcon').removeClass('d-none');
			}
		});

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