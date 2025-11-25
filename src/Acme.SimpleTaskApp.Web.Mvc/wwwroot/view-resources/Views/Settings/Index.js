(function ($) {
  var _settingService = abp.services.app.setting;
  var _modalManager;

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

    // Enter key support
    $('#SmtpSettingsForm input').keypress(function (e) {
      if (e.which === 13) {
        saveSmtpSettings();
      }
    });
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
