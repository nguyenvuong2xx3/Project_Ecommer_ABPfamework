(function ($) {
  // --- KHAI BÁO BIẾN CHO MODAL ---
  var l = abp.localization.getSource('SimpleTaskApp');
  var _$form = $('#productCreateForm'); // Sửa lại selector form
  var _$modal = $('#ProductsCreateModal'); // Đảm bảo modal có ID này

  // --- LOGIC XỬ LÝ UPLOAD HÌNH ẢNH ---
  let imageFiles = []; // Mảng tạm để lưu các file ảnh đã chọn

  function setupImageUploader() {
    const imageUploader = $('.image-uploader')[0];
    const imageInput = $('#ImageFiles')[0];
    const previewContainer = $('#imagePreviewContainer')[0];

    if (!imageUploader) return;

    $(imageUploader).on('click', function () {
      $(imageInput).click();
    });

    $(imageUploader).on('dragover', function (e) {
      e.preventDefault();
      $(this).css('background-color', '#e9ecef');
    });

    $(imageUploader).on('dragleave', function (e) {
      e.preventDefault();
      $(this).css('background-color', 'transparent');
    });

    $(imageUploader).on('drop', function (e) {
      e.preventDefault();
      $(this).css('background-color', 'transparent');
      const files = e.originalEvent.dataTransfer.files;
      handleFiles(files);
    });

    $(imageInput).on('change', function (e) {
      handleFiles(e.target.files);
    });
  }

  function handleFiles(files) {
    const previewContainer = $('#imagePreviewContainer')[0];

    for (const file of files) {
      if (!file.type.startsWith('image/')) continue;
      imageFiles.push(file);
      const reader = new FileReader();
      reader.onload = function (e) {
        const preview = createPreviewElement(e.target.result, file);
        previewContainer.appendChild(preview);
      };
      reader.readAsDataURL(file);
    }
  }

  function createPreviewElement(src, file) {
    const wrapper = document.createElement('div');
    wrapper.className = 'img-preview-wrapper';

    const img = document.createElement('img');
    img.src = src;
    img.className = 'img-preview';

    const removeBtn = document.createElement('button');
    removeBtn.innerHTML = '&times;';
    removeBtn.className = 'remove-img-btn';
    removeBtn.type = 'button';
    removeBtn.onclick = function () {
      imageFiles = imageFiles.filter(f => f !== file);
      wrapper.remove();
    };

    wrapper.appendChild(img);
    wrapper.appendChild(removeBtn);
    return wrapper;
  }

  // --- LOGIC XỬ LÝ TẠO BIẾN THỂ ---
  function setupVariantGenerator() {
    const colorInput = $('input[name=option_color]')[0];
    const storageInput = $('input[name=option_storage]')[0];
    const ramInput = $('input[name=option_ram]')[0];

    // Khởi tạo Tagify cho các input thuộc tính
    const tagifyColor = new Tagify(colorInput);
    const tagifyStorage = new Tagify(storageInput);
    const tagifyRam = new Tagify(ramInput);

    $('#generateVariantsBtn').on('click', function () {
      console.log('Generating variants...');
      const colors = tagifyColor.value.map(tag => tag.value);
      const storages = tagifyStorage.value.map(tag => tag.value);
      const rams = tagifyRam.value.map(tag => tag.value);

      const options = [];
      if (colors.length) options.push(colors);
      if (storages.length) options.push(storages);
      if (rams.length) options.push(rams);

      if (options.length === 0) {
        abp.notify.warn('Vui lòng nhập ít nhất một thuộc tính (màu, dung lượng, hoặc RAM).');
        return;
      }

      // Hàm nhân các mảng thuộc tính để tạo biến thể
      const cartesian = (...a) => a.reduce((acc, val) => acc.flatMap(d => val.map(e => [d, e].flat())));
      const variants = cartesian(...options);

      const variantsTableBody = $('#variantsTable tbody');
      variantsTableBody.empty(); // Xóa các dòng cũ

      if (variants.length === 0) {
        variantsTableBody.html(`<tr><td colspan="5" class="text-center">Không có biến thể nào được tạo.</td></tr>`);
        return;
      }

      variants.forEach((variant, index) => {
        const variantName = Array.isArray(variant) ? variant.join(' / ') : variant;

        // Xác định giá trị cho từng thuộc tính
        const color = colors.length ? variant[0] : '';
        const storage = storages.length ? variant[colors.length ? 1 : 0] : '';
        const ram = rams.length ? variant[colors.length + storages.length ? 2 : (colors.length ? 1 : 0)] : '';

        const row = `
                    <tr class="variant-row">
                        <td>
                            <strong>${variantName}</strong>
                            <input type="hidden" name="Variants[${index}].Color" value="${color || ''}" />
                            <input type="hidden" name="Variants[${index}].Storage" value="${storage || ''}" />
                            <input type="hidden" name="Variants[${index}].Ram" value="${ram || ''}" />
                        </td>
                        <td>
                            <div class="input-group">
                                <input type="number" name="Variants[${index}].Price" class="form-control" placeholder="Giá" required min="0">
                                <div class="input-group-append"><span class="input-group-text">VNĐ</span></div>
                            </div>
                        </td>
                        <td><input type="number" name="Variants[${index}].StockQuantity" class="form-control" placeholder="Số lượng" required min="0"></td>
                        <td><input type="text" name="Variants[${index}].SKU" class="form-control" placeholder="Để trống tự sinh"></td>
                        <td><button type="button" class="btn btn-danger btn-sm remove-variant-row"><i class="fas fa-trash"></i></button></td>
                    </tr>
                `;
        variantsTableBody.append(row);
      });
    });

    // Sự kiện xóa một dòng biến thể
    $('#variantsTable').on('click', '.remove-variant-row', function () {
      $(this).closest('tr').remove();
    });
  }

  // --- XỬ LÝ SUBMIT FORM ---
  _$form.on('submit', function (e) {
    e.preventDefault();

    if (!_$form.valid()) {
      return;
    }

    // Sử dụng FormData để gửi cả file và dữ liệu form
    var formData = new FormData(_$form[0]);

    // Thêm các file ảnh đã chọn vào FormData
    imageFiles.forEach((file, index) => {
      formData.append('ImageFiles', file);
    });

    abp.ui.setBusy(_$form);

    // Gọi service bằng AJAX
    $.ajax({
      url: _$form.attr('action'), // Lấy URL từ action của form
      type: 'POST',
      data: formData,
      processData: false, // Cần thiết cho FormData
      contentType: false, // Cần thiết cho FormData
      success: function (result) {
        abp.notify.info(l('SavedSuccessfully'));
        // Chuyển hướng về trang danh sách
        window.location.href = abp.appPath + 'Products/Index';
      },
      error: function (err) {
        console.error(err);
        abp.notify.error('Có lỗi xảy ra khi tạo sản phẩm');
      },
      complete: function () {
        abp.ui.clearBusy(_$form);
      }
    });
  });

  // --- KHỞI TẠO KHI TRANG ĐƯỢC TẢI ---
  $(document).ready(function () {
    console.log('Product create page loaded');

    // Reset form và các thành phần
    _$form[0].reset();
    $('#imagePreviewContainer').html('');
    imageFiles = [];
    $('#variantsTable tbody').html(`
            <tr id="variant-placeholder">
                <td colspan="5" class="text-center text-muted p-4">
                    <p>Chưa có biến thể nào.</p>
                    <small>Hãy nhập các thuộc tính ở trên và nhấn nút "Tạo biến thể".</small>
                </td>
            </tr>
        `);

    // Khởi tạo các thành phần
    setupImageUploader();
    setupVariantGenerator();

    // Khởi tạo validation nếu cần
    if ($.fn.validate) {
      _$form.validate();
    }
  });

})(jQuery);