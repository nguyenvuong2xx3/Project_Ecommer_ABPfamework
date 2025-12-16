(function ($) {
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
  let categoryMap = {};

  $(document).ready(function () {
    initCategoryMap();
    initFilters();
    bindEvents();

		// kiểm tra và hiển thị nút xem thêm
    checkLoadMoreVisibility();
  });

  function initCategoryMap() {
    $('input[name="category"]').each(function () {
      const categoryId = parseInt($(this).val());
      
			const categoryName = $(this).data('name');
      categoryMap[categoryId] = categoryName;
    });
  }

  function initFilters() {
    const urlParams = new URLSearchParams(window.location.search);
    filterState.filter = urlParams.get('keyword') || '';
    if (filterState.filter) {
      $('.search-box').val(filterState.filter);
    }
  }

  function bindEvents() {
    // Xử lý sự kiện thay đổi danh mục
    $('input[name="category"]').on('change', function () {
      updateCategoryFilter();
      updateActiveFilters();
      performSearch();
    });

    // Xử lý sự kiện áp dụng bộ lọc giá tùy chỉnh
    $('.apply-price-btn').on('click', function () {
      updateCustomPriceFilter();
      updateActiveFilters();
      performSearch();
    });

    // Xử lý sự kiện chọn mức giá có sẵn
    $('input[name="price"]').on('change', function () {
      updatePricePresetFilter();
      updateActiveFilters();
      performSearch();
    });

    // Xử lý sự kiện thay đổi bộ lọc khác (giảm giá, còn hàng)
    $('input[name="discount"], input[name="instock"]').on('change', function () {
      updateOtherFilters();
      updateActiveFilters();
      performSearch();
    });

    // Xử lý sự kiện click nút sắp xếp
    // Lấy cả data-sort và data-direction từ nút được click
    $('.sort-btn[data-sort]').on('click', function () {
      const $btn = $(this);
      const sortBy = $btn.data('sort');
      const direction = $btn.data('direction'); // Lấy hướng sắp xếp (asc/desc)
      
      updateSort(sortBy, direction);
      performSearch();
    });

    // Xử lý sự kiện click nút xem thêm
    $('#btn-see-more').on('click', handleLoadMore);

    // Xử lý sự kiện click vào sản phẩm để xem chi tiết
    $(document).on('click', '.product-click-detail', function () {
      const id = $(this).data('id');
      window.location.href = '/HomeCustomer/DetailProductCustomer?id=' + id;
    });

    // Xử lý sự kiện xóa tất cả bộ lọc
    $('.clear-all-filters').on('click', clearAllFilters);
  }

  function performSearch(append = false) {
    if (isLoading) return;
    showLoading();

    const searchInput = {
      filter: filterState.filter,
      categoryIds: filterState.categoryIds,
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

    $.ajax({
      url: '/HomeCustomer/GetListProduct',
      type: 'POST',
      data: searchInput,
      success: function (htmlResponse) {
        // 1. Cập nhật Grid
        updateProductGrid(htmlResponse, append);

        // 2. Kiểm tra và cập nhật nút Xem thêm
        checkLoadMoreVisibility();

        hideLoading();
      },
      error: function (err) {
        console.error('Search error:', err);
        hideLoading();
        abp.notify.error('Có lỗi xảy ra khi tìm kiếm');
      }
    });
  }

  function updateProductGrid(html, append) {
    const $productsGrid = $('.products-grid');
    if (append) {
      $productsGrid.append(html);
    } else {
      $productsGrid.html(html);
    }
  }

	// Kiểm tra và ẩn/hiện nút Xem thêm
  function checkLoadMoreVisibility() {
    // Lấy TotalCount từ thẻ input hidden mới nhất (nằm trong Partial View vừa render)
    const totalCount = parseInt($('.total-count-hidden').last().val()) || 0;

    // Đếm số sản phẩm đang hiển thị trên màn hình
    const currentDisplayCount = $('.products-grid .product-item').length;

    // Cập nhật text hiển thị số lượng kết quả
    updateResultsCount(totalCount);

    // Ẩn/Hiện nút
    const $btnSeeMore = $('#btn-see-more');
    if (currentDisplayCount >= totalCount || totalCount === 0) {
      $btnSeeMore.hide();
    } else {
      $btnSeeMore.show();
    }
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

  function handleLoadMore() {
    if (!isLoading) {
      filterState.skipCount += filterState.maxResultCount;
      performSearch(true);
    }
  }

  // Cập nhật bộ lọc danh mục
  function updateCategoryFilter() {
    filterState.categoryIds = [];
    $('input[name="category"]:checked').each(function () {
      filterState.categoryIds.push(parseInt($(this).val()));
    });
    resetPagination();
  }

  // Cập nhật bộ lọc giá tùy chỉnh (nhập tay)
  function updateCustomPriceFilter() {
    const min = parseInt($('.price-min').val()) || null;
    const max = parseInt($('.price-max').val()) || null;
    if (min !== null || max !== null) {
      filterState.minPrice = min;
      filterState.maxPrice = max;
      // Bỏ chọn các mức giá có sẵn khi nhập giá tùy chỉnh
      $('input[name="price"]').prop('checked', false);
      resetPagination();
    }
  }

  // Cập nhật bộ lọc mức giá có sẵn
  function updatePricePresetFilter() {
    const val = $('input[name="price"]:checked').val();
    if (val) {
      const parts = val.split('-');
      filterState.minPrice = parseInt(parts[0]);
      filterState.maxPrice = parseInt(parts[1]);
      resetPagination();
    }
  }

  // Cập nhật bộ lọc khác (giảm giá, còn hàng)
  function updateOtherFilters() {
    filterState.hasDiscount = $('input[name="discount"]').is(':checked');
    filterState.inStock = $('input[name="instock"]').is(':checked');
    resetPagination();
  }

  /**
   * Cập nhật trạng thái sắp xếp
   * @param {string} sortBy - Loại sắp xếp: 'price', 'newest', 'bestseller'
   * @param {string} direction - Hướng sắp xếp: 'asc' (thấp đến cao), 'desc' (cao đến thấp)
   */
  function updateSort(sortBy, direction) {
    // Reset tất cả các loại sắp xếp về false
    filterState.sortingByPrice = false;
    filterState.sortingByName = false;
    filterState.sortingCreation = false;
    
    // Bật loại sắp xếp tương ứng
    if (sortBy === 'price') {
      filterState.sortingByPrice = true;
      // Cập nhật hướng sắp xếp giá: ASC = thấp đến cao, DESC = cao đến thấp
      filterState.sortDirection = (direction === 'desc') ? 'DESC' : 'ASC';
    }
    if (sortBy === 'newest') {
      filterState.sortingCreation = true;
      // Mới nhất = sắp xếp giảm dần theo ngày tạo
      filterState.sortDirection = 'DESC';
    }
    if (sortBy === 'bestseller') {
      // Bán chạy = sắp xếp theo số lượng bán (nếu có)
      // Tạm thời dùng sortingByName hoặc thêm field mới nếu cần
      filterState.sortingByName = true;
      filterState.sortDirection = 'DESC';
    }

    // Bỏ class active của tất cả nút sắp xếp
    $('.sort-btn').removeClass('active');
    
    // Thêm class active cho nút được click
    // Nếu là sắp xếp theo giá, cần xác định đúng nút dựa trên cả sort và direction
    if (sortBy === 'price' && direction) {
      $(`.sort-btn[data-sort="price"][data-direction="${direction}"]`).addClass('active');
    } else {
      $(`.sort-btn[data-sort="${sortBy}"]`).addClass('active');
    }
    
    // Reset phân trang về trang đầu
    resetPagination();
  }

  // Cập nhật hướng sắp xếp (không còn dùng riêng nữa, đã gộp vào updateSort)
  function updateSortDirection(direction) {
    filterState.sortDirection = direction === 'asc' ? 'ASC' : 'DESC';
    resetPagination();
  }

  // Reset phân trang về trang đầu
  function resetPagination() {
    filterState.skipCount = 0;
  }

  function updateActiveFilters() {
    const $container = $('.active-filters-container');
    const $tagsContainer = $('#filter-tags');
    $tagsContainer.empty();
    let hasActiveFilters = false;

    if (filterState.filter) {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('keyword', 'Từ khóa', `"${filterState.filter}"`));
    }

    filterState.categoryIds.forEach(id => {
      hasActiveFilters = true;
      $tagsContainer.append(createFilterTag('category', categoryMap[id] || id, null, id));
    });

    if (filterState.minPrice !== null || filterState.maxPrice !== null) {
      hasActiveFilters = true;
      let priceText = '';
      if (filterState.minPrice && filterState.maxPrice) priceText = `${formatPrice(filterState.minPrice)} - ${formatPrice(filterState.maxPrice)}`;
      else if (filterState.minPrice) priceText = `Từ ${formatPrice(filterState.minPrice)}`;
      else priceText = `Đến ${formatPrice(filterState.maxPrice)}`;
      $tagsContainer.append(createFilterTag('price', 'Giá', priceText));
    }

    if (filterState.hasDiscount) { hasActiveFilters = true; $tagsContainer.append(createFilterTag('discount', 'Đang giảm giá')); }
    if (!filterState.inStock) { hasActiveFilters = true; $tagsContainer.append(createFilterTag('instock', 'Hết hàng')); }

    hasActiveFilters ? $container.show() : $container.hide();
  }

  function createFilterTag(type, name, value = null, id = null) {
    const displayValue = value ? ` ${value}` : '';
    return `<div class="filter-tag" data-type="${type}" data-value="${id}"><div class="tag-content"><span class="tag-name">${name}</span>${displayValue ? `<span class="tag-value">${displayValue}</span>` : ''}</div><button type="button" class="remove-filter" onclick="removeSingleFilter('${type}', ${id})">×</button></div>`;
  }

  function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price);
  }

  window.removeSingleFilter = function (type, value) {
    switch (type) {
      case 'keyword': filterState.filter = ''; $('.search-box').val(''); break;
      case 'category':
        filterState.categoryIds = filterState.categoryIds.filter(id => id !== value);
        $(`input[name="category"][value="${value}"]`).prop('checked', false);
        break;
      case 'price':
        filterState.minPrice = null; filterState.maxPrice = null;
        $('.price-min, .price-max').val(''); $('input[name="price"]').prop('checked', false);
        break;
      case 'discount': filterState.hasDiscount = false; $('input[name="discount"]').prop('checked', false); break;
      case 'instock': filterState.inStock = true; $('input[name="instock"]').prop('checked', true); break;
    }
    resetPagination();
    updateActiveFilters();
    performSearch();
  };

  function clearAllFilters() {
    filterState = { ...filterState, filter: '', categoryIds: [], minPrice: null, maxPrice: null, hasDiscount: false, inStock: true, skipCount: 0 };
    $('.search-box, .price-min, .price-max').val('');
    $('input[type="checkbox"], input[type="radio"]').prop('checked', false);
    $('input[name="instock"]').prop('checked', true);
    $('.sort-btn').removeClass('active');
    $('.sort-btn[data-sort="relevant"]').addClass('active');

    updateActiveFilters();
    performSearch();
  }

})(jQuery);