var app = app || {};
(function ($) {
  app.UserNotificationHelper = (function () {
    return function () {

      //Order notifications
      abp.notifications.messageFormatters['App.NewOrder'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Có đơn hàng mới';
      };

      abp.notifications.messageFormatters['App.OrderStatusChanged'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Trạng thái đơn hàng đã thay đổi';
      };

      abp.notifications.messageFormatters['App.OrderApproved'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Đơn hàng đã được duyệt';
      };

      abp.notifications.messageFormatters['App.OrderRejected'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Đơn hàng đã bị từ chối';
      };

      abp.notifications.messageFormatters['App.OrderCompleted'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Đơn hàng đã hoàn thành';
      };

      //Comment notifications
      abp.notifications.messageFormatters['App.NewProductComment'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Có bình luận mới về sản phẩm';
      };

      abp.notifications.messageFormatters['App.CommentReply'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Có người trả lời bình luận của bạn';
      };

      abp.notifications.messageFormatters['App.LowStock'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Sản phẩm sắp hết hàng';
      };

      abp.notifications.messageFormatters['App.NewProduct'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Có sản phẩm mới';
      };

      // Legacy support
      abp.notifications.messageFormatters['Đơn hàng mới'] = function (userNotification) {
        return userNotification.notification.data.properties['Message'] || 
               'Có đơn hàng mới';
      };

      var _notificationService = abp.services.app.notification;

      /* Converter functions ***************************************/
      function getUrl(userNotification) {
        var data = userNotification.notification.data.properties;
        
        switch (userNotification.notification.notificationName) {
          case 'App.NewUserRegistered':
            return '/AppAreaName/users?filterText=' + data.emailAddress;

          // Order notifications
          case 'App.NewOrder':
          case 'App.OrderStatusChanged':
          case 'App.OrderApproved':
          case 'App.OrderRejected':
          case 'App.OrderCompleted':
            // Sử dụng URL từ backend hoặc fallback
            return data['Url'] || '/App/Orders/Detail?code=' + data['Code'];

          //Comment notifications
          case 'App.NewProductComment':
          case 'App.CommentReply':
            // Sử dụng URL từ backend
            return data['Url'] || '/HomeCustomer/DetailProductCustomer?id=' + data['ProductId'];

          case 'App.GdprDataPrepared':
            return (
              '/File/DownloadBinaryFile?id=' +
              data.binaryObjectId +
              '&contentType=application/zip&fileName=collectedData.zip'
            );
        }
        
        //Fallback: Nếu có URL trong data thì dùng
        return data['Url'] || '';
      }

      /* PUBLIC functions ******************************************/
      var format = function (userNotification, truncateText) {
        var formatted = {
          userNotificationId: userNotification.id,
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
          formatted.text = abp.utils.truncateStringWithPostfix(formatted.text, 100); //Tăng từ 50 lên 100 ký tự
        }

        return formatted;
      };

      var show = function (userNotification) {
        //Application notification (toast)
        //abp.notifications.showUiNotifyForUserNotification(userNotification, {
        //  onclick: function () {
        //    // Take action when user clicks to live toastr notification
        //    var url = getUrl(userNotification);
        //    if (url) {
        //      location.href = url;
        //    }
        //  },
        //});

        //Desktop notification (Push)
        Push.create('SimpleTaskApp', {
          body: format(userNotification).text,
          icon: abp.appPath + 'Common/Images/app-logo-small.svg',
          timeout: 6000,
          onClick: function () {
            window.focus();
            this.close();
            
            //Navigate to URL when clicking desktop notification
            var url = getUrl(userNotification);
            if (url) {
              location.href = url;
            }
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
