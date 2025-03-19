(function ($) {
  var _tourService = abp.services.app.tour,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$modal = $('#TourCreateModal'),
    _$form = _$modal.find('form'),
    _$table = $('#TourTable');

  var _$tourTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _tourService.searchTour,
      inputFilter: function () {
        return $('#TourSearchForm').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$tourTable.draw(false)
      }
    ],
    responsive: {
      details: {
        type: 'column'
      }
    },
    columnDefs: [
      // 1. TourName
      {
        targets: 0,
        data: 'tourName',
        sortable: false,
        render: (data) => data ? data : '<span class="text-muted">Không có tên</span>'
      },
      // 2. MinGroupSize
      {
        targets: 1,
        data: 'minGroupSize',
        sortable: false,
        render: (data) => data ? data : '<span class="text-muted">Chưa xác định</span>'
      },
      // 3. MaxGroupSize
      {
        targets: 2,
        data: 'maxGroupSize',
        sortable: false,
        render: (data) => data ? data : '<span class="text-muted">Chưa xác định</span>'
      },
      // 4. TourTypeCid
      {
        targets: 3,
        data: 'tourTypeCid',
        sortable: false,
        render: function (data, type, row) {
          switch (data) {
            case 0: return '<span class="badge bg-success">Tour du lịch nội tỉnh</span>';
            case 1: return '<span class="badge bg-warning">Tour du lịch liên tỉnh</span>';
            case 2: return '<span class="badge bg-warning">Tour du lịch quốc tế </span>';
            default:
              return '<span class="text-muted">Chưa xác định</span>';
          }
        }
      },
      // 5. StartDate
      {
        targets: 4,
        data: 'startDate',
        sortable: false,
        render: (data) => data ? new Date(data).toLocaleDateString('vi-VN') : '<span class="text-muted">Chưa xác định</span>'
      },
      // 6. EndDate
      {
        targets: 5,
        data: 'endDate',
        sortable: false,
        render: (data) => data ? new Date(data).toLocaleDateString('vi-VN') : '<span class="text-muted">Chưa xác định</span>'
      },
      // 7. Transportation
      {
        targets: 6,
        data: 'transportation',
        sortable: false,
        render: (data) => data ? `<span class="badge bg-info">${data}</span>` : '<span class="text-muted">Chưa xác định</span>'
      },
      // 8. TourPrice
      {
        targets: 7,
        data: 'tourPrice',
        sortable: false,
        render: (data) => data ? `${data.toLocaleString('vi-VN')} VND` : '<span class="text-muted">Chưa có giá</span>'
      },
      // 9. PhoneNumber
      {
        targets: 8,
        data: 'phoneNumber',
        sortable: false,
        render: (data) => data ? data : '<span class="text-muted">Chưa có số điện thoại</span>'
      },
      // 10. Description
      {
        targets: 9,
        data: 'description',
        sortable: false,
        render: (data) => data ? data : '<span class="text-muted">Không có mô tả</span>'
      },
      // 11. Attachment (Hình ảnh)
      {
        targets: 10,
        data: 'attachment',
        sortable: false,
        render: function (data) {
          if (data) {
            return `<img src="${data}" alt="Hình ảnh tour" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
          }
          return '<span class="text-muted">Không có ảnh</span>';
        }
      },
      // 12. Actions (Các hành động)
      {
        targets: 11,
        data: null,
        sortable: false,
        autoWidth: true,
        defaultContent: '',
        render: (data, type, row) => {
          return [
            `<button type="button" class="btn btn-sm bg-secondary edit-tour" data-tour-id="${row.id}" data-toggle="modal" data-target="#TourEditModal">`,
            `   <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
            `</button>`,
            `<button type="button" class="btn btn-sm bg-danger delete-tour" data-tour-id="${row.id}" data-tour-name="${row.tourName}">`,
            `   <i class="fas fa-trash"></i> ${l('Delete')}`,
            `</button>`,
            `<button type="button" class="btn btn-sm bg-info detail-tour" data-tour-id="${row.id}" data-toggle="modal">`,
            `   <i class="fas fa-eye"></i> ${l('Details')}`,
            `</button>`
          ].join(' ');
        }
      }
    ]
  });


  //refresh
  $(document).on('click', '.buttons-refresh', function () {
    _$tourTable.ajax.reload();
  });
  // Hiển thị ảnh khi thêm mới
  document.getElementById('tourImage').addEventListener('change', function (event) {
    previewImage(event);
  });

  function previewImage(event) {
    const input = event.target;
    const preview = document.getElementById('imagePreview');
    const deleteBtn = document.getElementById('deleteImageBtn');

    if (input.files && input.files[0]) {
      const reader = new FileReader();

      reader.onload = function (e) {
        preview.src = e.target.result;
        preview.style.display = 'block'; // Hiển thị ảnh xem trước
        deleteBtn.style.display = 'inline-block'; // Hiển thị nút xóa khi có ảnh
      };

      reader.readAsDataURL(input.files[0]);
    } else {
      preview.style.display = 'none';
      preview.src = '';
      deleteBtn.style.display = 'none';
    }
  }

  $(document).on('click', '#deleteImageBtn', function () {
    removeImage();
  });

  function removeImage() {
    const input = document.getElementById('tourImage');
    const preview = document.getElementById('imagePreview');
    const deleteBtn = document.getElementById('deleteImageBtn');

    // Xóa nội dung của input file
    input.value = '';

    // Ẩn ảnh xem trước
    preview.src = '';
    preview.style.display = 'none';

    // Ẩn nút xóa ảnh
    deleteBtn.style.display = 'none';
  }


  // lưu tour
  _$form.find('.save-button').on('click', (e) => {
    e.preventDefault();

    if (!_$form.valid()) {
      return;
    }

    var tour = _$form.serializeFormToObject(); // Lấy dữ liệu từ form

    var formData = new FormData(_$form[0]);

    abp.ui.setBusy(_$modal);
    $.ajax({

      url: abp.appPath + 'Tours/Create', // Đường dẫn đến phương thức trong controller
      type: 'POST',
      processData: false, // Important! Không xử lý dữ liệu
      contentType: false, // Important!  Không đặt kiểu dữ liệu
      data: formData,
      error: function (xhr, textStatus, errorThrown) {
        var errorMessage;
        if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
          errorMessage = xhr.responseJSON.errors.join("<br/>");
        }
        else {
          errorMessage = "Có lỗi xảy ra khi tạo mới sản phẩm (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
        }
        $("#error-message").html(errorMessage).show();
      }
    }).done(function () {
      /*resetDefaultImage();*/
      _$modal.modal('hide');
      _$form[0].reset();
      abp.notify.info(l('Lưu thành công'));
      _$tourTable.ajax.reload();

    }).always(function () {

      abp.ui.clearBusy(_$modal);

    });
  });


  /// xóa tour
  $(document).on('click', '.delete-tour', function () {
    var tourId = $(this).attr("data-tour-id");
    var tourName = $(this).attr('data-tour-name');

    deleteProduct(tourId, tourName);
  });
  function deleteProduct(tourId, tourName) {
    abp.message.confirm(
      abp.utils.formatString(
        l('Bạn có muốn xóa'),
        tourName),
      null,
      (isConfirmed) => {
        if (isConfirmed) {
          $.ajax({
            url: '/Tours/Delete',
            type: 'POST',
            data: { id: tourId }
          }).done(() => {
            abp.notify.info(l('Xoá thành công'));
            _$tourTable.ajax.reload();
          });
        }
      }
    );
  }

    // Xử lý sự kiện click nút "Edit"
  $(document).on('click', '.edit-tour', function () {
        var tourId = $(this).data("tour-id");

        abp.ajax({
          url: abp.appPath + 'Tours/EditTour?tourId=' + tourId,
            type: 'POST',
            processData: false, // Important! Không xử lý dữ liệu
            contentType: false, // Important!  Không đặt kiểu dữ liệu
            dataType: 'html',
            success: function (content) {
                // Chèn nội dung vào modal-content
                $('#TourEditModal .modal-content').html(content);
                // Hiển thị modal
                $('#TourEditModal').modal('show');
            },
            error: function (e) {
                abp.notify.error('Could not load edit form');
            }
        });
    });
    $('.btn-search').on('click', (e) => {
      _$tourTable.ajax.reload();
    });

   $('.txt-search').on('keypress', (e) => {  
       if (e.which == 13) {  
         _$tourTable.ajax.reload();  
           return false;  
       }  
   });


  $.validator.addMethod("greaterThan",
    function (value, element, param) {
      var target = $(param);
      if (this.settings.onfocusout) {
        target.unbind(".validate-greaterThan").bind("blur.validate-greaterThan", function () {
          $(element).valid();
        });
      }
      return value > target.val();
    },
    "Phải lớn hơn giá trị so sánh"
  );
  // Thêm phương thức extension vào jQuery.validate
  $.validator.addMethod("extension", function (value, element, param) {
    param = typeof param === "string" ? param.replace(/,/g, "|") : "png|jpe?g|gif";
    return this.optional(element) || value.match(new RegExp("\\.(" + param + ")$", "i"));
  }, "Chỉ cho phép các định dạng: jpg, jpeg, png, gif");



   //validate
  $(document).ready(function () {
    $("form[name='tourCreateForm']").validate({
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
          min: 1000
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
