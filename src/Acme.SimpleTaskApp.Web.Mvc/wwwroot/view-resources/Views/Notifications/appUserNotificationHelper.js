var app = app || {};
(function ($) {
  app.UserNotificationHelper = (function () {
    return function () {

      /* 1. ĐĂNG KÝ FORMATTER CHO THÔNG BÁO MỚI ********/
      // Đây là phần bạn còn thiếu để hiển thị text đúng
      abp.notifications.messageFormatters['App.NewOrder'] = function (userNotification) {
        // Lấy dữ liệu từ property "Message" mà ta đã gửi từ Backend
        return userNotification.notification.data.properties['Message'];
      };

      abp.notifications.messageFormatters['Đơn hàng mới'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'];
      };

      var _notificationService = abp.services.app.notification;

      /* Converter functions ***************************************/
      function getUrl(userNotification) {
        switch (userNotification.notification.notificationName) {
          case 'App.NewUserRegistered':
            return '/AppAreaName/users?filterText=' + userNotification.notification.data.properties.emailAddress;

          // 2. Thêm URL điều hướng cho đơn hàng mới
          case 'App.NewOrder':
            // Giả sử bạn có trang chi tiết đơn hàng, thay đổi link này cho đúng
            // Ví dụ: /Admin/Orders/Detail?code=...
            return '/App/Orders/Detail?code=' + userNotification.notification.data.properties['Code'];

          case 'App.GdprDataPrepared':
            return (
              '/File/DownloadBinaryFile?id=' +
              userNotification.notification.data.properties.binaryObjectId +
              '&contentType=application/zip&fileName=collectedData.zip'
            );
        }
        return '';
      }

      /* PUBLIC functions ******************************************/
      var format = function (userNotification, truncateText) {
        var formatted = {
          userNotificationId: userNotification.id,
          // Dòng này sẽ gọi cái formatter ta vừa đăng ký ở trên
          text: abp.notifications.getFormattedMessageFromUserNotification(userNotification),
          time: moment(userNotification.notification.creationTime).format('YYYY-MM-DD HH:mm:ss'),
          icon: app.notification.getUiIconBySeverity(userNotification.notification.severity),
          state: abp.notifications.getUserNotificationStateAsString(userNotification.state),
          data: userNotification.notification.data,
          url: getUrl(userNotification),
          isUnread: userNotification.state === abp.notifications.userNotificationState.UNREAD,
          timeAgo: moment(userNotification.notification.creationTime).fromNow(),
          iconFontClass: app.notification.getIconFontClassBySeverity(userNotification.notification.severity),
        };

        if (truncateText || truncateText === undefined) {
          formatted.text = abp.utils.truncateStringWithPostfix(formatted.text, 50);
        }

        return formatted;
      };

      var show = function (userNotification) {
        //Application notification
        abp.notifications.showUiNotifyForUserNotification(userNotification, {
          onclick: function () {
            //Take action when user clicks to live toastr notification
            var url = getUrl(userNotification);
            if (url) {
              location.href = url;
            }
          },
        });

        //Desktop notification
        Push.create('SaaS', {
          body: format(userNotification).text,
          icon: abp.appPath + 'Common/Images/app-logo-small.svg',
          timeout: 6000,
          onClick: function () {
            window.focus();
            this.close();
          },
        });
      };

      var setAllAsRead = function (callback) {
        _notificationService.setAllNotificationsAsRead().done(function () {
          abp.event.trigger('app.notifications.refresh');
          callback && callback();
        });
      };

      var setAsRead = function (userNotificationId, callback) {
        _notificationService
          .setNotificationAsRead({
            id: userNotificationId,
          })
          .done(function (result) {
            abp.event.trigger('app.notifications.read', userNotificationId, result.success);
            callback && callback(userNotificationId);
          });
      };

      var openSettingsModal = function () {
        new app.ModalManager({
          viewUrl: abp.appPath + 'AppAreaName/Notifications/SettingsModal',
          scriptUrl: abp.appPath + 'view-resources/Areas/AppAreaName/Views/Notifications/_SettingsModal.js',
          modalClass: 'NotificationSettingsModal',
        }).open();
      };

      /* Expose public API *****************************************/

      return {
        format: format,
        show: show,
        setAllAsRead: setAllAsRead,
        setAsRead: setAsRead,
        openSettingsModal: openSettingsModal,
      };
    };
  })();
})(jQuery);
