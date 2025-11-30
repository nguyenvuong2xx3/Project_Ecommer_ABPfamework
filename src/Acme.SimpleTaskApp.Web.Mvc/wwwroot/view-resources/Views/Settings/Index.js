(function ($) {
  var _settingService = abp.services.app.setting;

  // Initialize form
  function initializeForm() {
    // Toggle password visibility
    $('.toggle-password').click(function () {
      var passwordInput = $('#Password');
      var icon = $(this).find('i');

      if (passwordInput.attr('type') === 'password') {
        passwordInput.attr('type', 'text');
        icon.removeClass('fa-eye').addClass('fa-eye-slash');
      } else {
        passwordInput.attr('type', 'password');
        icon.removeClass('fa-eye-slash').addClass('fa-eye');
      }
    });

    // Save SMTP settings
    $('#SaveSmtpButton').click(function () {
      saveSmtpSettings();
    });

    // Send test email
    $('#SendTestEmailBtn').click(function () {
      sendTestEmail();
    });

    // Test SMTP connection
    $('#TestSmtpButton').click(function () {
      testSmtpConnection();
    });

    // Save Store settings
    $('#SaveStoreButton').click(function () {
      saveStoreSettings();
    });

    // Preview logo when file is selected
    $('#LogoFile').on('change', function(e) {
      var file = e.target.files[0];
      if (file) {
        // Update custom file input label
        var fileName = file.name;
        $(this).next('.custom-file-label').html(fileName);
        
        // Preview image
        var reader = new FileReader();
        reader.onload = function(e) {
          $('#LogoPreview').attr('src', e.target.result);
        };
        reader.readAsDataURL(file);
      }
    });

    // Preview logo when URL changes
    $('#UrlLogo').on('input', function() {
      var url = $(this).val();
      if (url && url.trim() !== '') {
        updateLogoPreview(url);
      }
    });

    // Enter key support
    $('#SmtpSettingsForm input').keypress(function (e) {
      if (e.which === 13) {
        saveSmtpSettings();
      }
    });

    $('#StoreSettingsForm input[type="text"]').keypress(function (e) {
      if (e.which === 13) {
        saveStoreSettings();
      }
    });
  }

  // Save Store settings with file upload
  function saveStoreSettings() {
    var form = $('#StoreSettingsForm')[0];

    if (!form.checkValidity()) {
      $(form).addClass('was-validated');
      abp.notify.warn('Vui lòng điền đầy đủ thông tin bắt buộc!');
      return;
    }

    // Validate: Must have either file upload or URL
    var logoFile = $('#LogoFile')[0].files[0];
    var urlLogo = $('#UrlLogo').val();
    
    if (!logoFile && !urlLogo) {
      abp.notify.warn('Vui lòng upload logo hoặc nhập URL logo!');
      return;
    }

    // Create FormData to handle file upload
    var formData = new FormData();
    formData.append('NameStore', $('#NameStore').val());
    
    // Add logo file if selected
    if (logoFile) {
      formData.append('LogoFile', logoFile);
    } else if (urlLogo) {
      // If no file, send URL
      formData.append('UrlLogo', urlLogo);
    }

    abp.ui.setBusy($('#StoreSettingsForm'));

    $.ajax({
      url: abp.appPath + 'Settings/UpdateStoreSettings',
      type: 'POST',
      data: formData,
      processData: false,  // Important for file upload
      contentType: false,  // Important for file upload
      success: function (response) {
        if (response.success) {
          abp.notify.success(response.message || 'Cập nhật thông tin cửa hàng thành công!');
          $(form).removeClass('was-validated');
          
          // Clear file input
          $('#LogoFile').val('');
          $('.custom-file-label').html('Chọn file ảnh...');
          
          // Reload page to update sidebar logo
          setTimeout(function() {
            location.reload();
          }, 1000);
        } else {
          abp.notify.error(response.message || 'Có lỗi xảy ra!');
        }
      },
      error: function (xhr, status, error) {
        var errorMessage = 'Không thể cập nhật thông tin cửa hàng!';
        if (xhr.responseJSON && xhr.responseJSON.error) {
          errorMessage = xhr.responseJSON.error.message || errorMessage;
        }
        abp.notify.error(errorMessage);
        console.error('Error:', error);
      },
      complete: function () {
        abp.ui.clearBusy($('#StoreSettingsForm'));
      }
    });
  }

  // Update logo preview
  function updateLogoPreview(url) {
    var $preview = $('#LogoPreview');
    var $previewContainer = $preview.parent();
    
    if (url && url.trim() !== '') {
      // Show image preview
      $previewContainer.html(`
        <img id="LogoPreview" src="${url}" 
             alt="Store Logo" class="img-thumbnail" 
             style="max-height: 150px; max-width: 300px;"
             onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
        <div style="display:none;" class="text-center py-5">
          <i class="fas fa-exclamation-triangle text-warning" style="font-size: 3rem;"></i>
          <p class="text-muted mt-3 mb-0">Không thể tải ảnh</p>
          <small class="text-muted">URL không hợp lệ hoặc file không tồn tại</small>
        </div>
      `);
    } else {
      // Show placeholder
      $previewContainer.html(`
        <div class="text-center py-5">
          <i class="fas fa-image text-muted" style="font-size: 4rem;"></i>
          <p class="text-muted mt-3 mb-0">Chưa có logo</p>
          <small class="text-muted">Vui lòng upload hoặc nhập URL logo</small>
        </div>
      `);
    }
  }

  // Save SMTP settings
  function saveSmtpSettings() {
    var form = $('#SmtpSettingsForm');

    if (!form[0].checkValidity()) {
      form.addClass('was-validated');
      return;
    }

    var smtpSettings = form.serializeFormToObject();

    abp.ui.setBusy($('#SmtpSettingsForm'));

    _settingService.updateMailSettings(smtpSettings)
      .done(function () {
        abp.notify.success('SMTP settings saved successfully!');
        form.removeClass('was-validated');
      })
      .always(function () {
        abp.ui.clearBusy($('#SmtpSettingsForm'));
      });
  }

  // Test SMTP connection
  function testSmtpConnection() {
    var form = $('#SmtpSettingsForm');

    if (!form[0].checkValidity()) {
      form.addClass('was-validated');
      return;
    }

    var smtpSettings = form.serializeFormToObject();

    abp.ui.setBusy($('#SmtpSettingsForm'));

    _settingService.testConnection(smtpSettings)
      .done(function (result) {
        showTestResult(result.success, result.message || 'SMTP connection test completed successfully!');
      })
      .always(function () {
        abp.ui.clearBusy($('#SmtpSettingsForm'));
      });
  }

  // Send test email
  function sendTestEmail() {
    var testEmail = $('#TestEmail').val();

    if (!testEmail) {
      abp.notify.warn('Please enter a test email address.');
      return;
    }

    if (!isValidEmail(testEmail)) {
      abp.notify.warn('Please enter a valid email address.');
      return;
    }

    var form = $('#SmtpSettingsForm');
    var smtpSettings = form.serializeFormToObject();
    smtpSettings.testEmailAddress = testEmail;

    abp.ui.setBusy($('#SmtpSettingsForm'));

    _settingService.sendTestEmail(smtpSettings)
      .done(function (result) {
        showTestResult(result.success, result.message || 'Test email sent successfully!');
      })
      .always(function () {
        abp.ui.clearBusy($('#SmtpSettingsForm'));
      });
  }

  // Show test result in modal
  function showTestResult(success, message) {
    var resultContent = $('#TestResultContent');
    var icon = success ? 'fa-check-circle text-success' : 'fa-times-circle text-danger';

    resultContent.html(`
      <div class="text-center">
        <i class="fa ${icon} fa-3x mb-3"></i>
        <h4>${success ? 'Success' : 'Error'}</h4>
        <p class="mb-0">${message}</p>
      </div>
    `);

    $('#TestResultModal').modal('show');
  }

  // Email validation
  function isValidEmail(email) {
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  // Initialize when document is ready
  $(document).ready(function () {
    initializeForm();
  });

})(jQuery);
