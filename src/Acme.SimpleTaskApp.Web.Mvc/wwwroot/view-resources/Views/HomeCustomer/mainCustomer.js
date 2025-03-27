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


  //// Testimonial carousel
  //$(".testimonial-carousel").owlCarousel({
  //  autoplay: true,
  //  smartSpeed: 2000,
  //  center: false,
  //  dots: true,
  //  loop: true,
  //  margin: 25,
  //  nav: true,
  //  navText: [
  //    '<i class="bi bi-arrow-left"></i>',
  //    '<i class="bi bi-arrow-right"></i>'
  //  ],
  //  responsiveClass: true,
  //  responsive: {
  //    0: {
  //      items: 1
  //    },
  //    576: {
  //      items: 1
  //    },
  //    768: {
  //      items: 1
  //    },
  //    992: {
  //      items: 2
  //    },
  //    1200: {
  //      items: 2
  //    }
  //  }
  //});


  // vegetable carousel
  //$(".vegetable-carousel").owlCarousel({
  //  autoplay: true,
  //  smartSpeed: 1500,
  //  center: false,
  //  dots: true,
  //  loop: true,
  //  margin: 25,
  //  nav: true,
  //  navText: [
  //    '<i class="bi bi-arrow-right"></i>',
  //    '<i class="bi bi-arrow-left"></i>'
  //  ],
  //  responsiveClass: true,
  //  responsive: {
  //    0: {
  //      items: 1 // 1 sản phẩm trên màn hình nhỏ
  //    },
  //    576: {
  //      items: 2 // 2 sản phẩm trên màn hình >= 576px
  //    },
  //    768: {
  //      items: 3 // 3 sản phẩm trên màn hình >= 768px
  //    },
  //    992: {
  //      items: 4 // 4 sản phẩm trên màn hình >= 992px
  //    },
  //    1200: {
  //      items: 6 // 6 sản phẩm trên màn hình >= 1200px
  //    }

  //  }
  //});


  // Modal Video
  //$(document).ready(function () {
  //  var $videoSrc;
  //  $('.btn-play').click(function () {
  //    $videoSrc = $(this).data("src");
  //  });
  //  console.log($videoSrc);

  //  $('#videoModal').on('shown.bs.modal', function (e) {
  //    $("#video").attr('src', $videoSrc + "?autoplay=1&amp;modestbranding=1&amp;showinfo=0");
  //  })

  //  $('#videoModal').on('hide.bs.modal', function (e) {
  //    $("#video").attr('src', $videoSrc);
  //  })
  //});



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

