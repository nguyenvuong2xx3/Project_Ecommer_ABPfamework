(function ($) {
  var skipCount = 0,
    pageSize = 12,
    currentFilters = {
      brands: [],
      minPrice: null,
      maxPrice: null,
      hasDiscount: false,
      inStock: true,
      sortingByPrice: false,
      sortingByName: false,
      sortingCreation: false,
      sortDirection: "ASC",
      searchTerm: '',
      categoryId: null
    };

  // Khởi tạo
  function init() {
    bindEvents();
    loadProducts();
  }

  // Bind events
  function bindEvents() {
    // Brand filter
    $('.brand-list input[name="brand"]').on('change', function () {
      handleBrandFilter();
    });

    // Price presets
    $('.price-presets input[name="price"]').on('change', function () {
      handlePricePreset($(this));
    });

    // Apply price button
    $('.apply-price-btn').on('click', function () {
      handleCustomPriceFilter();
    });

    // Other filters
    $('.other-filters input[name="discount"]').on('change', function () {
      currentFilters.hasDiscount = $(this).is(':checked');
      loadProducts();
    });

    $('.other-filters input[name="instock"]').on('change', function () {
      currentFilters.inStock = $(this).is(':checked');
      loadProducts();
    });

    // Sort buttons
    $('.sort-btn').on('click', function () {
      handleSortButtonClick($(this));
    });

    // Direction buttons
    $('.direction-btn').on('click', function () {
      handleDirectionButtonClick($(this));
    });

    // Price input enter key
    $('.price-min, .price-max').on('keypress', function (e) {
      if (e.which === 13) {
        handleCustomPriceFilter();
      }
    });

    // See more button
    $('#btn-see-more').on('click', function () {
      loadMoreProducts();
    });
  }

  // Xử lý click sort button
  function handleSortButtonClick($button) {
    var sortType = $button.data('sort');

    // Reset tất cả sorting flags
    resetSortingFlags();

    // Remove active class từ tất cả buttons
    $('.sort-btn').removeClass('active');
    $('.sort-group').removeClass('active');

    switch (sortType) {
      case 'price':
        currentFilters.sortingByPrice = true;
        $button.closest('.sort-group').addClass('active');
        $button.addClass('active');
        // Set direction mặc định cho price
        setDirectionActive('price', 'asc');
        break;

      case 'name':
        currentFilters.sortingByName = true;
        $button.closest('.sort-group').addClass('active');
        $button.addClass('active');
        // Set direction mặc định cho name
        setDirectionActive('name', 'asc');
        break;

      case 'newest':
        currentFilters.sortingCreation = true;
        currentFilters.sortDirection = "DESC";
        $button.addClass('active');
        break;

      case 'bestseller':
        // Nếu có field bestseller, bạn có thể thêm ở đây
        $button.addClass('active');
        break;

      default: // relevant
        // Không set sorting nào - để server xử lý mặc định
        $button.addClass('active');
        break;
    }

    skipCount = 10; // Reset page to 1 when sorting changes
    loadProducts();
  }

  // Xử lý click direction button
  function handleDirectionButtonClick($button) {
    var direction = $button.data('direction');
    var $sortGroup = $button.closest('.sort-group');
    var sortType = $sortGroup.find('.sort-btn').data('sort');

    // Reset và set active cho direction buttons
    $sortGroup.find('.direction-btn').removeClass('active');
    $button.addClass('active');

    // Set sorting flags
    resetSortingFlags();

    switch (sortType) {
      case 'price':
        currentFilters.sortingByPrice = true;
        break;
      case 'name':
        currentFilters.sortingByName = true;
        break;
    }

    currentFilters.sortDirection = direction.toUpperCase();

    // Set active cho sort button
    $sortGroup.addClass('active');
    $sortGroup.find('.sort-btn').addClass('active');

    skipCount = 10; // Reset page to 1 when direction changes
    loadProducts();
  }

  // Set direction active
  function setDirectionActive(sortType, direction) {
    $(`.sort-group .sort-btn[data-sort="${sortType}"]`)
      .closest('.sort-group')
      .find(`.direction-btn[data-direction="${direction}"]`)
      .addClass('active');

    currentFilters.sortDirection = direction.toUpperCase();
  }

  // Reset tất cả sorting flags
  function resetSortingFlags() {
    currentFilters.sortingByPrice = false;
    currentFilters.sortingByName = false;
    currentFilters.sortingCreation = false;
  }

  // Xử lý lọc thương hiệu
  function handleBrandFilter() {
    currentFilters.brands = [];
    $('.brand-list input[name="brand"]:checked').each(function () {
      currentFilters.brands.push($(this).val());
    });
    skipCount = 10; // Reset page to 1 when brand filter changes
    loadProducts();
  }

  // Xử lý mức giá định sẵn
  function handlePricePreset($element) {
    var value = $element.val();
    var priceRange = value.split('-');

    currentFilters.minPrice = parseInt(priceRange[0]);
    currentFilters.maxPrice = parseInt(priceRange[1]);

    skipCount = 10; // Reset page to 1 when price preset changes
    loadProducts();
  }

  // Xử lý lọc giá tùy chỉnh
  function handleCustomPriceFilter() {
    var minPrice = $('.price-min').val().replace(/[^0-9]/g, '');
    var maxPrice = $('.price-max').val().replace(/[^0-9]/g, '');

    currentFilters.minPrice = minPrice ? parseInt(minPrice) : null;
    currentFilters.maxPrice = maxPrice ? parseInt(maxPrice) : null;

    skipCount = 10;  // Reset page to 1 when custom price changes
    loadProducts();
  }

  // Load products với filters - CALL CONTROLLER
  function loadProducts() {
    // Build query string từ filters
    var queryParams = buildQueryParams();
    var url = '/HomeCustomer/SearchProductCustomer' + queryParams;

    abp.ui.setBusy($('.search-content'));

    $.ajax({
      url: url,
      type: 'GET',
      success: function (response) {
        // Parse HTML response và cập nhật grid
        updateProductGridFromResponse(response, true); // Pass isInitialLoad = true
      },
      error: function (error) {
        abp.message.error('Có lỗi xảy ra khi tải sản phẩm');
        console.error(error);
      },
      complete: function () {
        abp.ui.clearBusy($('.search-content'));
      }
    });
  }

  // Load more products (for pagination)
  function loadMoreProducts() {
    skipCount = skipCount + 10;
    loadProducts();
  }

  // Build query parameters từ filters
  function buildQueryParams() {
    var params = [];

    // Page parameters
    params.push('skipCount=' + skipCount);
    params.push('pageSize=' + pageSize);

    // Filter parameters
    if (currentFilters.searchTerm) {
      params.push('filter=' + encodeURIComponent(currentFilters.searchTerm));
    }

    if (currentFilters.categoryId) {
      params.push('categoryId=' + currentFilters.categoryId);
    }

    // Brand filters
    if (currentFilters.brands.length > 0) {
      currentFilters.brands.forEach(function (brand) {
        params.push('brands=' + encodeURIComponent(brand));
      });
    }

    // Price filters
    if (currentFilters.minPrice !== null) {
      params.push('minPrice=' + currentFilters.minPrice);
    }

    if (currentFilters.maxPrice !== null) {
      params.push('maxPrice=' + currentFilters.maxPrice);
    }

    // Other filters
    if (currentFilters.hasDiscount) {
      params.push('hasDiscount=true');
    }

    if (!currentFilters.inStock) {
      params.push('inStock=false');
    }

    // Sorting parameters
    if (currentFilters.sortingByPrice) {
      params.push('sortingByPrice=true');
    }

    if (currentFilters.sortingByName) {
      params.push('sortingByName=true');
    }

    if (currentFilters.sortingCreation) {
      params.push('sortingCreation=true');
    }

    if (currentFilters.sortDirection) {
      params.push('sortDirection=' + currentFilters.sortDirection);
    }

    return '?' + params.join('&');
  }

  // Cập nhật grid sản phẩm từ HTML response
  function updateProductGridFromResponse(htmlResponse, isInitialLoad) {
    // Tạo temporary div để parse HTML
    var $temp = $('<div>').html(htmlResponse);

    // Lấy products grid từ response
    var $newProductsGrid = $temp.find('.row.row-cols-6.g-3.d-flex.flex-wrap');
    var $newResultsCount = $temp.find('.results-count');
    var $newSearchHeader = $temp.find('.search-header h1');

    // Cập nhật products grid
    if ($newProductsGrid.length) {
      if (isInitialLoad) {
        // If it's the first load, replace the entire grid
        $('.row.row-cols-6.g-3.d-flex.flex-wrap').html($newProductsGrid.html());
      } else {
        // If it's a subsequent load, append to the existing grid
        $('.row.row-cols-6.g-3.d-flex.flex-wrap').append($newProductsGrid.html());
      }
    } else {
      if (isInitialLoad) {
        // If no products are found on the first load, display a message
        $('.row.row-cols-6.g-3.d-flex.flex-wrap').html('<p>Không tìm thấy sản phẩm nào.</p>');
      }
      // If no products are found on subsequent loads, you might want to hide the "See More" button
      //$('#btn-see-more').hide();
    }

    // Cập nhật results count
    if ($newResultsCount.length) {
      $('.results-count').text($newResultsCount.text());
    }

    // Cập nhật header nếu có
    if ($newSearchHeader.length && $newSearchHeader.text() !== $('.search-header h1').text()) {
      $('.search-header h1').text($newSearchHeader.text());
    }

    // Re-bind click events cho các sản phẩm mới
    $('.product-click-detail').off('click').on('click', function () {
      var productId = $(this).data('id');
      viewProductDetail(productId);
    });

    updateActiveFilters();
  }


  // Cập nhật active filters hiển thị
  function updateActiveFilters() {
    // Có thể thêm logic để hiển thị các filter đang active
    console.log('Current filters:', currentFilters);
  }

  // Format price
  function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + '₫';
  }

  // Xem chi tiết sản phẩm
  function viewProductDetail(productId) {
    // Implement product detail view logic here
    console.log('View product detail:', productId);
    // window.location.href = '/Product/Detail/' + productId;
  }

  // Public functions
  function searchProducts(searchTerm) {
    currentFilters.searchTerm = searchTerm;
    skipCount = 10; 
    loadProducts();
  }

  function filterByCategory(categoryId, categoryName) {
    currentFilters.categoryId = categoryId;
    currentFilters.searchTerm = '';
    skipCount = 10; 

    // Update page title
    $('.search-header h1').text('Danh mục - ' + categoryName);
    loadProducts();
  }

  function clearFilters() {
    currentFilters = {
      brands: [],
      minPrice: null,
      maxPrice: null,
      hasDiscount: false,
      inStock: true,
      sortingByPrice: false,
      sortingByName: false,
      sortingCreation: false,
      sortDirection: "ASC",
      searchTerm: currentFilters.searchTerm,
      categoryId: currentFilters.categoryId
    };

    // Reset UI
    $('.brand-list input[name="brand"]').prop('checked', false);
    $('.price-presets input[name="price"]').prop('checked', false);
    $('.other-filters input[name="discount"]').prop('checked', false);
    $('.other-filters input[name="instock"]').prop('checked', true);
    $('.price-min').val('');
    $('.price-max').val('');
    $('.sort-btn').removeClass('active');
    $('.sort-group').removeClass('active');
    $('.direction-btn').removeClass('active');

    // Set relevant as default
    $('.sort-btn[data-sort="relevant"]').addClass('active');

    loadProducts();
  }

  // Xử lý phân trang
  function skipCount(skipCount) {
    skipCount = skipCount;
    loadProducts();
  }

  // Global functions
  window.homeCustomerSearch = {
    init: init,
    search: searchProducts,
    filterByCategory: filterByCategory,
    clearFilters: clearFilters,
    skipCount: skipCount,
    loadProducts: loadProducts
  };

  // Auto-init khi DOM ready
  $(document).ready(function () {
    init();

    // Set relevant as default active
    $('.sort-btn[data-sort="relevant"]').addClass('active');
  });

})(jQuery);