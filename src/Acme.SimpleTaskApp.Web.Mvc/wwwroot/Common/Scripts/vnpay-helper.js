// VNPay Payment Helper
var VnpayHelper = (function () {
    
    // Tạo URL thanh toán và redirect
    function createPaymentUrl(orderId, amount, orderDescription) {
        // Validate input
        if (!orderId || !amount || amount <= 0) {
            abp.notify.error('Thông tin thanh toán không hợp lệ');
            return;
        }

        // Show loading
        abp.ui.setBusy();

        var requestData = {
            orderId: orderId,
            amount: amount,
            orderDescription: orderDescription || 'Thanh toán đơn hàng ' + orderId
        };

        $.ajax({
            url: '/api/Vnpay/create-payment-url',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                abp.ui.clearBusy();
                
                if (response.success && response.paymentUrl) {
                    // Redirect to VNPay payment page
                    window.location.href = response.paymentUrl;
                } else {
                    abp.notify.error('Không thể tạo link thanh toán');
                }
            },
            error: function (xhr, status, error) {
                abp.ui.clearBusy();
                
                var errorMessage = 'Có lỗi xảy ra khi tạo link thanh toán';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                
                abp.notify.error(errorMessage);
            }
        });
    }

    // Hiển thị modal xác nhận thanh toán
    function showPaymentConfirmation(orderId, amount, onConfirm) {
        var formattedAmount = new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);

        abp.message.confirm(
            'Bạn sẽ được chuyển đến trang thanh toán VNPay để thanh toán số tiền: ' + formattedAmount,
            'Xác nhận thanh toán',
            function (isConfirmed) {
                if (isConfirmed) {
                    if (typeof onConfirm === 'function') {
                        onConfirm();
                    } else {
                        createPaymentUrl(orderId, amount);
                    }
                }
            }
        );
    }

    // Expose public methods
    return {
        createPaymentUrl: createPaymentUrl,
        showPaymentConfirmation: showPaymentConfirmation
    };
})();

// Usage Example:
/*
// Simple usage
VnpayHelper.createPaymentUrl('ORDER123', 1000000, 'Thanh toán đơn hàng ORDER123');

// With confirmation
VnpayHelper.showPaymentConfirmation('ORDER123', 1000000, function() {
    VnpayHelper.createPaymentUrl('ORDER123', 1000000);
});

// From button click
$('#btnPayWithVnpay').click(function() {
    var orderId = $('#orderId').val();
    var amount = parseFloat($('#totalAmount').val());
    VnpayHelper.showPaymentConfirmation(orderId, amount);
});
*/
