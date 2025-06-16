(function ($) {
  var _cartItemService = abp.services.app.cartItem;
  var _cartService = abp.services.app.cart

    // Xử lý khi nhấn nút tăng
    $(document).on('click', '.btl-click-plus', function () {
      let productId = $(this).data('product-id');
      let input = $(`#quantity-${productId}`);
      let cartId = $(this).data('product-cartid');
      let quantity = parseInt(input.val());
      // Lấy số hiện tại trong #cart-count
      var $cartCount = $('#cart-count');
      var currentCount = parseInt($cartCount.text()) || 0;
      var newCount = currentCount + 1;

      // Cập nhật lại số và hiện badge nếu đang ẩn
      $cartCount.text(newCount);
      if (newCount > 0) {
        $cartCount.css('display', 'inline-block');
      }
      updateCart(productId, quantity, input, cartId);
    });

    // Xử lý khi nhấn nút giảm
    $(document).on('click', '.btl-click-minus', function () {
      let productId = $(this).data('product-id');
      let input = $(`#quantity-${productId}`);
      let cartId = $(this).data('product-cartid');
      let quantity = parseInt(input.val());

      // Lấy số hiện tại trong #cart-count
      var $cartCount = $('#cart-count');
      var currentCount = parseInt($cartCount.text()) || 0;
      var newCount = currentCount > 0 ? currentCount - 1 : 0;

      // Nếu số lượng sau khi giảm <= 0, xóa sản phẩm khỏi giỏ hàng
      if (quantity < 1) {
        deleteCartItem(productId, cartId, $cartCount);

        //// Giảm badge giỏ hàng
        //$cartCount.text(newCount);
        //if (newCount > 0) {
        //  $cartCount.css('display', 'inline-block');
        //} else {
        //  $cartCount.css('display', 'none');
        //}
      } else {
        // Giảm số lượng sản phẩm
        let newQuantity = quantity - 1;
        input.val(newQuantity);

        // Cập nhật giỏ hàng
        updateCart(productId, quantity, input, cartId);

        // Giảm badge giỏ hàng
        $cartCount.text(newCount);
        if (newCount > 0) {
          $cartCount.css('display', 'inline-block');
        } else {
          $cartCount.css('display', 'none');
        }
      }
    });
    // Xử lý khi người dùng thay đổi trực tiếp giá trị trong input
    $(document).on('change', '.quantity-input', function () {
      let productId = $(this).data('product-id');
      let input = $(`#quantity-${productId}`);
      let cartId = $(this).data('product-cartid');
      let quantity = parseInt($(this).val());

      if (isNaN(quantity) || quantity < 1) {
        quantity = 1; // Đặt về giá trị tối thiểu là 1 nếu nhập sai
      }

      updateCart(productId, quantity, input, cartId);
    });


    // Hàm cập nhật giỏ hàng
  function updateCart(productId, quantity, input, cartId) {
      abp.ui.setBusy();
      _cartItemService.updateItem({
        productId: productId,
        cartId: cartId, // Thêm CartId
        quantity: quantity
      }).done(function () {
        input.val(quantity);
        const price = parseFloat($('#product-price-' + productId).text());
        const newTotal = price * quantity;
        $('#product-total-' + productId).text(newTotal.toLocaleString('vi-VN') + ' VND');
        abp.notify.info('Cập nhật thành công!');
      }).fail(function (error) {
        abp.notify.error('Cập nhật thất bại!');
        console.error(error);
      }).always(function () {
        abp.ui.clearBusy();
      });
  }

  $(document).on('click', '.btl-click-delete', function () {
    let productId = $(this).data('product-id');
    let cartId = $(this).data('product-cartid');
    deleteCartItem(productId, cartId);
  });

  function deleteCartItem(productId, cartId) {
    abp.message.confirm(
      'Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?',
      'Xác nhận xóa',
      function (isConfirmed) {
        if (isConfirmed) {
          abp.ui.setBusy();
          _cartItemService.deleteItem(productId, cartId)
            .done(function () {
              abp.notify.info('Xóa thành công!');
              location.reload();
              //// Giảm badge giỏ hàng
              //$cartCount.text(newCount);
              //if (newCount > 0) {
              //  $cartCount.css('display', 'inline-block');
              //} else {
              //  $cartCount.css('display', 'none');
              //}
            })
            .fail(function (error) {
              abp.notify.error('Xóa thất bại!');
              console.error(error);
            })
            .always(function () {
              abp.ui.clearBusy();
            });
        } else {
          document.getElementById(`quantity-${productId}`).value = 1;
        }
      }
    );
  }


  // add cart
  //$(document).off('click', '.add-to-cart');
  $(document).on('click', '.add-to-cart', function (e) {
    e.preventDefault();
    var productId = $(this).data('product-id');
    var quantity = 1;
    addToCart(productId, quantity);

    // Lấy số hiện tại trong #cart-count
   
  });

  function addToCart(productId, quantity) {
    abp.ui.setBusy();
    _cartService.createCart(productId, quantity)
      .done(function () {
        abp.notify.success('Thêm vào giỏ hàng thành công!');
        var $cartCount = $('#cart-count');
        var currentCount = parseInt($cartCount.text()) || 0;
        var newCount = currentCount + 1;

        // Cập nhật lại số và hiện badge nếu đang ẩn
        $cartCount.text(newCount);
        if (newCount > 0) {
          $cartCount.css('display', 'inline-block');
        }
      })
      .fail(function (error) {
        abp.message.error('Thêm vào giỏ hàng thất bại đăng nhập để thêm vào giỏ hàng ');
      })
      .always(function () {
        abp.ui.clearBusy();
      });
  }
  $('#Btn_addOrder').click(function () {

    abp.services.app.sendMail.sendMailOrder().done(function () {
      abp.message.success('Đặt hàng thành công');
    }).fail(function (error) {
      abp.message.error('Gửi mail thất bại!');
    });
  });
})(jQuery);
