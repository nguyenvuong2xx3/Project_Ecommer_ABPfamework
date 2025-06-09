(function ($) {
  "use strict";


  // Fixed Navbar
  let lastScrollTop = 0;

  $(window).scroll(function () {
    let currentScroll = $(this).scrollTop();

    if (currentScroll > lastScrollTop) {
      // Nếu cuộn xuống thì ẩn thanh navbar
      $('.navbar2').slideUp(200);
    } else {
      // Nếu cuộn lên thì hiện thanh navbar
      $('.navbar2').slideDown(200);
    }

    lastScrollTop = currentScroll <= 0 ? 0 : currentScroll; // Đảm bảo giá trị không bị âm
  });

  document.addEventListener("DOMContentLoaded", function () {
    var myCarouselEl = document.querySelector('#carouselId');

    // Khởi tạo Carousel tự động chạy
    var carousel = new bootstrap.Carousel(myCarouselEl, {
      interval: 1000,  // 2.5 giây
      pause: false     // Không dừng khi hover
    });

    // Nút Prev
    document.querySelector('.carousel-control-prev').addEventListener('click', function () {
      carousel.prev();
    });

    // Nút Next
    document.querySelector('.carousel-control-next').addEventListener('click', function () {
      carousel.next();
    });
  });

  // Back to top button
  $(window).scroll(function () {
    if ($(this).scrollTop() > 300) {
      $('.back-to-top').fadeIn('slow');
    } else {
      $('.back-to-top').fadeOut('slow');
    }
  });
  $('.back-to-top').click(function () {
    $('html, body').animate({ scrollTop: 0 }, 1500, 'easeInOutExpo');
    return false;
  });


  // Product Quantity
  $('.quantity button').on('click', function () {
    var button = $(this);
    var oldValue = button.parent().parent().find('input').val();
    if (button.hasClass('btn-plus')) {
      var newVal = parseFloat(oldValue) + 1;
    } else {
      if (oldValue > 0) {
        var newVal = parseFloat(oldValue) - 1;
      } else {
        newVal = 0;
      }
    }
    button.parent().parent().find('input').val(newVal);
  });


  // Product Details
  //function goToDetail(productId) {
    function goToDetail(productId) {
      window.location.href = `/Product/DetailProductCustomer/${productId}`;
    }

  //}

  $(document).on('click', '.product-click-detail', function () {
    var productId = $(this).data('id'); // Sửa "data-id" thành 'id'
    window.location.href = '/HomeCustomer/DetailProductCusTomer?productId=' + productId;
  });

  


  // Home/Shop
  $(document).on('click', '.click-shop', function () {
    window.location.href = '/HomeCustomer/ShopProductCusTomer';
  });

  document.addEventListener('DOMContentLoaded', function () {
    // Lấy các phần tử DOM
    const searchInput = document.querySelector('.txt-search');
    const searchButton = document.querySelector('.btn[data-bs-target="#searchModal"]');

    // Hàm xử lý tìm kiếm
    function handleSearch() {
      const keyword = searchInput.value.trim();

      if (keyword) {
        window.location.href = '/HomeCustomer/SearchProductCustomer?keyword=' + keyword;
      } else {
        alert('Vui lòng nhập từ khóa tìm kiếm');
      }
    }

    // Xử lý sự kiện click trên icon search
    document.getElementById("searchButton").addEventListener("click", function () {
      // Code xử lý sự kiện click ở đây
      handleSearch()
    });

    // Xử lý sự kiện nhấn Enter trong input
    searchInput.addEventListener('keypress', function (e) {
      if (e.key === 'Enter') {
        e.preventDefault();
        handleSearch();
      }
    });
  });

  //tìm kiếm theo danh mục
  $(document).on('click', '.category-click-getid', function () {
    var categoryId = $(this).data('id'); // Sửa "data-id" thành 'id'
    window.location.href = '/HomeCustomer/SearchProductCustomer?category=' + categoryId;
  });
  // nếu click vào AllCategory( tất cả danh mục)
  $(document).on('click', '.category-click-getall', function () {
    window.location.href = '/HomeCustomer';
  });
  // nhận sự kiện nút << firt
  $(document).on('click', '.firt-button', function () {
    window.location.href = '/HomeCustomer?page' + 1;
  });

 


})(jQuery);

