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
    $('input[name="category"]').on('change', function () {
      updateCategoryFilter();
      updateActiveFilters();
      performSearch();
    });

    $('.apply-price-btn').on('click', function () {
      updateCustomPriceFilter();
      updateActiveFilters();
      performSearch();
    });

    $('input[name="price"]').on('change', function () {
      updatePricePresetFilter();
      updateActiveFilters();
      performSearch();
    });

    $('input[name="discount"], input[name="instock"]').on('change', function () {
      updateOtherFilters();
      updateActiveFilters();
      performSearch();
    });

    $('.sort-btn[data-sort]').on('click', function () {
      updateSort($(this).data('sort'));
      performSearch();
    });

    $('.dropdown-item[data-direction]').on('click', function () {
      updateSortDirection($(this).data('direction'));
      performSearch();
    });

    $('#btn-see-more').on('click', handleLoadMore);

    $(document).on('click', '.product-click-detail', function () {
      const id = $(this).data('id');
      window.location.href = '/HomeCustomer/DetailProductCustomer?id=' + id;
    });

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

  function updateCategoryFilter() {
    filterState.categoryIds = [];
    $('input[name="category"]:checked').each(function () {
      filterState.categoryIds.push(parseInt($(this).val()));
    });
    resetPagination();
  }

  function updateCustomPriceFilter() {
    const min = parseInt($('.price-min').val()) || null;
    const max = parseInt($('.price-max').val()) || null;
    if (min !== null || max !== null) {
      filterState.minPrice = min;
      filterState.maxPrice = max;
      $('input[name="price"]').prop('checked', false);
      resetPagination();
    }
  }

  function updatePricePresetFilter() {
    const val = $('input[name="price"]:checked').val();
    if (val) {
      const parts = val.split('-');
      filterState.minPrice = parseInt(parts[0]);
      filterState.maxPrice = parseInt(parts[1]);
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
    if (sortBy === 'price') filterState.sortingByPrice = true;
    if (sortBy === 'newest') filterState.sortingCreation = true;

    $('.sort-btn').removeClass('active');
    $(`.sort-btn[data-sort="${sortBy}"]`).addClass('active');
    resetPagination();
  }

  function updateSortDirection(direction) {
    filterState.sortDirection = direction === 'asc' ? 'ASC' : 'DESC';
    resetPagination();
  }

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