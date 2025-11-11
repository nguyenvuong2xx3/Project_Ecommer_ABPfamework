(function () {
  app.modals.ProductExportModal = function () {
    var _modalManager;
    var _productImportExportService = abp.services.app.productImportExport;
    var _$form = null;
    var _filterData;

    var _selectedDateRange = {
      StartTime: null,
      EndTime: null
    };
    $('#StartEndRangeExport').daterangepicker({
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

    // Sự kiện apply
    $('#StartEndRangeExport').on('apply.daterangepicker', function (ev, picker) {
      $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));

      // CẬP NHẬT _selectedDateRange
      _selectedDateRange.StartTime = picker.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss');
      _selectedDateRange.EndTime = picker.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss');
    });

    $('#StartEndRangeExport').on('cancel.daterangepicker', function (ev, picker) {
      $(this).val('');
      _selectedDateRange.StartTime = null;
      _selectedDateRange.EndTime = null;
    });

    this.init = function (modalManager) {
      _modalManager = modalManager;
      _filterData = _modalManager.getArgs().filter;

      _$form = _modalManager.getModal().find('form[name=ExportForm]');
      _$form.validate({
        validClass: "valid",
        errorClass: "invalid-feedback",
        highlight: function (element, errorClass, validClass) {
          $(element).addClass('is-invalid').removeClass('is-valid');
        },
        unhighlight: function (element, errorClass, validClass) {
          $(element).addClass('is-valid').removeClass('is-invalid');
        },
        rules: {
          SelectFileFormat: {
            required: true,
          },
        },
        messages: {
          SelectFileFormat: {
            required: 'Định dạng file phải được chọn',
          },
        },
      });

      // Xử lý checkbox "Chọn tất cả"
      if ($('#alloption').is(':checked') == true) {
        $('.input-options').find("input:checkbox:not(:checked)").prop("checked", true);
      } else {
        $('.input-options').find("input:checkbox").prop("checked", false);
      }

      $('#alloption').on('change', function () {
        if ($(this).is(':checked') == true) {
          $('.input-options').find("input:checkbox:not(:checked)").prop("checked", true);
        } else {
          $('.input-options').find("input:checkbox").prop("checked", false);
        }
      });

      $('.input-options').find("input:checkbox").on('change', function () {
        if ($('.input-options').find("input:checkbox:not(:checked)").length > 0) {
          $('#alloption').prop("checked", false)
        } else {
          $('#alloption').prop("checked", true)
        }
      });

    };

    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      var filter = function () {
        var datafilter = {};

        // THÊM CATEGORY ID VÀ CREATION TIME VÀO BỘ LỌC
        datafilter.categoryId = $("#CategoryFilter").val();

        // Sử dụng _selectedDateRange đã được cập nhật từ date range picker
        datafilter.creationTimeStart = _selectedDateRange.StartTime;
        datafilter.creationTimeEnd = _selectedDateRange.EndTime;

        datafilter.exportFileType = $("#SelectFileFormat").val();

        return datafilter;
      };

      _modalManager.setBusy(true);
      _productImportExportService
        .exportProducts(filter())
        .done(function (result) {
          debugger;
          if (result.isSuccess) {
            // 1. Lấy các thuộc tính từ kết quả trả về
            // Lưu ý: C# "FileContent" khi qua JSON có thể thành "fileContent" (camelCase)
            var b64Data = result.fileContent;
            var contentType = result.contentType;
            var fileName = result.fileName || 'exported_file';

            // 2. Tạo Data URL từ chuỗi Base64
            // Cú pháp: 'data:[<mime_type>][;base64],<data>'
            var dataUrl = 'data:' + contentType + ';base64,' + b64Data;

            // 3. Tạo link và gán Data URL
            var link = document.createElement('a');
            link.href = dataUrl; // Sử dụng Data URL
            link.download = fileName; // Sử dụng tên file từ result
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            abp.notify.success('Export successful!');
          } else {
            abp.notify.error('Export failed. Please try again.');
          }
          _modalManager.close();
        })
    };
  };
})();