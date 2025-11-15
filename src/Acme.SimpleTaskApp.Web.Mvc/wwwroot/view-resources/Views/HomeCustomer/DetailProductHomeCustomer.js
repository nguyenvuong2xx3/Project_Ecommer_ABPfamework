$(document).ready(function () {
  let currentImageIndex = 0;
  let allImages = [];
  let currentVariantId = 0;

  // Khởi tạo
  function init() {
    currentVariantId = $('#current-variant-id').val();
    loadAllImages();
    bindEvents();
  }

  // Lấy tất cả ảnh từ các variants hiện có trên trang
  function loadAllImages() {
    allImages = [];
    $('.color-option img').each(function () {
      const src = $(this).attr('src');
      if (src && !allImages.includes(src)) {
        allImages.push(src);
      }
    });
  }
  $(document).ready(function () {
    initializeVariantEvents();
  });

  function initializeVariantEvents() {
    // Gán sự kiện click cho tất cả các phần tử có thuộc tính data-variant-id
    $('[data-variant-id]').on('click', function () {
      const variantId = $(this).data('variant-id');
      changeVariant(variantId);
    });
  }

  function changeVariant(variantId) {
    // Gọi controller DetailProductCustomer với variantId
    window.location.href = '/HomeCustomer/DetailProductCustomer?id=' + variantId;
  }

  // Bind tất cả events
  function bindEvents() {
    // Color option click
    $('.color-option').on('click', function () {
      const variantId = $(this).data('variant-id');
      if (variantId && variantId != currentVariantId) {
        selectColor(variantId);
      }
    });

    // Storage option click
    $('.storage-option').on('click', function () {
      const variantId = $(this).data('variant-id');
      if (variantId && variantId != currentVariantId) {
        selectColor(variantId);
      }
    });

    // Gallery navigation
    $('.gallery-nav-prev').on('click', function () {
      changeImage(-1);
    });

    $('.gallery-nav-next').on('click', function () {
      changeImage(1);
    });

    // Keyboard navigation
    $(document).on('keydown', function (e) {
      if (e.key === 'ArrowLeft') {
        changeImage(-1);
      } else if (e.key === 'ArrowRight') {
        changeImage(1);
      }
    });

    // Add to cart
    //$('.add-to-cart').on('click', function () {
    //  addToCart(currentVariantId);
    //});

    // Buy now
    $('.buy-now').on('click', function () {
      buyNow(currentVariantId);
    });
  }

  // Chuyển ảnh trong gallery
  function changeImage(direction) {
    if (allImages.length === 0) return;

    currentImageIndex += direction;

    if (currentImageIndex < 0) {
      currentImageIndex = allImages.length - 1;
    } else if (currentImageIndex >= allImages.length) {
      currentImageIndex = 0;
    }

    const mainImage = $('#main-product-image');
    const imageCounter = $('#image-counter');

    // Hiệu ứng fade
    mainImage.fadeOut(200, function () {
      $(this).attr('src', allImages[currentImageIndex]).fadeIn(200);
    });

    imageCounter.text(`Xem tất cả Hình (${currentImageIndex + 1}/${allImages.length})`);
  }

  // Chọn màu sản phẩm
  function selectColor(variantId) {
    showLoading();

    abp.services.app.homeCustomer.getProductById(variantId)
      .done(function (result) {
        updateProductView(result);
        currentVariantId = variantId;
        hideLoading();
      })
      .fail(function (error) {
        console.error('Error loading product:', error);
        abp.notify.error('Có lỗi xảy ra khi tải thông tin sản phẩm');
        hideLoading();
      });
  }

  // Cập nhật view với dữ liệu mới
  function updateProductView(productData) {
    // Cập nhật tiêu đề
    $('.product-title').text(
      `${productData.name} ${productData.productVariant.ram} ${productData.productVariant.storage} ${productData.productVariant.color}`
    );

    // Cập nhật ảnh chính
    const mainImage = $('#main-product-image');
    if (productData.imageUrls && productData.imageUrls.length > 0) {
      mainImage.fadeOut(200, function () {
        $(this).attr('src', productData.imageUrls[0]).fadeIn(200);
      });
      currentImageIndex = 0;
      allImages = productData.imageUrls;
      $('#image-counter').text(`Xem tất cả Hình (1/${allImages.length})`);
    }

    // Cập nhật small color options
    updateSmallColorOptions(productData.productVariants);

    // Cập nhật storage options
    updateStorageOptions(productData.productVariants);

    // Cập nhật large color options
    updateLargeColorOptions(productData.productVariants);

    // Cập nhật giá
    updatePrice(productData.productVariant.price);

    // Cập nhật active state
    updateActiveState(productData.productVariant.id);

    // Cập nhật thông tin sản phẩm trong tabs (nếu có)
    updateProductInfo(productData);
  }

  // Cập nhật small color options
  function updateSmallColorOptions(variants) {
    const container = $('.small-color-options');
    container.empty();

    variants.forEach(function (variant) {
      const firstImage = variant.imageUrls && variant.imageUrls.length > 0 ? variant.imageUrls[0] : '';
      const isActive = variant.id === currentVariantId ? 'border-primary' : '';

      const optionHtml = `
                <div class="color-option border border-2 rounded p-2 text-center d-flex flex-column align-items-center ${isActive}"
                     style="width: 60px; margin-right: 10px; border-radius: 7px; cursor: pointer;"
                     data-variant-id="${variant.id}"
                     title="${variant.color}">
                    <img src="${firstImage}" style="height: 45px; object-fit: contain;">
                    <span class="small mt-1">${variant.color}</span>
                </div>
            `;
      container.append(optionHtml);
    });
  }

  // Cập nhật storage options
  function updateStorageOptions(variants) {
    const container = $('.storage-options');
    container.empty();

    variants.forEach(function (variant) {
      const isChecked = variant.id === currentVariantId ? 'checked' : '';

      const optionHtml = `
                <input type="radio" class="btn-check storage-option" name="storage" 
                       id="storage${variant.id}" ${isChecked} data-variant-id="${variant.id}">
                <label class="btn btn-outline-primary" for="storage${variant.id}" 
                       data-variant-id="${variant.id}">
                    ${variant.storage}
                </label>
            `;
      container.append(optionHtml);
    });
  }

  // Cập nhật large color options
  function updateLargeColorOptions(variants) {
    const container = $('.large-color-options');
    container.empty();

    variants.forEach(function (variant) {
      const firstImage = variant.imageUrls && variant.imageUrls.length > 0 ? variant.imageUrls[0] : '';
      const isActive = variant.id === currentVariantId ? 'border-primary' : '';

      const optionHtml = `
                <div class="color-option border border-2 rounded-3 p-2 d-flex align-items-center ${isActive}"
                     style="min-width: 150px; border-radius: 10px; margin-right: 10px; cursor: pointer;"
                     data-variant-id="${variant.id}">
                    <img src="${firstImage}" style="height: 45px; width: 45px; object-fit: contain; border-radius: 5px; margin-right: 10px">
                    <div class="ms-2 d-flex flex-column align-items-start">
                        <span class="small fw-semibold">${variant.color}</span>
                        <span class="small text-muted">${formatPrice(variant.price)}</span>
                    </div>
                </div>
            `;
      container.append(optionHtml);
    });
  }

  // Cập nhật giá
  function updatePrice(price) {
    $('.current-price').text(formatPrice(price));
  }

  // Cập nhật active state
  function updateActiveState(activeVariantId) {
    $('.color-option').removeClass('border-primary');
    $(`.color-option[data-variant-id="${activeVariantId}"]`).addClass('border-primary');

    $('.storage-option').prop('checked', false);
    $(`#storage${activeVariantId}`).prop('checked', true);
  }

  // Cập nhật thông tin sản phẩm
  function updateProductInfo(productData) {
    // Cập nhật thông số kỹ thuật nếu cần
    // $('#specs-content').html(productData.specifications);

    // Cập nhật thông tin sản phẩm nếu cần
    // $('#info-content').html(productData.description);
  }

  // Định dạng giá
  function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + '₫';
  }

  // Mua ngay
  function buyNow(variantId) {
    abp.services.app.carts.addToCart(variantId, 1)
      .done(function () {
        window.location.href = '/Carts/Checkout';
      })
      .fail(function (error) {
        abp.notify.error('Có lỗi khi thêm vào giỏ hàng');
      });
  }

  // Hiển thị loading
  function showLoading() {
    $('#loading-overlay').removeClass('d-none');
  }

  // Ẩn loading
  function hideLoading() {
    $('#loading-overlay').addClass('d-none');
  }

  // Khởi tạo
  init();
});