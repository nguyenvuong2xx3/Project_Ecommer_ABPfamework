(function ($) {
  var _orderService = abp.services.app.orders,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$table = $('#OrdersTable');



  var getFilter = function () {
    let dataFilter = {};

    //// Lấy giá trị từ ô tìm kiếm NameUser
    //dataFilter.userName = $('#NameUser').val();

    // Lấy giá trị từ dropdown PaymentMethod
    let paymentMethod = $('#PaymentMethod').val();
    dataFilter.paymentMethod = paymentMethod ? parseInt(paymentMethod) : null;

    // Lấy giá trị từ dropdown OrderStatus
    let status = $('#OrderStatus').val();
    dataFilter.status = status ? parseInt(status) : null;

    console.log(dataFilter);
    return dataFilter;
  };


  var _$ordersTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    processing: true,
    listAction: {
      ajaxFunction: _orderService.getAllOrderForUser,
      inputFilter: getFilter
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$ordersTable.draw(false)
      }
    ],
    columnDefs: [
      {
        targets: 0,
        data: 'userName'
      },
      {
        targets: 1,
        data: 'name'
      },
      {
        targets: 2,
        data: 'totalCount',
        render: function (data) {
          return Number(data).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
        }
      },
      {
        targets: 3,
        data: 'paymentMethod',
        render: function (data) {
          if (data == 0) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#4CAF50;color:white;">Thanh toán tiền mặt</span>';
          } else if (data == 1) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#2196F3;color:white;">Chuyển khoản</span>';
          } else {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#9E9E9E;color:white;">Không xác định</span>';
          }
        }
      },
      {
        targets: 4,
        data: 'creationTime',
      },
      {
        targets: 5,
        data: 'status',
        render: function (data) {
          if (data == 0) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#FFC107;color:white;">Đang chờ</span>';
          } else if (data == 1) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#2196F3;color:white;">Đang giao hàng</span>';
          } else if (data == 2) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#4CAF50;color:white;">Hoàn thành</span>';
          } else if (data == 3) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#F44336;color:white;">Từ chối</span>';
          } else if (data == 4) {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#FF9800;color:white;">Đã hủy</span>';
          } else {
            return '<span style="display:inline-block;padding:2px 8px;border-radius:5px;background-color:#9E9E9E;color:white;">Không xác định</span>';
          }
        }
      },
      {
        targets: 6,
        data: null,
        orderable: false,
        render: function (data, type, row) {
          var buttons = [];

          if (row.status === 0) { // Chờ duyệt
            buttons.push(
              `<button type="button" style="color: white;" class="btn btn-sm bg-danger cancel-order" data-order-id="${row.id}">`,
              `<i class="fas fa-times"></i> ${l('Hủy đơn')}`,
              '</button>'
            );
          }

          if (row.status === 4) { // Đã hủy → cho đặt lại
            buttons.push(
              `<button type="button" style="color: white;" class="btn btn-sm bg-success reorder-order" data-order-id="${row.id}">`,
              `<i class="fas fa-redo"></i> ${l('Đặt lại đơn')}`,
              '</button>'
            );
          }

          // Nút Detail luôn có
          buttons.push(
            `<button type="button" style="color: white;" class="btn btn-sm bg-info detail-order" data-order-id="${row.id}">`,
            `<i class="fas fa-eye"></i> ${l('Detail')}`,
            '</button>'
          );

          // Bọc trong div để căn giữa + khoảng cách
          return `
      <div style="display: flex; justify-content: center; gap: 8px; flex-wrap: wrap;">
        ${buttons.join('')}
      </div>
    `;
        }
      }

    ]
  });

  // Xem chi tiết đơn hàng
  $(document).on('click', '.detail-order', function () {
    var orderId = $(this).data('order-id');
    abp.ajax({
      url: abp.appPath + 'Orders/DetailOrder?orderId=' + orderId,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#OrderDetailModal .modal-content').html(content);
        $('#OrderDetailModal').modal('show');
      },
      error: function (e) {
        abp.notify.error('Could not load detail form');
      }
    });
  });

  $(document).on('click', '.cancel-order', function () {
    var orderId = $(this).data('order-id');
    _orderService.cancelOrder(orderId)
      .done(function () {
        abp.notify.success('Đã hủy đơn thành công!');
        _$ordersTable.ajax.reload();

      })
      .fail(function () {
        abp.notify.error('Hủy đơn thất bại!');
      });
  });

  $(document).on('click', '.reorder-order', function () {
    var orderId = $(this).data('order-id');
    _orderService.reorderOrder(orderId)
      .done(function () {
        abp.notify.success('Đã đặt lại đơn thành công!');
        _$ordersTable.ajax.reload();

      })
      .fail(function () {
        abp.notify.error('Hủy đơn thất bại!');
      });
  });

  // Chỉnh sửa đơn hàng
  $(document).on('click', '.edit-order', function () {
    var orderId = $(this).data('order-id');
    abp.ajax({
      url: abp.appPath + 'Orders/EditModal?orderId=' + orderId,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#OrderEditModal .modal-content').html(content);
        $('#OrderEditModal').modal('show');
      },
      error: function (e) {
        abp.notify.error('Could not load edit form');
      }
    });
  });

  // Tìm kiếm
  $('.btn-search').on('click', function () {
    _$ordersTable.ajax.reload();
  });

  $('#OrdersTableFilter').on('keypress', function (e) {
    if (e.which === 13) {
      _$ordersTable.ajax.reload();
    }
  });

})(jQuery);