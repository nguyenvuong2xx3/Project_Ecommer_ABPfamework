(function ($) {
  app.modals.BannerCreateModal = function () {
    var _modalManager;
    var _$form = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$form = $modal.find('form[name=BannerCreateForm]');

      // Enhanced validation
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
            digits: true,
            min: 0
          },
          BannerImage: {
            required: true
          }
        },
        messages: {
          Title: {
            required: "Vui lòng nhập tiêu đề banner",
            minlength: "Tiêu đề phải có ít nhất 3 ký tự",
            maxlength: "Tiêu đề tối đa 200 ký tự"
          },
          Position: {
            required: "Vui lòng chọn vị trí hiển thị"
          },
          SortOrder: {
            required: "Vui lòng nhập thứ tự hiển thị",
            digits: "Thứ tự phải là số nguyên",
            min: "Thứ tự phải >= 1"
          },
          BannerImage: {
            required: "Vui lòng chọn ảnh banner"
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

      // Image preview
      $('#BannerImage').on('change', function (e) {
        previewImage(this);
      });

      // Delete image preview
      $('#deleteBannerImageBtn').on('click', function () {
        resetImagePreview();
      });
    };

    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      var formData = new FormData(_$form[0]);

      for (var pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
      }

      const sortOrderText = formData.get("SortOrder");
      const sortOrderInt = parseInt(sortOrderText, 10);
      formData.set("SortOrder", isNaN(sortOrderInt) ? 0 : sortOrderInt);

      const isActive = $("#IsActive").is(":checked");
      formData.set("IsActive", isActive);

      _modalManager.setBusy(true);
      $('#error-message').hide();

      $.ajax({
        url: abp.appPath + 'Banners/Create',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
          _modalManager.close();
          abp.notify.success('Tạo banner thành công!');
          abp.event.trigger('banner.created', response);
        },
        error: function (xhr) {
          var errorMessage;
          if (xhr.responseJSON && xhr.responseJSON.error) {
            errorMessage = xhr.responseJSON.error.message;
          } else if (xhr.responseText) {
            errorMessage = xhr.responseText;
          } else {
            errorMessage = "Có lỗi xảy ra khi tạo banner. Vui lòng kiểm tra lại thông tin.";
          }
          $('#error-message').html(errorMessage).show();
        },
        complete: function () {
          _modalManager.setBusy(false);
        }
      });
    };

    function previewImage(input) {
      const file = input.files[0];
      if (file) {
        const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
        if (!validTypes.includes(file.type)) {
          abp.notify.error('Vui lòng chọn file ảnh hợp lệ (JPG, PNG, GIF, WEBP)');
          resetImagePreview();
          return;
        }

        if (file.size > 5 * 1024 * 1024) {
          abp.notify.error('Kích thước ảnh không được vượt quá 5MB');
          resetImagePreview();
          return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
          $('#bannerImagePreview').attr('src', e.target.result);
          $('#bannerImagePreviewContainer').show();
        };
        reader.readAsDataURL(file);

        const fileName = file.name;
        $(input).next('.custom-file-label').html(fileName);
      }
    }

    function resetImagePreview() {
      $('#BannerImage').val('');
      $('#bannerImagePreview').attr('src', '');
      $('#bannerImagePreviewContainer').hide();
      $('.custom-file-label').html('Chọn ảnh banner...');
    }
  };
})(jQuery);
