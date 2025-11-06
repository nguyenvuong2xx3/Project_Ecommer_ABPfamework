(function ($) {
  'use strict';

  const ImportProductModal = {
    _$modal: null,
    _$form: null,
    _$fileInput: null,
    _selectedFile: null,

    init: function () {
      this._$modal = $('#ImportProductModal');
      this._$form = this._$modal.find('form');
      this._$fileInput = this._$modal.find('input[name="excelFile"]');

      this.bindEvents();
    },

    bindEvents: function () {
      const self = this;

      // File input change event
      this._$fileInput.on('change', function (e) {
        const file = e.target.files[0];
        if (file) {
          self._selectedFile = file;
          const fileName = file.name;
          self._$modal.find('.file-name-display').text(fileName);
          self._$modal.find('.file-info').show();
        }
      });

      // Drag and drop
      const dropZone = this._$modal.find('.drop-zone');
      
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
          self._$fileInput[0].files = files;
          self._$fileInput.trigger('change');
        }
      });

      // Upload button
      this._$modal.find('.btn-import').on('click', function () {
        self.submitImport();
      });

      // Download template
      this._$modal.find('.btn-download-template').on('click', function () {
        self.downloadTemplate();
      });

      // Close modal reset
      this._$modal.on('hidden.bs.modal', function () {
        self.resetForm();
      });
    },

    submitImport: function () {
      if (!this._selectedFile) {
        abp.notify.warn('Vui lòng chọn file Excel để import');
        return;
      }

      if (!this._selectedFile.name.match(/\.(xlsx|xls)$/i)) {
        abp.notify.error('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
        return;
      }

      this.uploadFile();
    },

    uploadFile: function () {
      const self = this;
      const formData = new FormData();
      formData.append('file', this._selectedFile);

      abp.ui.setBusy(this._$modal);
      const $progressBar = this._$modal.find('.progress-bar');
      const $resultArea = this._$modal.find('.import-result');

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
          abp.ui.clearBusy(self._$modal);
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
          abp.ui.clearBusy(self._$modal);
          const errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra khi upload file';
          self.showErrorResult(errorMsg, $resultArea);
        },
        complete: function () {
          $progressBar.css('width', '0%').text('0%');
        }
      });
    },

    showResult: function (response, $resultArea) {
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
    },

    showErrorResult: function (errorMsg, $resultArea) {
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
    },

    downloadTemplate: function () {
      // Tạo Excel template
      const link = document.createElement('a');
      link.href = '/files/templates/ProductImportTemplate.xlsx';
      link.download = 'ProductImportTemplate.xlsx';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      abp.notify.info('Template đang tải về...');
    },

    resetForm: function () {
      this._$form[0].reset();
      this._$fileInput.val('');
      this._selectedFile = null;
      this._$modal.find('.file-name-display').text('Chưa chọn file');
      this._$modal.find('.file-info').hide();
      this._$modal.find('.import-result').html('').hide();
      this._$modal.find('.progress-bar').css('width', '0%').text('0%');
    },

    escapeHtml: function (text) {
      const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
      };
      return text.replace(/[&<>"']/g, m => map[m]);
    }
  };

  // Initialize khi document ready
  $(document).ready(function () {
    ImportProductModal.init();

    // Export button
    $('#ExportTemplateBtn').on('click', function () {
      ImportProductModal.downloadTemplate();
    });

    // Import button (show modal)
    $('#ImportProductBtn').on('click', function () {
      $('#ImportProductModal').modal('show');
    });
  });

  // Export global
  window.ImportProductModal = ImportProductModal;

})(jQuery);
