(function ($) {
  var _productService = abp.services.app.product,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$modal = $('#ProductCreateModal'),
    _$form = _$modal.find('form'),
    _$table = $('#ProductsTable');

  var _permissions = {
    view: abp.auth.hasPermission('Pages.Products.View'),
    create: abp.auth.hasPermission('Pages.Products.Create'),
    update: abp.auth.hasPermission('Pages.Products.Update'),
    delete: abp.auth.hasPermission('Pages.Products.Delete'),
  };
  
  var _importExcelModal = new app.ModalManager({
    viewUrl: abp.appPath + 'Products/ImportModal',
    scriptUrl: abp.appPath + 'view-resources/Views/Products/_ImportDataModal.js',
    modalClass: 'ProductImportModal',
    modalSize: 'modal-lg'
  });

  $('#ImportExcelBtn').click(function () {
    _importExcelModal.open();
  });

  var _exportProductModal = new app.ModalManager({
    viewUrl: abp.appPath + 'Products/ExportModal',
    scriptUrl: abp.appPath + 'view-resources/Views/Products/_ExportProductModal.js',
    modalClass: 'ProductExportModal',
    modalSize: 'modal-lg'
  });

  $('#ExportExcelBtn').click(function () {
    _exportProductModal.open();
  });

  var _createModal = new app.ModalManager({
    viewUrl: abp.appPath + 'Products/CreateModal',
    scriptUrl: abp.appPath + 'view-resources/Views/Products/_CreateModal.js',
    modalClass: 'ProductCreateModal',
    modalSize: 'modal-lg'
  });

  $('#CreateNewButton').click(function () {
    _createModal.open();
  });

  var _editModal = new app.ModalManager({
    viewUrl: abp.appPath + 'Products/EditModal',
    scriptUrl: abp.appPath + 'view-resources/Views/Products/_EditModal.js',
    modalClass: 'ProductEditModal',
    modalSize: 'modal-lg'
  });

  $(document).on('click', '.edit-product', function () {
    var productId = $(this).attr("data-product-id");
    _editModal.open({ productId: productId });
  });


  // Sửa lại toàn bộ phần Date Range Picker
  var _selectedDateRange = {
    StartTime: null,
    EndTime: null
  };

  // Date Range Picker - SỬA LẠI HOÀN TOÀN
  $('#StartEndRange').daterangepicker({
    autoUpdateInput: false,
    opens: 'left',
    locale: {
      format: 'DD/MM/YYYY',
      applyLabel: 'Áp dụng',
      cancelLabel: 'Hủy bỏ',
      fromLabel: 'Từ',
      toLabel: 'Đến',
      customRangeLabel: 'Phạm vi tùy chỉnh',
      firstDay: 1
    }
  });

  // Sự kiện apply - QUAN TRỌNG: phải dùng 'apply.daterangepicker'
  $('#StartEndRange').on('apply.daterangepicker', function (ev, picker) {
    $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));

    // CẬP NHẬT _selectedDateRange - SỬA ĐỊNH DẠNG
    _selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
    _selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
  });

  $('#StartEndRange').on('cancel.daterangepicker', function (ev, picker) {
    $(this).val('');
    _selectedDateRange.StartTime = null;
    _selectedDateRange.EndTime = null;
  });


  // Hiển thị/Ẩn bộ lọc nâng cao
  $('#ShowAdvancedFiltersSpan').click(function () {
    $('#ShowAdvancedFiltersSpan').hide();
    $('#HideAdvancedFiltersSpan').show();
    $('#AdvacedAuditFiltersArea').slideDown();
  });

  $('#HideAdvancedFiltersSpan').click(function () {
    $('#HideAdvancedFiltersSpan').hide();
    $('#ShowAdvancedFiltersSpan').show();
    $('#AdvacedAuditFiltersArea').slideUp();
  });

  // Reset bộ lọc
  $('#ResetFilters').click(function () {
    $('#ProductSearchForm')[0].reset();
    _selectedDateRange.StartTime = null;
    _selectedDateRange.EndTime = null;
    $('#StartEndRange').val('');
    _$productsTable.ajax.reload();
  });

  // DataTable
  var _$productsTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    processing: true,
    listAction: {
      ajaxFunction: _productService.getAllProduct,
      inputFilter: function () {
        var formData = $('#ProductSearchForm').serializeFormToObject(true);
        if (_selectedDateRange.StartTime) {
          formData.startTime = moment(_selectedDateRange.StartTime).format('YYYY-MM-DDT00:00:00');
        }
        if (_selectedDateRange.EndTime) {
          formData.endTime = moment(_selectedDateRange.EndTime).format('YYYY-MM-DDT23:59:59');
        }
        return formData;
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$productsTable.draw(false),
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
        data: 'name',
        sortable: false
      },
      {
        targets: 1,
        data: 'screen',
        sortable: false
      },
      {
        targets: 2,
        data: 'battery',
        sortable: false
      },
      {
        targets: 3,
        data: 'cameraSystem',
        sortable: false
      },
      {
        targets: 4,
        data: 'processor',
        sortable: false
      },
      {
        targets: 5,
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
        targets: 6,
        data: 'creationTime',
        sortable: false,
        render: function (data, type, row) {
          if (data) {
            return `<span class="badge bg-info">${moment(data).format('DD-MM-YYYY HH:mm')}</span>`; // HH:mm (chữ m thường)
          }
          return '<span class="text-muted">Không có thời gian</span>';
        }
      },
      {
        targets: 7,
        data: null,
        sortable: false,
        autoWidth: true,
        defaultContent: '',
        render: (data, type, row, meta) => {
          let buttons = [];

          if (_permissions.update) {
            buttons.push(
              `<button type="button" class="btn btn-sm bg-secondary edit-product me-2" data-product-id="${row.id}" data-toggle="modal" data-target="#ProductEditModal">` +
              `   <i class="fas fa-pencil-alt"></i> ${l('Edit')}` +
              '</button>'
            );
          }

          if (_permissions.delete) {
            buttons.push(
              `<button type="button" class="btn btn-sm bg-danger delete-product me-2" data-product-id="${row.id}" data-product-name="${row.name}">` +
              `   <i class="fas fa-trash"></i> ${l('Delete')}` +
              '</button>'
            );
          }

          if (_permissions.view) {
            buttons.push(
              `<button type="button" class="btn btn-sm bg-info detail-product" data-product-id="${row.id}" data-toggle="modal">` +
              `   <i class="fas fa-eye"></i> ${l('Details')}` +
              '</button>'
            );
          }

          return `<div class="d-flex justify-content-center">${buttons.join('')}</div>`;
        }
      }
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

  // Refresh table
  $(document).on('click', '.buttons-refresh', function () {
    _$productsTable.ajax.reload();
  });

  // Tìm kiếm
  $('.btn-search').on('click', (e) => {
    updateDateRangeFromPicker();
    _$productsTable.ajax.reload();
  });

  $('.txt-search').on('keypress', (e) => {
    if (e.which == 13) {
      updateDateRangeFromPicker();
      _$productsTable.ajax.reload();
      return false;
    }
  });
  function updateDateRangeFromPicker() {
    var currentValue = $('#StartEndRange').val();
    if (currentValue) {
      var dates = currentValue.split(' - ');
      if (dates.length === 2) {
        var startDate = moment(dates[0], 'DD/MM/YYYY').startOf('day');
        var endDate = moment(dates[1], 'DD/MM/YYYY').endOf('day');

        _selectedDateRange.StartTime = startDate.format('YYYY-MM-DDTHH:mm:ss');
        _selectedDateRange.EndTime = endDate.format('YYYY-MM-DDTHH:mm:ss');
      }
    }
  }
  // Lưu sản phẩm
  //_$form.find('.save-button').on('click', (e) => {
  //  e.preventDefault();

  //  if (!_$form.valid()) {
  //    return;
  //  }

  //  var formData = new FormData(_$form[0]);
  //  abp.ui.setBusy(_$modal);

  //  $.ajax({
  //    url: abp.appPath + 'Products/Create',
  //    type: 'POST',
  //    processData: false,
  //    contentType: false,
  //    data: formData,
  //    error: function (xhr, textStatus, errorThrown) {
  //      var errorMessage;
  //      if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
  //        errorMessage = xhr.responseJSON.errors.join("<br/>");
  //      } else {
  //        errorMessage = "Có lỗi xảy ra khi tạo mới sản phẩm (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
  //      }
  //      $("#error-message").html(errorMessage).show();
  //    }
  //  }).done(function () {
  //    resetDefaultImage();
  //    _$modal.modal('hide');
  //    _$form[0].reset();
  //    abp.notify.info(l('Lưu thành công'));
  //    _$productsTable.ajax.reload();
  //  }).always(function () {
  //    abp.ui.clearBusy(_$modal);
  //  });
  //});

  // Xóa sản phẩm
  $(document).on('click', '.delete-product', function () {
    var productId = $(this).attr("data-product-id");
    var productName = $(this).attr('data-product-name');
    deleteProduct(productId, productName);
  });

  function deleteProduct(productId, productName) {
    abp.message.confirm(
      abp.utils.formatString(l('Bạn có muốn xóa sản phẩm "{0}"?'), productName),
      null,
      (isConfirmed) => {
        if (isConfirmed) {
          $.ajax({
            url: '/Products/Delete',
            type: 'POST',
            data: { id: productId }
          }).done(() => {
            abp.notify.info(l('Xoá thành công'));
            _$productsTable.ajax.reload();
          }).fail((xhr) => {
            let errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra khi xoá sản phẩm có thể là do sản phẩm đã được người dùng thêm vào giỏ hàng';
            abp.notify.error(errorMsg);
          });
        }
      }
    );
  }

  // Export Excel
  //$('#ExportExcelBtn').click(function () {
  //  var input = $('#ProductSearchForm').serializeFormToObject(true);

  //  // Thêm phạm vi ngày vào input export
  //  if (_selectedDateRange.StartTime) {
  //    input.CreationTimeStart = _selectedDateRange.StartTime;
  //  }
  //  if (_selectedDateRange.EndTime) {
  //    input.CreationTimeEnd = _selectedDateRange.EndTime;
  //  }

  //  abp.ui.setBusy();

  //  $.ajax({
  //    url: abp.appPath + 'Products/ExportToExcel',
  //    type: 'POST',
  //    data: JSON.stringify(input),
  //    contentType: 'application/json',
  //    headers: {
  //      'RequestVerificationToken': abp.security.antiForgery.getToken()
  //    },
  //    xhrFields: {
  //      responseType: 'blob'
  //    },
  //    success: function (blob) {
  //      var url = window.URL.createObjectURL(blob);
  //      var a = document.createElement('a');
  //      a.href = url;
  //      a.download = 'Danh_sach_san_pham.xlsx';
  //      document.body.appendChild(a);
  //      a.click();
  //      window.URL.revokeObjectURL(url);
  //      document.body.removeChild(a);
  //      abp.notify.success('Xuất Excel thành công');
  //    },
  //    error: function (xhr) {
  //      abp.notify.error('Xuất Excel thất bại: ' + xhr.statusText);
  //    },
  //    complete: function () {
  //      abp.ui.clearBusy();
  //    }
  //  });
  //});

  // Xem chi tiết sản phẩm
  $(document).on('click', '.detail-product', function () {
    var productId = $(this).data("product-id");
    abp.ajax({
      url: abp.appPath + 'Products/DetailModal?productId=' + productId,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#ProductDetailModal .modal-content').html(content);
        $('#ProductDetailModal').modal('show');
      },
      error: function (e) {
        abp.notify.error('Không thể tải form chi tiết');
      }
    });
  });

  // Import Excel
  //$(document).ready(function () {
  //  $('#excelFile').on('change', function () {
  //    var fileName = $(this).val().split('\\').pop();
  //    $(this).next('.custom-file-label').html(fileName || 'Chưa chọn file');
  //    $('#importError').hide();
  //  });

  //  $('#btnImportExcel').click(function () {
  //    var fileInput = $('#excelFile')[0];
  //    if (!fileInput.files || fileInput.files.length === 0) {
  //      $('#importError').text('Vui lòng chọn file Excel').show();
  //      return;
  //    }

  //    var formData = new FormData();
  //    formData.append('file', fileInput.files[0]);

  //    abp.ui.setBusy($('#ImportExcelModal'));
  //    $.ajax({
  //      url: abp.appPath + 'Products/ImportFromExcel',
  //      type: 'POST',
  //      data: formData,
  //      processData: false,
  //      contentType: false,
  //      success: function (response) {
  //        abp.notify.success('Nhập dữ liệu thành công!');
  //        $('#ImportExcelModal').modal('hide');
  //        _$productsTable.ajax.reload();
  //      },
  //      error: function (xhr) {
  //        var errorMessage = xhr.responseJSON?.message || 'Lỗi khi nhập dữ liệu';
  //        $('#importError').html(errorMessage).show();
  //      },
  //      complete: function () {
  //        abp.ui.clearBusy($('#ImportExcelModal'));
  //      }
  //    });
  //  });

  //  $('#ImportExcelModal').on('hidden.bs.modal', function () {
  //    $('#ImportExcelForm')[0].reset();
  //    $('.custom-file-label').html('Chưa chọn file');
  //    $('#importError').hide();
  //  });
  //});

  // Reset ảnh mặc định
  function resetDefaultImage() {
    const input = document.getElementById('productImage');
    const preview = document.getElementById('productImagePreview');
    const deleteBtn = document.getElementById('deleteProductImageBtn');
    if (input) input.value = '';
    if (preview) {
      preview.src = '';
      preview.style.display = 'none';
    }
    if (deleteBtn) deleteBtn.style.display = 'none';
  }

  // Reset form khi đóng modal
  $('#ProductCreateModal').on('hidden.bs.modal', function () {
    $('#ProductCreateModal form')[0].reset();
    resetDefaultImage();
    $("#error-message").hide();
  });

  // Validation form
  $(document).ready(function () {
    $("form[name='productCreateForm']").validate({
      rules: {
        Name: {
          required: true,
          minlength: 5,
          maxlength: 128
        },
        Description: {
          required: true,
          minlength: 10,
          maxlength: 256
        },
        SKU: {
          required: true,
          minlength: 3,
          maxlength: 50
        },
        Screen: {
          required: true,
          minlength: 5,
        },
        Processor: {
          required: true,
          minlength: 3,
        },
        CameraSystem: {
          required: true,
          minlength: 5,
        },
        Battery: {
          required: true,
          minlength: 5,
        },
        CategoryId: {
          required: true
        },
        StockQuantity: {
          required: true,
          min: 0,
          number: true
        }
      },
      messages: {
        Name: {
          required: "Tên sản phẩm không được để trống",
          minlength: "Tên sản phẩm phải có ít nhất 5 ký tự",
          maxlength: "Tên sản phẩm tối đa 128 ký tự"
        },
        Description: {
          required: "Mô tả không được để trống",
          minlength: "Mô tả phải có ít nhất 10 ký tự",
          maxlength: "Mô tả tối đa 256 ký tự"
        },
        SKU: {
          required: "Mã SKU không được để trống",
          minlength: "Mã SKU phải có ít nhất 3 ký tự",
          maxlength: "Mã SKU tối đa 50 ký tự"
        },
        Screen: {
          required: "Thông số màn hình không được để trống",
          minlength: "Thông số màn hình phải có ít nhất 5 ký tự"
        },
        Processor: {
          required: "Bộ vi xử lý không được để trống",
          minlength: "Thông tin bộ vi xử lý phải có ít nhất 3 ký tự"
        },
        CameraSystem: {
          required: "Thông tin camera không được để trống",
          minlength: "Thông tin camera phải có ít nhất 5 ký tự"
        },
        Battery: {
          required: "Thông tin pin không được để trống",
          minlength: "Thông tin pin phải có ít nhất 5 ký tự"
        },
        CategoryId: {
          required: "Vui lòng chọn danh mục sản phẩm"
        },
        StockQuantity: {
          required: "Số lượng tồn kho không được để trống",
          min: "Số lượng tồn kho không được âm",
          number: "Số lượng tồn kho phải là số"
        }
      },
      errorElement: "div",
      errorClass: "text-danger"
    });
  });

  // Event khi product được edit
  abp.event.on('product.edited', (data) => {
    _$productsTable.ajax.reload();
  });

  abp.event.on('app.productCreatedOrUpdated', function () {
    _$productsTable.ajax.reload();
  });

  // Focus khi modal hiển thị
  _$modal.on('shown.bs.modal', () => {
    _$modal.find('input:not([type=hidden]):first').focus();
  }).on('hidden.bs.modal', () => {
    _$form.clearForm();
  });

})(jQuery);