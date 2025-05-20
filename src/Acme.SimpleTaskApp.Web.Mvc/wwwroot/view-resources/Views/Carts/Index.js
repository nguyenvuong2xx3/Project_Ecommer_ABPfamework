(function ($) {
  var _cartItemService = abp.services.app.cartItem;
  var _cartService = abp.services.app.cart

    // Xử lý khi nhấn nút tăng
    $(document).on('click', '.btl-click-plus', function () {
      let productId = $(this).data('product-id');
      let input = $(`#quantity-${productId}`);
      let cartId = $(this).data('product-cartid');
      let quantity = parseInt(input.val());
      
      updateCart(productId, quantity, input, cartId);
    });

    // Xử lý khi nhấn nút giảm
    $(document).on('click', '.btl-click-minus', function () {
      let productId = $(this).data('product-id');
      let input = $(`#quantity-${productId}`);
      let cartId = $(this).data('product-cartid');
      let quantity = parseInt(input.val());
      if (quantity < 1) {
        deleteCartItem(productId, cartId);
      }
      else {
        updateCart(productId, quantity, input, cartId);
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
    if (confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?")) {
      abp.ui.setBusy();
      _cartItemService.deleteItem(productId, cartId)
        .done(function () {
          abp.notify.info('Xóa thành công!');
          location.reload();
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


  // add cart
  //$(document).off('click', '.add-to-cart');
  $(document).on('click', '.add-to-cart', function (e) {
    e.preventDefault();
    var productId = $(this).data('product-id');
    var quantity = 1;
    console.log(productId)
    addCart(productId, quantity);
  });

  function addCart(productId, quantity) {
    abp.ui.setBusy();

    $.ajax({
      url: '/Carts/AddCart',
      method: 'POST',
      data: {
        productId: productId,
        quantity: quantity,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
      },
      success: function (result) {
        abp.notify.info('Thêm vào giỏ hàng thành công!');
        location.reload();
      },
      error: function (xhr) {
        abp.notify.error('Thêm vào giỏ hàng thất bại!');
        console.error(xhr);
      },
      complete: function () {
        abp.ui.clearBusy();
      }
    });
  }


})(jQuery);
