(function ($) {
  var _orderService = abp.services.app.orders,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$table = $('#OrdersTable');

  var _$ordersTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    processing: true,
    listAction: {
      ajaxFunction: _orderService.getAllOrder,
      inputFilter: function () {
        return $('#OrdersTableFilter').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$ordersTable.draw(false)
      }
    ],
    columnDefs: [
      //{
      //  targets: 0,
      //  data: 'code',
      //  render: function (data) {
      //    return '#' + data.toString().padStart(8, '0');
      //  }
      //},
      {
        targets: 0,
        data: 'customerName'
      },
      {
        targets: 1,
        data: 'orderDate',
        //render: function (data) {
        //  return new Date(data).toLocaleDateString('vi-VN');
        //}
      },
      {
        targets: 2,
        data: 'totalAmount',
        //render: function (data) {
        //  return data.toLocaleString('vi-VN') + '₫';
        //}
      },
      {
        targets: 3,
        data: 'status',
        render: function (data) {
          switch (data) {
            case 0: return '<span class="badge bg-info">Mới</span>';
            case 1: return '<span class="badge bg-warning">Đang xử lý</span>';
            case 2: return '<span class="badge bg-success">Hoàn thành</span>';
            case 3: return '<span class="badge bg-danger">Đã hủy</span>';
            default: return data;
          }
        }
      },
      {
        targets: 4,
        data: null,
        orderable: false,
        render: function (data, type, row) {
          return [
            `<button type="button" class="btn btn-sm bg-info detail-order" data-order-id="${row.id}">`,
            `<i class="fas fa-eye"></i> ${l('Detail')}`,
            '</button>',
            `<button type="button" class="btn btn-sm bg-primary edit-order" data-order-id="${row.id}">`,
            `<i class="fas fa-edit"></i> ${l('Edit')}`,
            '</button>'
          ].join('');
        }
      }
    ]
  });

  // Xem chi tiết đơn hàng
  $(document).on('click', '.detail-order', function () {
    var orderId = $(this).data('order-id');
    abp.ajax({
      url: abp.appPath + 'Orders/DetailModal?orderId=' + orderId,
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