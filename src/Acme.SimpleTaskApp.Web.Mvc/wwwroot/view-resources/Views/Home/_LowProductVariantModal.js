(function ($) {
  app.modals.LowProductVariantModal = function () {
    var _modalManager;
    var _lowproductVariantService = abp.services.app.dashboard;
    var l = abp.localization.getSource('SimpleTaskApp');
    var _$modal;
    var _$form;
    var _$table;
    var _$lowproductvariantsTable;
    var _isDataTableInitialized = false; // Biến để kiểm tra đã khởi tạo chưa

    this.init = function (modalManager) {
      _modalManager = modalManager;
      _$modal = _modalManager.getModal();
      _$form = _$modal.find('form');
      _$table = _$modal.find('#LowProductVariantsTable');

      _registerEvents();
      // KHÔNG gọi _initDataTable() ở đây nữa
    };

    function _initDataTable() {
      if (_isDataTableInitialized) {
        return; // Đã khởi tạo rồi thì không khởi tạo lại
      }

      _$lowproductvariantsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        processing: true,
        listAction: {
          ajaxFunction: _lowproductVariantService.getLowStockProducts,
          inputFilter: function () {
            return {};
          }
        },
        buttons: [
          {
            name: 'refresh',
            text: '<i class="fas fa-redo-alt"></i>',
            action: () => _$lowproductvariantsTable.draw(false),
          }
        ],
        responsive: {
          details: {
            type: 'column'
          }
        },
        columnDefs: [
          {
            targets: 0,
            data: 'productName',
            sortable: false
          },
          {
            targets: 1,
            data: 'imageUrl',
            sortable: false,
            render: function (data, type, row) {
              if (data) {
                return `<img src="${data}" alt="Ảnh sản phẩm" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
              }
              return '<span class="text-muted">Không có ảnh</span>';
            }
          },
          {
            targets: 2,
            data: 'ram',
            sortable: false
          },
          {
            targets: 3,
            data: 'storage',
            sortable: false
          },
          {
            targets: 4,
            data: 'color',
            sortable: false
          },
          {
            targets: 5,
            data: 'stockQuantity',
            sortable: false,
            render: function (data, type, row) {
              return `<span class="badge ${data > 0 ? 'bg-success' : 'bg-danger'}">${data}</span>`;
            }
          },
        ],
        language: {
          emptyTable: "Không có dữ liệu",
          info: "Hiển thị _START_ đến _END_ của _TOTAL_ bản ghi",
          infoEmpty: "Hiển thị 0 đến 0 của 0 bản ghi",
          infoFiltered: "(lọc từ _MAX_ tổng số bản ghi)",
          lengthMenu: "Hiển thị _MENU_ bản ghi",
          loadingRecords: "Đang tải...",
          processing: "Đang xử lý...",
          search: "Tìm kiếm:",
          zeroRecords: "Không tìm thấy kết quả phù hợp",
          paginate: {
            first: "Đầu",
            last: "Cuối",
            next: "Tiếp",
            previous: "Trước"
          }
        }
      });

      _isDataTableInitialized = true; // Đánh dấu đã khởi tạo
    }

    function _registerEvents() {
      _$modal.on('click', '.buttons-refresh', function () {
        if (_$lowproductvariantsTable) {
          _$lowproductvariantsTable.ajax.reload();
        }
      });

      // CHỈ khởi tạo DataTable khi modal được mở lần đầu
      _$modal.on('shown.bs.modal', function () {
        if (!_isDataTableInitialized) {
          _initDataTable(); // Chỉ khởi tạo lần đầu
        }
        // KHÔNG gọi ajax.reload() ở đây nữa
      });
    }

    this.open = function () {
      _modalManager.open();
    };

    this.close = function () {
      _modalManager.close();
    };

    this.refresh = function () {
      if (_$lowproductvariantsTable) {
        _$lowproductvariantsTable.ajax.reload();
      }
    };
  };
})(jQuery);