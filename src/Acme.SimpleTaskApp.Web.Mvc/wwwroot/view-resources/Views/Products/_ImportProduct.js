(function ($) {
  'use strict';

  app.modals.ProductImportModal = function () {
    var _modalManager;
    var _$form = null;
    var _selectedFile = null;

    // ĐỊNH NGHĨA CÁC PHƯƠNG THỨC TRƯỚC
    this.submitImport = function () {
      console.log('Submit import called');
      console.log('Selected file:', _selectedFile);

      if (!_selectedFile) {
        abp.notify.warn('Vui lòng chọn file Excel để import');
        return;
      }

      if (!_selectedFile.name.match(/\.(xlsx|xls)$/i)) {
        abp.notify.error('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
        return;
      }

      console.log('File validation passed, starting upload...');
      this.uploadFile();
    }.bind(this); // QUAN TRỌNG: Bind context

    this.uploadFile = function () {
      const $modal = _modalManager.getModal();
      const formData = new FormData();
      formData.append('file', _selectedFile);

      _modalManager.setBusy(true);
      const $progressBar = $modal.find('.progress-bar');
      const $resultArea = $modal.find('.import-result');
      const $progressContainer = $modal.find('.progress');

      // Hiển thị progress bar
      $progressContainer.show();

      $.ajax({
        url: abp.appPath + 'Products/ImportData',
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
          $progressContainer.hide();
          this.showResult(response, $resultArea);

          if (response.success) {
            // Reload product table nếu có
            if (typeof window.parent._$productsTable !== 'undefined') {
              setTimeout(() => {
                window.parent._$productsTable.ajax.reload();
              }, 1500);
            } else if (typeof _$productsTable !== 'undefined') {
              setTimeout(() => {
                _$productsTable.ajax.reload();
              }, 1500);
            }
          }
        }.bind(this), // QUAN TRỌNG: Bind context
        error: function (xhr, status, error) {
          _modalManager.setBusy(false);
          $progressContainer.hide();
          const errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra khi upload file';
          this.showErrorResult(errorMsg, $resultArea);
        }.bind(this), // QUAN TRỌNG: Bind context
        complete: function () {
          $progressBar.css('width', '0%').text('0%');
        }
      });
    }.bind(this); // QUAN TRỌNG: Bind context

    this.showResult = function (response, $resultArea) {
      let html = '';

      if (response.success) {
        html += `
                <div class="alert alert-success alert-dismissible fade show" role="alert">
                    <h5 class="alert-heading">
                        <i class="fas fa-check-circle"></i> Import Thành Công!
                    </h5>
                    <p class="mb-2">
                        <strong>Kết quả:</strong> ${response.successCount}/${response.totalCount} biến thể sản phẩm đã được import
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
      const $modal = _modalManager.getModal();

      _modalManager.setBusy(true);

      $.ajax({
        url: abp.appPath + 'Template/GenerateProductImportTemplate',
        method: 'GET',
        xhrFields: {
          responseType: 'blob'
        },
        success: function (blob, status, xhr) {
          let filename = 'ProductImportTemplate.xlsx';

          const disposition = xhr.getResponseHeader('Content-Disposition');
          if (disposition) {
            const filenameMatch = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (filenameMatch && filenameMatch[1]) {
              filename = filenameMatch[1].replace(/['"]/g, '');
              filename = decodeURIComponent(filename);
            }
          }

          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = filename;
          document.body.appendChild(link);
          link.click();

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

    this.resetFileSelection = function ($modal) {
      var $fileInput = $modal.find('#excelFileInput');
      var $dropZone = $modal.find('.drop-zone');
      var $fileInfo = $modal.find('.file-info');
      var $fileNameDisplay = $modal.find('.file-name-display');

      $fileInput.val('');
      _selectedFile = null;
      $fileNameDisplay.text('');
      $fileInfo.addClass('d-none').hide();

      $dropZone.removeClass('border-success bg-light');
      $dropZone.find('i').removeClass('fa-check-circle text-success')
        .addClass('fa-cloud-upload-alt text-primary');
      $dropZone.find('p').html('Kéo thả file Excel vào đây<br/><small class="text-muted">hoặc</small><br/><label for="excelFileInput" class="btn btn-sm btn-primary mt-2 cursor-pointer">Chọn File</label>');
    };

    this.resetForm = function ($modal) {
      if ($modal) {
        this.resetFileSelection($modal);
        $modal.find('.import-result').html('').hide();
        $modal.find('.progress').hide();
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
    }.bind(this); // QUAN TRỌNG: Bind context

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$form = $modal.find('form[name=ImportProductForm]');

      this.bindEvents($modal);
    };

    this.bindEvents = function ($modal) {
      var $fileInput = $modal.find('#excelFileInput');
      var $dropZone = $modal.find('.drop-zone');
      var $fileInfo = $modal.find('.file-info');
      var $fileNameDisplay = $modal.find('.file-name-display');

      // Lưu context vào biến self
      var self = this;

      // Click trên drop zone sẽ trigger file input
      $dropZone.on('click', function (e) {
        if (!$(e.target).is('label') && !$(e.target).is('button')) {
          $fileInput.trigger('click');
        }
      });

      // File input change event
      $fileInput.on('change', function (e) {
        const file = e.target.files[0];
        console.log('File selected:', file);
        if (file) {
          _selectedFile = file;
          const fileName = file.name;
          $fileNameDisplay.text(fileName);
          $fileInfo.removeClass('d-none').show();

          $dropZone.addClass('border-success bg-light');
          $dropZone.find('i').removeClass('fa-cloud-upload-alt text-primary')
            .addClass('fa-check-circle text-success');
          $dropZone.find('p').html(`<strong>${fileName}</strong> đã sẵn sàng để import`);
        } else {
          self.resetFileSelection($modal);
        }
      });

      // Drag and drop events
      $dropZone.on('dragover dragenter', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('drag-over');
      });

      $dropZone.on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        if (!$(this).has(e.relatedTarget).length) {
          $(this).removeClass('drag-over');
        }
      });

      $dropZone.on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');

        const files = e.originalEvent.dataTransfer.files;
        console.log('Files dropped:', files);
        if (files.length > 0) {
          const file = files[0];
          if (!file.name.match(/\.(xlsx|xls)$/i)) {
            abp.notify.error('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
            return;
          }

          const dataTransfer = new DataTransfer();
          dataTransfer.items.add(file);
          $fileInput[0].files = dataTransfer.files;
          $fileInput.trigger('change');
        }
      });

      // Upload button - SỬ DỤNG self thay vì this
      $modal.find('.btn-import').on('click', function () {
        self.submitImport();
      });

      // Download template - SỬ DỤNG self thay vì this
      $modal.find('.btn-download-template').on('click', function () {
        self.downloadTemplate();
      });

      // Close modal reset
      $modal.on('hidden.bs.modal', function () {
        self.resetForm($modal);
      });
    };
  };
})(jQuery);