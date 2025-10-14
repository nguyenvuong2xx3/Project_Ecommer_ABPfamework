(function () {
  $(function () {
    var _$notificationsTable = $('#NotificationsTable');
    var _notificationService = abp.services.app.notification;

    var _$targetValueFilterSelectionCombobox = $('#TargetValueFilterSelectionCombobox');

    var _appUserNotificationHelper = new app.UserNotificationHelper();

    var _selectedDateRangeNotification = {
      startDate: moment().startOf('day').subtract(7, 'days'),
      endDate: moment().endOf('day'),
    };

    $(document)
      .find('#ReceivedNotifications_StartEndRange')
      .daterangepicker(
        $.extend(true, app.createDateRangePickerOptions(), _selectedDateRangeNotification, {
          locale: {
            applyLabel: 'Áp dụng',
            cancelLabel: 'Hủy bỏ'
          }
        }),
        function (start, end) {
          _selectedDateRangeNotification.startDate = start.format('YYYY-MM-DDT00:00:00Z');
          _selectedDateRangeNotification.endDate = end.format('YYYY-MM-DDT23:59:59.999Z');

          getNotifications();
        }
      );


    var createNotificationReadButton = function ($td, record) {
      var $span = $('<span class="col-nowrap"/>');
      var $button = $('<button/>')
        .addClass('ps-0 border-0 bg-transparent lh-1 position-relative top-2')
        .attr('title', app.localize('SetAsRead'))
        .click(function (e) {
          e.preventDefault();
          setNotificationAsRead(record, function () {
            $button.find('i').removeClass('la-circle-o').addClass('la-check');
            $button.attr('disabled', 'disabled');
            $td.closest('tr').addClass('notification-read');
          });
        })
        .appendTo($span);

      var $buttonDelete = $('<button/>')
        .addClass('ps-0 border-0 bg-transparent lh-1 position-relative top-2')
        .attr('title', app.localize('Delete'))
        .click(function () {
          deleteNotification(record);
        })
        .appendTo($span);
      $('<i class="material-symbols-outlined fs-20 text-danger" >').text('delete').appendTo($buttonDelete);

      var $i = $('<i class="material-symbols-outlined fs-20 text-primary" >').appendTo($button);
      var notificationState = _appUserNotificationHelper.format(record).state;

      if (notificationState === 'READ') {
        $button.attr('disabled', 'disabled');
        $i.text('select_check_box');
      }

      if (notificationState === 'UNREAD') {
        $i.text('mark_email_unread');
      }

      $td.append($span);
      $td.addClass('text-nowrap');
    };

    function getNotificationTextBySeverity(severity) {
      switch (severity) {
        case abp.notifications.severity.SUCCESS:
          return app.localize('Success');
        case abp.notifications.severity.WARN:
          return app.localize('Warning');
        case abp.notifications.severity.ERROR:
          return app.localize('Error');
        case abp.notifications.severity.FATAL:
          return app.localize('Fatal');
        case abp.notifications.severity.INFO:
        default:
          return app.localize('Info');
      }
    }

    var dataTable = _$notificationsTable.DataTable({
      paging: true,
      serverSide: true,
      processing: true,
      listAction: {
        ajaxFunction: _notificationService.getUserNotifications,
        inputFilter: function () {
          return {
            state: _$targetValueFilterSelectionCombobox.val(),
            startDate: _selectedDateRangeNotification.startDate,
            endDate: _selectedDateRangeNotification.endDate,
          };
        },
      },
      columnDefs: [
        {
          className: 'dtr-control responsive',
          orderable: false,
          render: function () {
            return '';
          },
          targets: 0,
        },
        {
          targets: 1,
          responsivePriority: 0,
          data: null,
          orderable: false,
          defaultContent: '',
          width: '100px',
          createdCell: function (td, cellData, rowData, rowIndex, colIndex) {
            createNotificationReadButton($(td), rowData);
          },
        },
        {
          targets: 2,
          data: 'severity',
          orderable: false,
          render: function (severity, type, row, meta) {
            var icon = app.notification.getUiIconBySeverity(row.notification.severity);
            var iconFontClass = app.notification.getIconFontClassBySeverity(row.notification.severity);
            var $span = $('<span></span>');
            var $icon = $(
              `<i class="${icon} ${iconFontClass} " data-bs-toggle="tooltip" data-bs-placement="right" data-bs-container="body" data-bs-original-title="${getNotificationTextBySeverity(
                row.notification.severity
              )}"></i>`
            );
            $span.append($icon);

            return $span[0].outerHTML;
          },
        },
        {
          targets: 3,
          data: 'notification',
          responsivePriority: 0,
          orderable: false,
          render: function (notification, type, row, meta) {
            var $container;
            var formattedRecord = _appUserNotificationHelper.format(row, false);
            var rowClass = getRowClass(formattedRecord);

            if (formattedRecord.url && formattedRecord.url !== '#') {
              $container = $(
                '<a title="' +
                  formattedRecord.text +
                  '" href="' +
                  formattedRecord.url +
                  '" class="' +
                  rowClass +
                  '">' +
                  abp.utils.truncateStringWithPostfix(formattedRecord.text, 120) +
                  '</a>'
              );
            } else {
              $container = $(
                '<span title="' +
                  formattedRecord.text +
                  '" class="' +
                  rowClass +
                  '">' +
                  abp.utils.truncateStringWithPostfix(formattedRecord.text, 120) +
                  '</span>'
              );
            }

            return $container[0].outerHTML;
          },
        },
        {
          targets: 4,
          data: 'creationTime',
          orderable: false,
          render: function (creationTime, type, row, meta) {
            var formattedRecord = _appUserNotificationHelper.format(row);
            var rowClass = getRowClass(formattedRecord);
            var $container = $(
              '<span title="' +
                moment(row.notification.creationTime).format('llll') +
                '" class="' +
                rowClass +
                '">' +
                formattedRecord.timeAgo +
                '</span> &nbsp;'
            );
            return $container[0].outerHTML;
          },
        },
      ],
      initComplete: function (settings, json) {
        //KTApp.createInstances(); 
      },
    });

    function deleteNotification(notification) {
      abp.message.confirm(
        app.localize('NotificationDeleteWarningMessage', notification.notification.text),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _notificationService
              .deleteNotification({
                id: notification.id,
              })
              .done(function () {
                getNotifications();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        },
        { confirmButtonText: 'Xóa', confirmButtonColor: '#dc3545', cancelButtonText: 'Hủy' }
      );
    }

    function deleteNotifications() {
      abp.message.confirm(
        app.localize('NotificationDeleteWarningMessages'),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _notificationService
              .deleteAllUserNotifications({
                state: _$targetValueFilterSelectionCombobox.val(),
                startDate: _selectedDateRangeNotification.startDate,
                endDate: _selectedDateRangeNotification.endDate,
              })
              .done(function () {
                getNotifications();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        },
        { confirmButtonText: 'Xóa', confirmButtonColor: '#dc3545', cancelButtonText: 'Hủy' }
      );
    }

    function getRowClass(formattedRecord) {
      return formattedRecord.state === 'READ' ? 'notification-read text-muted' : '';
    }

    function getNotifications() {
      dataTable.ajax.reload();
    }

    function setNotificationAsRead(userNotification, callback) {
      _appUserNotificationHelper.setAsRead(userNotification.id, function () {
        getNotifications();
        if (callback) {
          callback();
        }
         abp.notify.success(app.localize('NotificationMarkedAsRead'));
      });
    }

    function setAllNotificationsAsRead() {
      _appUserNotificationHelper.setAllAsRead(function () {
        getNotifications();
      });
    }

    function openNotificationSettingsModal() {
      _appUserNotificationHelper.openSettingsModal();
    }

    _$targetValueFilterSelectionCombobox.change(function () {
      getNotifications();
    });

    $('#RefreshNotificationTableButton').click(function (e) {
      e.preventDefault();
    });

    $('#btnOpenNotificationSettingsModal').click(function (e) {
      openNotificationSettingsModal();
    });

    $('#btnSetAllNotificationsAsRead').click(function (e) {
      e.preventDefault();
      setAllNotificationsAsRead();
      getNotifications();
    });

    $('#DeleteAllNotificationsButton').click(function (e) {
      e.preventDefault();
      deleteNotifications();
    });
  });
})();
