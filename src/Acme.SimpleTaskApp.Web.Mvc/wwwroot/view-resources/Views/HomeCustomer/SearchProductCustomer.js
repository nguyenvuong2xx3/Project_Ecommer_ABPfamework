// SearchProductCustomer.js
(function ($) {
  'use strict';

  // Biến toàn cục để lưu trạng thái lọc
  let filterState = {
    filter: '',
    categoryIds: [],
    minPrice: null,
    maxPrice: null,
    hasDiscount: false,
    inStock: true,
    sortingByPrice: false,
    sortingByName: false,
    sortingCreation: false,
    sortDirection: "ASC",
    skipCount: 0,
    maxResultCount: 12
  };

  let isLoading = false;
  let categoryMap = {}; // Lưu map category id -> name

  // Khởi tạo
  $(document).ready(function () {
    initCategoryMap();
    initFilters();
    bindEvents();
    loadInitialData();
  });

  // Khởi tạo category map từ dữ liệu có sẵn
  function initCategoryMap() {
    $('input[name="category"]').each(function () {
      const categoryId = parseInt($(this).val());
      const categoryName = $(this).next('.checkmark').next().text().trim();
      categoryMap[categoryId] = categoryName;
    });
  }

  // Khởi tạo bộ lọc
  function initFilters() {
    const urlParams = new URLSearchParams(window.location.search);
    filterState.filter = urlParams.get('keyword') || '';

    if (filterState.filter) {
      $('.search-box').val(filterState.filter);
    }
  }

  // Bind events
  function bindEvents() {
    // Sự kiện cho danh mục
    $('input[name="category"]').on('change', function () {
      updateCategoryFilter();
      updateActiveFilters();
      performSearch();
    });

    // Sự kiện cho mức giá tùy chỉnh
    $('.apply-price-btn').on('click', function () {
      updateCustomPriceFilter();
      updateActiveFilters();
      performSearch();
    });

    // Sự kiện cho mức giá preset
    $('input[name="price"]').on('change', function () {
      updatePricePresetFilter();
      updateActiveFilters();
      performSearch();
    });

    // Sự kiện cho các bộ lọc khác
    $('input[name="discount"]').on('change', function () {
      updateOtherFilters();
      updateActiveFilters();
      performSearch();
    });

    $('input[name="instock"]').on('change', function () {
      updateOtherFilters();
      updateActiveFilters();
      performSearch();
    });

    // Sự kiện sắp xếp
    $('.sort-btn[data-sort]').on('click', function () {
      updateSort($(this).data('sort'));
      performSearch();
    });

    $('.dropdown-item[data-direction]').on('click', function () {
      updateSortDirection($(this).data('direction'));
      performSearch();
    });

    // Sự kiện xem thêm
    $('#btn-see-more').on('click', handleLoadMore);

    // Sự kiện xem chi tiết sản phẩm
    $(document).on('click', '.product-click-detail', handleProductDetail);

    // Sự kiện xóa tất cả filter
    $('.clear-all-filters').on('click', clearAllFilters);
  }

  // Cập nhật active filters UI
  function updateActiveFilters() {
    const $container = $('.active-filters-container');
    const $tagsContainer = $('#filter-tags');
    $tagsContainer.empty();

    let hasActiveFilters = false;

    // Filter theo keyword
    if (filterState.filter) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('keyword', `Từ khóa: "${filterState.filter}"`));
    }

    // Filter theo category
    if (filterState.categoryIds.length > 0) {
      hasActiveFilters = true;
      filterState.categoryIds.forEach(categoryId => {
        const categoryName = categoryMap[categoryId] || `Danh mục ${categoryId}`;
        $tagsContainer.append(createFilterTag('category', categoryName, categoryId));
      });
    }

    // Filter theo giá
    if (filterState.minPrice !== null || filterState.maxPrice !== null) {
      hasActiveFilters = true;
      let priceText = 'Giá: ';
      if (filterState.minPrice !== null && filterState.maxPrice !== null) {
        priceText += `${formatPrice(filterState.minPrice)} - ${formatPrice(filterState.maxPrice)}`;
      } else if (filterState.minPrice !== null) {
        priceText += `Từ ${formatPrice(filterState.minPrice)}`;
      } else {
        priceText += `Đến ${formatPrice(filterState.maxPrice)}`;
      }
      $tagsContainer.append(createFilterTag('price', priceText));
    }

    // Filter đang giảm giá
    if (filterState.hasDiscount) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('discount', 'Đang giảm giá'));
    }

    // Filter còn hàng
    if (!filterState.inStock) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('instock', 'Hết hàng'));
    }

    // Hiển thị/ẩn container
    if (hasActiveFilters) {
      $container.show();
    } else {
      $container.hide();
    }
  }

  // Tạo tag filter
  function createFilterTag(type, text, value = null) {
    return `
            <div class="filter-tag" data-type="${type}" data-value="${value}">
                <span>${text}</span>
                <button type="button" class="remove-filter" onclick="removeSingleFilter('${type}', ${value})">
                    ×
                </button>
            </div>
        `;
  }

  // Xóa single filter (phải là global function để có thể gọi từ onclick)
  window.removeSingleFilter = function (type, value) {
    switch (type) {
      case 'keyword':
        filterState.filter = '';
        $('.search-box').val('');
        break;
      case 'category':
        filterState.categoryIds = filterState.categoryIds.filter(id => id !== value);
        $(`input[name="category"][value="${value}"]`).prop('checked', false);
        break;
      case 'price':
        filterState.minPrice = null;
        filterState.maxPrice = null;
        $('.price-min').val('');
        $('.price-max').val('');
        $('input[name="price"]').prop('checked', false);
        break;
      case 'discount':
        filterState.hasDiscount = false;
        $('input[name="discount"]').prop('checked', false);
        break;
      case 'instock':
        filterState.inStock = true;
        $('input[name="instock"]').prop('checked', false);
        break;
    }

    resetPagination();
    updateActiveFilters();
    performSearch();
  };

  // Xóa tất cả filters
  function clearAllFilters() {
    // Reset filter state
    filterState = {
      filter: '',
      categoryIds: [],
      minPrice: null,
      maxPrice: null,
      hasDiscount: false,
      inStock: true,
      sortingByPrice: false,
      sortingByName: false,
      sortingCreation: false,
      sortDirection: "ASC",
      skipCount: 0,
      maxResultCount: 12
    };

    // Reset UI
    $('.search-box').val('');
    $('input[name="category"]').prop('checked', false);
    $('.price-min').val('');
    $('.price-max').val('');
    $('input[name="price"]').prop('checked', false);
    $('input[name="discount"]').prop('checked', false);
    $('input[name="instock"]').prop('checked', true);

    $('.sort-btn').removeClass('active');
    $('.sort-btn[data-sort="relevant"]').addClass('active');

    resetPagination();
    updateActiveFilters();
    performSearch();
  }

  // Các hàm update filter giữ nguyên...
  function updateCategoryFilter() {
    filterState.categoryIds = [];
    $('input[name="category"]:checked').each(function () {
      filterState.categoryIds.push(parseInt($(this).val()));
    });
    resetPagination();
  }

  function updateCustomPriceFilter() {
    const minPrice = parseInt($('.price-min').val()) || null;
    const maxPrice = parseInt($('.price-max').val()) || null;

    if (minPrice !== null || maxPrice !== null) {
      filterState.minPrice = minPrice;
      filterState.maxPrice = maxPrice;
      $('input[name="price"]').prop('checked', false);
      resetPagination();
    }
  }

  function updatePricePresetFilter() {
    const selectedPrice = $('input[name="price"]:checked').val();
    if (selectedPrice) {
      const [min, max] = selectedPrice.split('-').map(Number);
      filterState.minPrice = min;
      filterState.maxPrice = max;
      resetPagination();
    }
  }

  function updateOtherFilters() {
    filterState.hasDiscount = $('input[name="discount"]').is(':checked');
    filterState.inStock = $('input[name="instock"]').is(':checked');
    resetPagination();
  }

  function updateSort(sortBy) {
    filterState.sortingByPrice = false;
    filterState.sortingByName = false;
    filterState.sortingCreation = false;

    switch (sortBy) {
      case 'price':
        filterState.sortingByPrice = true;
        break;
      case 'newest':
        filterState.sortingCreation = true;
        break;
    }

    $('.sort-btn').removeClass('active');
    $(`.sort-btn[data-sort="${sortBy}"]`).addClass('active');
    resetPagination();
  }

  function updateSortDirection(direction) {
    filterState.sortDirection = direction === 'asc' ? 'ASC' : 'DESC';
    resetPagination();
  }

  function handleLoadMore() {
    if (!isLoading) {
      filterState.skipCount += filterState.maxResultCount;
      performSearch(true);
    }
  }

  function handleProductDetail() {
    const productId = $(this).data('id');
    window.location.href = `/Product/Detail/${productId}`;
  }

  function performSearch(append = false) {
    if (isLoading) return;

    showLoading();

    const searchInput = {
      filter: filterState.filter || null,
      minPrice: filterState.minPrice,
      maxPrice: filterState.maxPrice,
      hasDiscount: filterState.hasDiscount,
      inStock: filterState.inStock,
      sortingByPrice: filterState.sortingByPrice,
      sortingByName: filterState.sortingByName,
      sortingCreation: filterState.sortingCreation,
      sortDirection: filterState.sortDirection,
      skipCount: filterState.skipCount,
      maxResultCount: filterState.maxResultCount
    };

    if (filterState.categoryIds.length > 0) {
      searchInput.categoryIds = filterState.categoryIds;
    }

    console.log('Search input:', searchInput);

    abp.services.app.homeCustomer.getAllProductHomeCustomers(searchInput)
      .then(function (result) {
        updateProductGrid(result, append);
        updateResultsCount(result.totalCount);
        hideLoading();
        toggleLoadMoreButton(result.totalCount, result.items.length);
      })
      .catch(function (error) {
        console.error('Search error:', error);
        hideLoading();
        abp.notify.error('Có lỗi xảy ra khi tìm kiếm sản phẩm');
      });
  }

  function updateProductGrid(result, append) {
    const $productsGrid = $('.products-grid');
    const $productItems = $productsGrid.find('.product-item');

    if (!append) {
      $productItems.remove();
      $productsGrid.find('.load-more-container').remove();
    }

    if (result.items && result.items.length > 0) {
      const productHtml = generateProductHtml(result.items);

      if (append) {
        $productsGrid.append(productHtml);
      } else {
        $productsGrid.prepend(productHtml);
      }

      if (!append) {
        $productsGrid.after(`
                <div class="load-more-container">
                    <button id="btn-see-more" class="btn btn-outline-primary px-4 py-2">
                        Xem thêm
                    </button>
                </div>
            `);
        $('#btn-see-more').off('click').on('click', handleLoadMore);
      }
    } else if (!append) {
      $productsGrid.html(`
            <div class="no-products-message">
                <p class="text-muted">Không tìm thấy sản phẩm nào phù hợp</p>
            </div>
        `);
    }
  }

  function generateProductHtml(products) {
    let html = '';
    products.forEach(product => {
      if (product.productVariants && product.productVariants.length > 0) {
        product.productVariants.forEach(variant => {
          html += `
                    <div class="product-item">
                        <div class="product-card product-click-detail" data-id="${product.id}">
                            <div class="image-container">
                                ${variant.imageUrl ?
              `<img src="${variant.imageUrl}" class="product-image" alt="${product.name}">` :
              `<img src="/img/products/default.png" class="product-image" alt="${product.name}">`
            }
                            </div>
                            <div class="product-info">
                                <div class="product-name">${product.name}</div>
                                <div class="variant-info">
                                    ${variant.ram ?
              `${variant.ram}/${variant.storage}` :
              `${variant.storage}/${variant.color || ''}`
            }
                                </div>
                                <div class="price-section">
                                    <div class="current-price">${formatPrice(variant.price)}₫</div>
                                </div>
                            </div>
                            <button class="view-detail-btn productvariant-click-detail" data-id="${variant.id}">
                                Xem chi tiết
                            </button>
                        </div>
                    </div>
                `;
        });
      }
    });
    return html;
  }

  function updateResultsCount(totalCount) {
    $('.results-count').text(`${totalCount} kết quả`);
  }

  function showLoading() {
    isLoading = true;
    $('.products-grid').addClass('loading');
    $('#btn-see-more').prop('disabled', true).text('Đang tải...');
  }

  function hideLoading() {
    isLoading = false;
    $('.products-grid').removeClass('loading');
    $('#btn-see-more').prop('disabled', false).text('Xem thêm');
  }

  function resetPagination() {
    filterState.skipCount = 0;
  }

  function toggleLoadMoreButton(totalCount, currentCount) {
    const $btnSeeMore = $('#btn-see-more');
    if (!$btnSeeMore.length) return;

    if (filterState.skipCount + currentCount >= totalCount || currentCount === 0) {
      $btnSeeMore.hide();
    } else {
      $btnSeeMore.show();
    }
  }

  function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price);
  }

  function loadInitialData() {
    if (filterState.filter) {
      performSearch();
    }
  }

  // Cập nhật active filters UI
  function updateActiveFilters() {
    const $container = $('.active-filters-container');
    const $tagsContainer = $('#filter-tags');
    $tagsContainer.empty();

    let hasActiveFilters = false;

    // Filter theo keyword
    if (filterState.filter) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('keyword', 'Từ khóa', `"${filterState.filter}"`));
    }

    // Filter theo category
    if (filterState.categoryIds.length > 0) {
      hasActiveFilters = true;
      filterState.categoryIds.forEach(categoryId => {
        const categoryName = categoryMap[categoryId] || `Danh mục ${categoryId}`;
        $tagsContainer.append(createFilterTag('category', categoryName, null, categoryId));
      });
    }

    // Filter theo giá
    if (filterState.minPrice !== null || filterState.maxPrice !== null) {
      hasActiveFilters = true;
      let priceValue = '';
      if (filterState.minPrice !== null && filterState.maxPrice !== null) {
        priceValue = `${formatPrice(filterState.minPrice)} - ${formatPrice(filterState.maxPrice)}`;
      } else if (filterState.minPrice !== null) {
        priceValue = `Từ ${formatPrice(filterState.minPrice)}`;
      } else {
        priceValue = `Đến ${formatPrice(filterState.maxPrice)}`;
      }
      $tagsContainer.append(createFilterTag('price', 'Giá', priceValue));
    }

    // Filter đang giảm giá
    if (filterState.hasDiscount) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('discount', 'Đang giảm giá'));
    }

    // Filter còn hàng (chỉ hiển thị khi là false - hết hàng)
    if (!filterState.inStock) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('instock', 'Hết hàng'));
    }

    // Hiển thị/ẩn container
    if (hasActiveFilters) {
      $container.show();
    } else {
      $container.hide();
    }
  }

  // Tạo tag filter với format: TênFilter (GiáTrị) ×
  function createFilterTag(type, name, value = null, id = null) {
    const displayValue = value ? ` ${value}` : '';
    return `
        <div class="filter-tag" data-type="${type}" data-value="${id}">
            <div class="tag-content">
                <span class="tag-name">${name}</span>
                ${displayValue ? `<span class="tag-value">${displayValue}</span>` : ''}
            </div>
            <button type="button" class="remove-filter" onclick="removeSingleFilter('${type}', ${id})">
                ×
            </button>
        </div>
    `;
  }

  // Xóa single filter
  window.removeSingleFilter = function (type, value) {
    switch (type) {
      case 'keyword':
        filterState.filter = '';
        $('.search-box').val('');
        break;
      case 'category':
        filterState.categoryIds = filterState.categoryIds.filter(id => id !== value);
        $(`input[name="category"][value="${value}"]`).prop('checked', false);
        break;
      case 'price':
        filterState.minPrice = null;
        filterState.maxPrice = null;
        $('.price-min').val('');
        $('.price-max').val('');
        $('input[name="price"]').prop('checked', false);
        break;
      case 'discount':
        filterState.hasDiscount = false;
        $('input[name="discount"]').prop('checked', false);
        break;
      case 'instock':
        filterState.inStock = true;
        $('input[name="instock"]').prop('checked', true);
        break;
    }

    resetPagination();
    updateActiveFilters();
    performSearch();
  };

  // Xóa tất cả filters
  function clearAllFilters() {
    // Reset filter state
    filterState = {
      filter: '',
      categoryIds: [],
      minPrice: null,
      maxPrice: null,
      hasDiscount: false,
      inStock: true,
      sortingByPrice: false,
      sortingByName: false,
      sortingCreation: false,
      sortDirection: "ASC",
      skipCount: 0,
      maxResultCount: 12
    };

    // Reset UI
    $('.search-box').val('');
    $('input[name="category"]').prop('checked', false);
    $('.price-min').val('');
    $('.price-max').val('');
    $('input[name="price"]').prop('checked', false);
    $('input[name="discount"]').prop('checked', false);
    $('input[name="instock"]').prop('checked', true);

    $('.sort-btn').removeClass('active');
    $('.sort-btn[data-sort="relevant"]').addClass('active');

    resetPagination();
    updateActiveFilters();
    performSearch();
  }

})(jQuery);