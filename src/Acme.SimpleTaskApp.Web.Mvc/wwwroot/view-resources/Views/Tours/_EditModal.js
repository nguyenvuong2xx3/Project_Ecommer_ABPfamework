(function ($) {
  $(document).on('shown.bs.modal', '#TourEditModal', function () {
    var _$modal = $(this);
    var _$form = _$modal.find('form');
    var _$tourTable = $('#TourTable').DataTable();

    // Xử lý sự kiện click nút Save
    _$form.off('click', '.save-button').on('click', '.save-button', function (e) {
      e.preventDefault();
      if (!_$form.valid()) return;

      var formData = new FormData(_$form[0]);
      abp.ui.setBusy(_$modal);

      $.ajax({
        url: abp.appPath + 'Tours/UpdateTour',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
      })
        .done(function (response) {
          if (response.success) {
            abp.notify.success(response.message);
            _$modal.modal('hide');
            _$tourTable.ajax.reload();
          } else {
            abp.notify.error(response.message);
          }
        })
        .fail(function (xhr) {
          var errorMessage = xhr.responseJSON?.error?.message || "Lỗi không xác định";
          abp.notify.error(errorMessage);
        })
        .always(function () {
          abp.ui.clearBusy(_$modal);
        });
    });
  });

  //$(document).on('click', '.remove-image', function () {

  //  removeImage();
  //});


  //  function removeImage() {
  //    // Xóa nội dung của input file
  //    document.getElementById('tourAttachment').value = "";
  //  // Ẩn vùng hiển thị ảnh
  //    const previewContainer = document.getElementById('imagePreview');
  //  previewContainer.classList.add('d-none');
  //  }



  //validate
  $(document).ready(function () {
    $("form[name='TourEditForm']").validate({
      rules: {
        TourName: {
          required: true,
          minlength: 3,
          maxlength: 100
        },
        MinGroupSize: {
          required: true,
          digits: true,
          min: 1
        },
        MaxGroupSize: {
          required: true,
          digits: true,
          greaterThan: "input[name='MinGroupSize']"
        },
        TourTypeCid: {
          required: true,
          range: [0, 2]
        },
        StartDate: {
          required: true,
          date: true
        },
        EndDate: {
          required: true,
          date: true,
          greaterThan: "input[name='StartDate']"
        },
        Transportation: {
          required: true
        },
        TourPrice: {
          required: true,
          number: true,
          min: 1
        },
        PhoneNumber: {
          required: true,
          regex: /^(0[2-9]|84[2-9])[0-9]{8}$/
        },
        Description: {
          maxlength: 500
        },
        ImageFile: {
          required: false,
          extension: "jpg|jpeg|png|gif"
        }
      },
      messages: {
        TourName: {
          required: "Tên tour không được để trống",
          minlength: "Tên tour phải có ít nhất 3 ký tự",
          maxlength: "Tên tour tối đa 100 ký tự"
        },
        MinGroupSize: {
          required: "Vui lòng nhập số lượng nhóm tối thiểu",
          digits: "Phải là số nguyên",
          min: "Phải lớn hơn 0"
        },
        MaxGroupSize: {
          required: "Vui lòng nhập số lượng nhóm tối đa",
          digits: "Phải là số nguyên",
          greaterThan: "Số lượng nhóm tối đa phải lớn hơn số lượng nhóm tối thiểu"
        },
        TourTypeCid: {
          required: "Vui lòng chọn loại tour",
          range: "Tour không hợp lệ vui lòng chọn 1 trong 3 Tour"
        },
        StartDate: {
          required: "Vui lòng chọn ngày bắt đầu",
          date: "Định dạng ngày không hợp lệ"
        },
        EndDate: {
          required: "Vui lòng chọn ngày kết thúc",
          date: "Định dạng ngày không hợp lệ",
          greaterThan: "Ngày kết thúc phải lớn hơn ngày bắt đầu"
        },
        Transportation: {
          required: "Vui lòng nhập phương tiện di chuyển"
        },
        TourPrice: {
          required: "Vui lòng nhập giá tour",
          number: "Phải là số",
          min: "Giá tour phải lớn hơn 0"
        },
        PhoneNumber: {
          required: "Vui lòng nhập số điện thoại",
          regex: "Số điện thoại không hợp lệ"
        },
        Description: {
          maxlength: "Mô tả không được quá 500 ký tự"
        },
        ImageFile: {
          extension: "Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif"
        }
      },
      errorElement: "div",
      errorClass: "text-danger"
    });
  });
})(jQuery);