(function () {
    var _userService = abp.services.app.user;
    var _locationService = abp.services.app.location;
    var _$form = null;
    var _locations = [];

    $(document).ready(function () {
        _$form = $('form[name=updateProfileForm]');

        initializeValidation();
        initializeLocationData();
        bindEvents();
    });

    function initializeValidation() {
        if (!$.fn.validate) {
            return;
        }

        $.validator.addMethod("customEmail", function (value, element) {
            if (!value) return true;
            // đơn giản và an toàn hơn là dùng EmailAddressAttribute phía server cho hợp lệ chuẩn hơn
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
        }, "Địa chỉ email không hợp lệ");


        $.validator.addMethod("vietnamPhone", function (value, element) {
            if (!value) return true;
            return /^(0[3|5|7|8|9])+([0-9]{8})$/.test(value);
        }, "Số điện thoại không hợp lệ");

        $.validator.addMethod("selectRequired", function (value, element) {
            return value !== null && value !== "";
        }, "Vui lòng chọn một giá trị");

        _$form.validate({
            rules: {
                FullName: {
                    required: true,
                    minlength: 2,
                    maxlength: 100
                },
                EmailAddress: {
                    required: true,
                    customEmail: true,
                    maxlength: 256
                },
                PhoneNumber: {
                    required: true,
                    vietnamPhone: true
                },
                TinhThanh: {
                    selectRequired: true
                },
                PhuongXa: {
                    selectRequired: true
                },
                DiaChiChiTiet: {
                    required: true,
                    minlength: 5,
                    maxlength: 200
                }
            },
            messages: {
                FullName: {
                    required: 'Họ tên không được để trống',
                    minlength: 'Họ tên phải có ít nhất 2 ký tự',
                    maxlength: 'Họ tên không được quá 100 ký tự'
                },
                PhoneNumber: {
                    required: 'Số điện thoại không được để trống',
                    vietnamPhone: 'Số điện thoại không hợp lệ (VD:  0912345678)'
                },
                EmailAddress: {
                    required: 'Email không được để trống',
                    customEmail: 'Địa chỉ email không hợp lệ',
                    maxlength: 'Email không được quá 256 ký tự'
                },
                TinhThanh: {
                    selectRequired: 'Vui lòng chọn Tỉnh/Thành phố'
                },
                PhuongXa: {
                    selectRequired: 'Vui lòng chọn Phường/Xã'
                },
                DiaChiChiTiet: {
                    required: 'Địa chỉ chi tiết không được để trống',
                    minlength: 'Địa chỉ chi tiết phải có ít nhất 5 ký tự',
                    maxlength: 'Địa chỉ chi tiết không được quá 200 ký tự'
                }
            },
            errorElement: "div",
            errorClass: "invalid-feedback",
            highlight: function (element) {
                $(element).addClass('is-invalid');
            },
            unhighlight: function (element) {
                $(element).removeClass('is-invalid');
            }
        });
    }

    function validateGioiTinh() {
        var gioiTinh = $('input[name="GioiTinh"]:checked').val();
        var $gioiTinhContainer = $('input[name="GioiTinh"]').closest('.mb-3');
        var $errorMsg = $gioiTinhContainer.find('.gender-error');

        if (gioiTinh === undefined) {
            if ($errorMsg.length === 0) {
                $gioiTinhContainer.append('<div class="gender-error text-danger mt-1">Vui lòng chọn giới tính</div>');
            }
            return false;
        } else {
            $errorMsg.remove();
            return true;
        }
    }

    function bindEvents() {
        // Nút Lưu
        $('#saveProfile').on('click', function (e) {
            e.preventDefault();
            saveProfile();
        });

        // Nút Hủy
        $('#cancelProfileEdit').on('click', function (e) {
            e.preventDefault();
            resetForm();
        });

        // Clear gender error khi chọn
        $('input[name="GioiTinh"]').on('change', function () {
            $('.gender-error').remove();
        });

        // Thay đổi Tỉnh/Thành phố
        $('#TinhThanh').on('change', function () {
            var selectedValue = $(this).val();
            $('#PhuongXa').val('').removeClass('is-valid is-invalid');

            if (selectedValue) {
                populatePhuongXa(selectedValue);
            } else {
                $('#PhuongXa').html('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);
            }
        });
    }

    function initializeLocationData() {
        _locationService.getAllDonViHanhChinh()
            .then(function (result) {
                _locations = result;
                populateTinhThanh();
            })
            .catch(function (error) {
                console.error('Lỗi khi tải dữ liệu địa phương:', error);
                abp.notify.error('Không thể tải dữ liệu địa phương! ');
            });
    }

    function populateTinhThanh() {
        var $tinhThanh = $('#TinhThanh');
        var currentValue = $tinhThanh.val();

        $tinhThanh.html('<option value="">Chọn Tỉnh/Thành phố</option>');

        $.each(_locations, function (index, tinhThanh) {
            $tinhThanh.append($('<option>', {
                value: tinhThanh.matinhTMS,
                text: tinhThanh.tentinhmoi,
                'data-matinhBNV': tinhThanh.matinhBNV
            }));
        });

        if (currentValue) {
            $tinhThanh.val(currentValue);
            populatePhuongXa(currentValue);
        }
    }

    function populatePhuongXa(tinhThanhCode) {
        var $phuongXa = $('#PhuongXa');
        var currentValue = $phuongXa.val();

        $phuongXa.html('<option value="">Chọn Phường/Xã</option>');

        if (!tinhThanhCode) {
            $phuongXa.prop('disabled', true);
            return;
        }

        var selectedTinhThanh = _locations.find(function (p) {
            return p.matinhTMS === tinhThanhCode;
        });

        if (selectedTinhThanh && selectedTinhThanh.phuongxa && selectedTinhThanh.phuongxa.length > 0) {
            $.each(selectedTinhThanh.phuongxa, function (index, phuongXa) {
                $phuongXa.append($('<option>', {
                    value: phuongXa.maphuongxa,
                    text: phuongXa.tenphuongxa
                }));
            });
            $phuongXa.prop('disabled', false);

            if (currentValue) {
                $phuongXa.val(currentValue);
            }
        } else {
            $phuongXa.html('<option value="">Không có dữ liệu phường/xã</option>');
            $phuongXa.prop('disabled', true);
        }
    }

    function saveProfile() {
        if (!_$form.valid()) {
            return;
        }

        if (!validateGioiTinh()) {
            abp.notify.warn('Vui lòng chọn giới tính!');
            return;
        }

        var fullName = $('#FullName').val().trim();
        var parts = fullName.split(' ');
        var name = parts.length > 0 ? parts.pop() : '';
        var surname = parts.length > 0 ? parts.join(' ') : '';

        var formData = {
            Id: $('#UserId').val(),
            Name: name,
            Surname: surname,
            PhoneNumber: $('#PhoneNumber').val(),
            EmailAddress: $('#EmailAddress').val(),
            GioiTinh: $('input[name="GioiTinh"]:checked').val(),
            TinhThanh: $('#TinhThanh').val(),
            PhuongXa: $('#PhuongXa').val(),
            DiaChiChiTiet: $('#DiaChiChiTiet').val(),
            IsDiaChiMacDinh: $('#IsDiaChiMacDinh').is(':checked') ? 1 : 0
        };

        var $formArea = _$form.closest('.info-card');
        abp.ui.setBusy($formArea);

        _userService.updateForCustomer(formData)
            .done(function (result) {
                abp.notify.success('Cập nhật thông tin thành công!');
            })
            .fail(function (error) {
                console.error('API lỗi:', error);
                abp.notify.error('Cập nhật thông tin thất bại:  ' + (error.message || ''));
            })
            .always(function () {
                abp.ui.clearBusy($formArea);
            });
    }

    function resetForm() {
        _$form[0].reset();
        _$form.find('.is-valid, .is-invalid').removeClass('is-valid is-invalid');
        _$form.find('.invalid-feedback, .gender-error').remove();

        if (_$form.data('validator')) {
            _$form.validate().resetForm();
        }

        abp.notify.info('Đã hủy thay đổi');
    }

})();