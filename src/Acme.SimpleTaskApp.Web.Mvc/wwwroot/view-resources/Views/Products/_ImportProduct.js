(function ($) {
  'use strict';

  app.modals.ProductImportModal = function () {
    var _modalManager;
    var _$form = null;
    var _selectedFile = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$form = $modal.find('form[name=ImportProductForm]');

      this.bindEvents($modal);
    };

    this.bindEvents = function ($modal) {
      const self = this;

      // File input change event
      $modal.find('input[name="excelFile"]').on('change', function (e) {
        const file = e.target.files[0];
        if (file) {
          self._selectedFile = file;
          const fileName = file.name;
          $modal.find('.file-name-display').text(fileName);
          $modal.find('.file-info').show();
        }
      });

      // Drag and drop
      const dropZone = $modal.find('.drop-zone');

      dropZone.on('dragover dragenter', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('drag-over');
      });

      dropZone.on('dragleave drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');
      });

      dropZone.on('drop', function (e) {
        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
          $modal.find('input[name="excelFile"]')[0].files = files;
          $modal.find('input[name="excelFile"]').trigger('change');
        }
      });

      // Upload button
      $modal.find('.btn-import').on('click', function () {
        self.submitImport();
      });

      // Download template
      $modal.find('.btn-download-template').on('click', function () {
        self.downloadTemplate();
      });

      // Close modal reset
      $modal.on('hidden.bs.modal', function () {
        self.resetForm($modal);
      });
    };

    this.submitImport = function () {
      if (!this._selectedFile) {
        abp.notify.warn('Vui lòng chọn file Excel để import');
        return;
      }

      if (!this._selectedFile.name.match(/\.(xlsx|xls)$/i)) {
        abp.notify.error('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
        return;
      }

      this.uploadFile();
    };

    this.uploadFile = function () {
      const self = this;
      const $modal = _modalManager.getModal();
      const formData = new FormData();
      formData.append('file', this._selectedFile);

      _modalManager.setBusy(true);
      const $progressBar = $modal.find('.progress-bar');
      const $resultArea = $modal.find('.import-result');

      $.ajax({
        url: '/api/ProductImport/import-excel',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        xhr: function () {
          const xhr = new window.XMLHttpRequest();
          xhr.upload.addEventListener('progress', function (e) {
            if (e.lengthComputable) {
              const percentComplete = (e.loaded / e.total) * 100;
              $progressBar.css('width', percentComplete + '%');
              $progressBar.text(Math.round(percentComplete) + '%');
            }
          }, false);
          return xhr;
        },
        success: function (response) {
          _modalManager.setBusy(false);
          self.showResult(response, $resultArea);

          if (response.success) {
            // Reload product table nếu có
            if (typeof _$productsTable !== 'undefined') {
              setTimeout(() => {
                _$productsTable.ajax.reload();
              }, 1500);
            }
          }
        },
        error: function (xhr, status, error) {
          _modalManager.setBusy(false);
          const errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra khi upload file';
          self.showErrorResult(errorMsg, $resultArea);
        },
        complete: function () {
          $progressBar.css('width', '0%').text('0%');
        }
      });
    };

    this.showResult = function (response, $resultArea) {
      let html = '';

      if (response.success) {
        html += `
          <div class="alert alert-success alert-dismissible fade show" role="alert">
            <h5 class="alert-heading">
              <i class="fas fa-check-circle"></i> Import Thành Công!
            </h5>
            <p class="mb-2">
              <strong>Kết quả:</strong> ${response.successCount}/${response.totalCount} sản phẩm đã được import
            </p>
            <p class="mb-0">${response.message}</p>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
          </div>
        `;

        // Hiển thị warnings nếu có
        if (response.warnings && response.warnings.length > 0) {
          html += `
            <div class="alert alert-warning alert-dismissible fade show mt-3" role="alert">
              <h6 class="alert-heading">
                <i class="fas fa-exclamation-triangle"></i> 
                ${response.warningCount} Cảnh báo
              </h6>
              <ul class="mb-0 small">
          `;

          response.warnings.forEach(warning => {
            html += `<li>${this.escapeHtml(warning)}</li>`;
          });

          html += `
              </ul>
              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
          `;
        }
      } else {
        html += `
          <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <h5 class="alert-heading">
              <i class="fas fa-exclamation-circle"></i> Import Thất Bại!
            </h5>
            <p class="mb-2"><strong>Lỗi:</strong> ${this.escapeHtml(response.message)}</p>
            ${response.details ? `<p class="mb-0 small text-muted">${this.escapeHtml(response.details)}</p>` : ''}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
          </div>
        `;
      }

      $resultArea.html(html).show();
    };

    this.showErrorResult = function (errorMsg, $resultArea) {
      const html = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
          <h5 class="alert-heading">
            <i class="fas fa-times-circle"></i> Lỗi Upload
          </h5>
          <p class="mb-0">${this.escapeHtml(errorMsg)}</p>
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
      `;
      $resultArea.html(html).show();
    };

    this.downloadTemplate = function () {
      const self = this;
      const $modal = _modalManager.getModal();

      _modalManager.setBusy(true);

      $.ajax({
        url: abp.appPath + 'Template/GenerateProductImportTemplate',
        method: 'GET',
        xhrFields: {
          responseType: 'blob'
        },
        success: function (blob, status, xhr) {
          // Lấy tên file từ Content-Disposition header
          let filename = 'ProductImportTemplate.xlsx'; // default fallback
          
          const disposition = xhr.getResponseHeader('Content-Disposition');
          if (disposition) {
            const filenameMatch = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (filenameMatch && filenameMatch[1]) {
              filename = filenameMatch[1].replace(/['"]/g, '');
              // Decode nếu có UTF-8 encoding
              filename = decodeURIComponent(filename);
            }
          }

          // Tạo URL tạm thời
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = filename; // Sử dụng tên từ server
          document.body.appendChild(link);
          link.click();

          // Dọn dẹp
          window.URL.revokeObjectURL(url);
          document.body.removeChild(link);

          _modalManager.setBusy(false);
          abp.notify.success('Tải template thành công!');
        },
        error: function (xhr, status, error) {
          console.error('Lỗi tải template:', error);
          _modalManager.setBusy(false);
          abp.notify.error('Tải template thất bại: ' + (xhr.responseJSON?.message || error));
        }
      });
    };
    this.resetForm = function ($modal) {
      if ($modal) {
        $modal.find('form')[0].reset();
        $modal.find('input[name="excelFile"]').val('');
        this._selectedFile = null;
        $modal.find('.file-name-display').text('Chưa chọn file');
        $modal.find('.file-info').hide();
        $modal.find('.import-result').html('').hide();
        $modal.find('.progress-bar').css('width', '0%').text('0%');
      }
    };

    this.escapeHtml = function (text) {
      const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
      };
      return text.replace(/[&<>"']/g, m => map[m]);
    };

    // Hàm save để tương thích với ModalManager
    this.save = function () {
      this.submitImport();
    };
  };

})(jQuery);