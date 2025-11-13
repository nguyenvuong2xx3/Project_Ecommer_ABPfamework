(function ($) {
  app.modals.BannerEditModal = function () {
    var _modalManager;
    var _$form = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$form = $modal.find('form[name=BannerEditForm]');

      // Image preview
      $('#BannerImage').on('change', function (e) {
        previewNewImage(this);
      });

      // Delete new image preview
      $('#deleteNewBannerImageBtn').on('click', function () {
        resetNewImagePreview();
      });

      // Form validation
      _$form.validate({
        rules: {
          Title: {
            required: true,
            minlength: 3,
            maxlength: 200
          },
          Position: {
            required: true
          },
          SortOrder: {
            required: true,
            number: true,
            min: 0
          }
        },
        messages: {
          Title: {
            required: "Vui lòng nh?p tiêu ?? banner",
            minlength: "Tiêu ?? ph?i có ít nh?t 3 ký t?",
            maxlength: "Tiêu ?? t?i ?a 200 ký t?"
          },
          Position: {
            required: "Vui lòng ch?n v? trí hi?n th?"
          },
          SortOrder: {
            required: "Vui lòng nh?p th? t? hi?n th?",
            number: "Th? t? ph?i là s?",
            min: "Th? t? ph?i >= 0"
          }
        },
        errorElement: "div",
        errorClass: "invalid-feedback",
        highlight: function (element) {
          $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
          $(element).removeClass('is-invalid');
        }
      });

      // Submit form
      _$form.on('submit', function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
          return;
        }

        var formData = new FormData(_$form[0]);

        const sortOrderText = formData.get("SortOrder");
        const sortOrderInt = parseInt(sortOrderText, 10);
        formData.set("SortOrder", isNaN(sortOrderInt) ? 0 : sortOrderInt);

        const isActive = $("#IsActive").is(":checked");
        formData.set("IsActive", isActive);

        _modalManager.setBusy(true);
        $('#error-message').hide();

        $.ajax({
          url: abp.appPath + 'Banners/Edit',
          type: 'POST',
          data: formData,
          processData: false,
          contentType: false,
          success: function (response) {
            _modalManager.close();
            abp.notify.success('C?p nh?t banner thành công!');
            abp.event.trigger('banner.edited', response);
          },
          error: function (xhr) {
            var errorMessage;
            if (xhr.responseJSON && xhr.responseJSON.error) {
              errorMessage = xhr.responseJSON.error.message;
            } else if (xhr.responseText) {
              errorMessage = xhr.responseText;
            } else {
              errorMessage = "Có l?i x?y ra khi c?p nh?t banner. Vui lòng ki?m tra l?i thông tin.";
            }
            $('#error-message').html(errorMessage).show();
          },
          complete: function () {
            _modalManager.setBusy(false);
          }
        });
      });
    };

    function previewNewImage(input) {
      const file = input.files[0];
      if (file) {
        // Validate file type
        const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
        if (!validTypes.includes(file.type)) {
          abp.notify.error('Vui lòng ch?n file ?nh h?p l? (JPG, PNG, GIF, WEBP)');
          resetNewImagePreview();
          return;
        }

        // Validate file size (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
          abp.notify.error('Kích th??c ?nh không ???c v??t quá 5MB');
          resetNewImagePreview();
          return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
          $('#newBannerImagePreview').attr('src', e.target.result);
          $('#newBannerImagePreviewContainer').show();
        };
        reader.readAsDataURL(file);

        // Update label
        const fileName = file.name;
        $(input).next('.custom-file-label').html(fileName);
      }
    }

    function resetNewImagePreview() {
      $('#BannerImage').val('');
      $('#newBannerImagePreview').attr('src', '');
      $('#newBannerImagePreviewContainer').hide();
      $('.custom-file-label').html('Ch?n ?nh m?i...');
    }
  };
})(jQuery);
