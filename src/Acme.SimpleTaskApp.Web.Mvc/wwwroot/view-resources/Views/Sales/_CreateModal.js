(function ($) {
  app.modals.SaleCreateModal = function () {
    var _saleService = abp.services.app.sale;
    var _categoryService = abp.services.app.category;
    var _productService = abp.services.app.product;
    var _productVariantService = abp.services.app.productVariant;

    var _modalManager;
    var _$form = null;

    // Tagify instances
    var categoryTagify, productTagify, productVariantTagify;

    // DateRangePicker data
    var _selectedDateRange = {
      StartDate: null,
      EndDate: null
    };

    this.init = function (modalManager) {
      _modalManager = modalManager;
      _$form = _modalManager.getModal().find('form[name=SaleCreateForm]');

      // Initialize form validation
      initFormValidation();

      // Initialize DateRangePicker
      initDateRangePicker();

      // Initialize event handlers
      initEventHandlers();

      // Initialize Tagify (load data from server)
      initTagify();

      // Initialize currency formatting
      initCurrencyFormatting();
    };

    // Initialize currency formatting
    function initCurrencyFormatting() {
      // Format currency inputs
      $('#MaximumDiscountAmount, #MinimumOrderValue').on('input', function () {
        formatCurrencyInput($(this));
      });

      // Format on blur (final format)
      $('#MaximumDiscountAmount, #MinimumOrderValue').on('blur', function () {
        finalFormatCurrency($(this));
      });

      // Remove formatting on focus for easy editing
      $('#MaximumDiscountAmount, #MinimumOrderValue').on('focus', function () {
        removeCurrencyFormatting($(this));
      });

      // Xử lý input phần trăm giảm giá
      $('#DiscountPercentage').on('input', function () {
        formatPercentageInput($(this));
      });

    }
    // Thêm hàm xử lý phần trăm
    function formatPercentageInput($input) {
      var value = $input.val();

      // Chỉ cho phép số và dấu chấm thập phân
      value = value.replace(/[^\d.]/g, '');

      // Chỉ cho phép một dấu chấm thập phân
      var parts = value.split('.');
      if (parts.length > 2) {
        value = parts[0] + '.' + parts.slice(1).join('');
      }

      // Giới hạn 2 chữ số sau dấu thập phân
      if (parts.length === 2 && parts[1].length > 2) {
        value = parts[0] + '.' + parts[1].substring(0, 2);
      }

      $input.val(value);
    }

    // Format currency input in real-time
    function formatCurrencyInput($input) {
      var value = $input.val().replace(/\./g, '');

      // Only allow numbers
      if (!/^\d*$/.test(value)) {
        value = value.replace(/[^\d]/g, '');
      }

      // Add thousand separators
      if (value.length > 3) {
        value = value.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
      }

      $input.val(value);
    }

    // Final format with proper currency formatting
    function finalFormatCurrency($input) {
      var value = $input.val().replace(/\./g, '');

      if (value === '') {
        $input.val('');
        return;
      }

      var numberValue = parseInt(value);
      if (isNaN(numberValue)) {
        $input.val('');
        return;
      }

      // Format with thousand separators
      $input.val(numberValue.toLocaleString('vi-VN'));
    }

    // Remove formatting for editing
    function removeCurrencyFormatting($input) {
      var value = $input.val().replace(/\./g, '');
      $input.val(value);
    }

    // Parse currency value to number
    function parseCurrencyValue(value) {
      if (!value || value === '') {
        return null;
      }

      var numberValue = parseInt(value.replace(/\./g, ''));
      return isNaN(numberValue) ? null : numberValue;
    }

    // Initialize form validation
    function initFormValidation() {
      // Custom validator for currency fields
      $.validator.addMethod('currency', function (value, element) {
        if (value === '') return true; // Empty is allowed

        var numericValue = parseCurrencyValue(value);
        return numericValue !== null && numericValue >= 0;
      }, 'Vui lòng nhập số tiền hợp lệ');

      $.validator.addMethod('percentage', function (value, element) {
        if (value === '') return false;

        var numericValue = parseFloat(value);
        return !isNaN(numericValue) && numericValue >= 0 && numericValue <= 100;
      }, 'Vui lòng nhập phần trăm hợp lệ (0-100)');
      _$form.validate({
        rules: {
          Name: {
            required: true,
            minlength: 3,
            maxlength: 200
          },
          Description: {
            maxlength: 1000
          },
          DiscountType: {
            required: true
          },
          VoucherCode: {
            maxlength: 50
          },
          DiscountPercentage: {
            required: true,
            percentage: true
          },
          MaximumDiscountAmount: {
            currency: true // Use custom currency validator
          },
          MinimumOrderValue: {
            currency: true // Use custom currency validator
          },
          UsageLimit: {
            digits: true,
            min: 1
          },
          ApplyTo: {
            required: true
          }
        },
        messages: {
          Name: {
            required: 'Tên chương trình không được để trống',
            minlength: 'Tên phải có ít nhất 3 ký tự',
            maxlength: 'Tên không được vượt quá 200 ký tự'
          },
          DiscountType: {
            required: 'Vui lòng chọn loại giảm giá'
          },
          DiscountPercentage: {
            required: 'Phần trăm giảm giá không được để trống',
            percentage: 'Phần trăm giảm giá phải từ 0 đến 100'
          },
          ApplyTo: {
            required: 'Vui lòng chọn phạm vi áp dụng'
          },
          MaximumDiscountAmount: {
            currency: 'Vui lòng nhập số tiền hợp lệ'
          },
          MinimumOrderValue: {
            currency: 'Vui lòng nhập số tiền hợp lệ'
          }
        },
        errorPlacement: function (error, element) {
          error.addClass('text-danger');
          error.insertAfter(element);
        }
      });
    }

    // Initialize DateRangePicker
    function initDateRangePicker() {
      var now = new Date();
      var tomorrow = new Date(now);
      tomorrow.setDate(tomorrow.getDate() + 7);

      $('#DateRange').daterangepicker({
        autoUpdateInput: false,
        startDate: now,
        endDate: tomorrow,
        timePicker: true,
        timePicker24Hour: true,
        timePickerIncrement: 15,
        opens: 'left',
        locale: {
          format: 'DD/MM/YYYY HH:mm',
          applyLabel: 'Áp dụng',
          cancelLabel: 'Hủy',
          fromLabel: 'Từ',
          toLabel: 'Đến',
          customRangeLabel: 'Tùy chỉnh',
          firstDay: 1,
          daysOfWeek: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
          monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
            'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12']
        }
      });

      $('#DateRange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('DD/MM/YYYY HH:mm') + ' - ' + picker.endDate.format('DD/MM/YYYY HH:mm'));
        _selectedDateRange.StartDate = picker.startDate.format('YYYY-MM-DDTHH:mm:ss');
        _selectedDateRange.EndDate = picker.endDate.format('YYYY-MM-DDTHH:mm:ss');
        $('#StartDate').val(_selectedDateRange.StartDate);
        $('#EndDate').val(_selectedDateRange.EndDate);
      });

      $('#DateRange').on('cancel.daterangepicker', function () {
        $(this).val('');
        _selectedDateRange.StartDate = null;
        _selectedDateRange.EndDate = null;
        $('#StartDate').val('');
        $('#EndDate').val('');
      });

      // Set default values
      $('#DateRange').data('daterangepicker').setStartDate(now);
      $('#DateRange').data('daterangepicker').setEndDate(tomorrow);
      $('#DateRange').trigger('apply.daterangepicker', $('#DateRange').data('daterangepicker'));
    }

    // Initialize event handlers
    function initEventHandlers() {
      // DiscountType change - show/hide VoucherCode
      $('#DiscountType').on('change', function () {
        var discountType = parseInt($(this).val());
        if (discountType === 1) { // Voucher
          $('#VoucherCodeGroup').slideDown();
          $('#VoucherCode').prop('required', true);
        } else {
          $('#VoucherCodeGroup').slideUp();
          $('#VoucherCode').prop('required', false);
          $('#VoucherCode').val('');
        }
      });

      // ApplyTo change - show/hide corresponding tags
      $('#ApplyTo').on('change', function () {
        var applyTo = parseInt($(this).val());

        // Hide all
        $('#CategoryIdsGroup, #ProductIdsGroup, #ProductVariantIdsGroup').hide();

        // Show corresponding group
        switch (applyTo) {
          case 1: // Categories
            $('#CategoryIdsGroup').show();
            break;
          case 2: // Products
            $('#ProductIdsGroup').show();
            break;
          case 3: // Variants
            $('#ProductVariantIdsGroup').show();
            break;
        }
      });

      // Generate random voucher code
      $('#GenerateVoucherBtn').on('click', function () {
        var code = generateVoucherCode();
        $('#VoucherCode').val(code);
      });

      // Auto uppercase voucher code
      $('#VoucherCode').on('input', function () {
        $(this).val($(this).val().toUpperCase());
      });
    }

    // Initialize Tagify
    function initTagify() {
      // Load categories
      _categoryService.getAllCategoriesForSelect()
        .done(function (categories) {
          var categoryWhitelist = categories.map(function (cat) {
            return { value: cat.id.toString(), label: cat.name };
          });

          categoryTagify = new Tagify(document.querySelector('#CategoryTags'), {
            whitelist: categoryWhitelist,
            dropdown: {
              enabled: 0,
              maxItems: 50,
              classname: 'tagify__dropdown',
              searchKeys: ['label']
            },
            templates: {
              tag: function (tagData) {
                return `<tag title="${tagData.label}" contenteditable='false' spellcheck='false' tabIndex="-1" 
                        class="${this.settings.classNames.tag} ${tagData.class ? tagData.class : ''}" ${this.getAttributes(tagData)}>
                  <x title='Xóa' class="${this.settings.classNames.tagX}" role='button' aria-label='remove tag'></x>
                  <div><span class="${this.settings.classNames.tagText}">${tagData.label}</span></div>
                </tag>`;
              },
              dropdownItem: function (tagData) {
                return `<div ${this.getAttributes(tagData)} class='${this.settings.classNames.dropdownItem} ${tagData.class ? tagData.class : ''}' 
                        tabindex="0" role="option">${tagData.label}</div>`;
              }
            }
          });
        })
        .fail(function (error) {
          console.error('Failed to load categories:', error);
        });

      // Load products
      _productService.getAllProductsForSelect()
        .done(function (products) {
          var productWhitelist = products.map(function (prod) {
            return { value: prod.id.toString(), label: prod.name };
          });

          productTagify = new Tagify(document.querySelector('#ProductTags'), {
            whitelist: productWhitelist,
            dropdown: {
              enabled: 0,
              maxItems: 50,
              classname: 'tagify__dropdown',
              searchKeys: ['label']
            },
            templates: {
              tag: function (tagData) {
                return `<tag title="${tagData.label}" contenteditable='false' spellcheck='false' tabIndex="-1" 
                        class="${this.settings.classNames.tag} ${tagData.class ? tagData.class : ''}" ${this.getAttributes(tagData)}>
                  <x title='Xóa' class="${this.settings.classNames.tagX}" role='button' aria-label='remove tag'></x>
                  <div><span class="${this.settings.classNames.tagText}">${tagData.label}</span></div>
                </tag>`;
              },
              dropdownItem: function (tagData) {
                return `<div ${this.getAttributes(tagData)} class='${this.settings.classNames.dropdownItem} ${tagData.class ? tagData.class : ''}' 
                        tabindex="0" role="option">${tagData.label}</div>`;
              }
            }
          });
        })
        .fail(function (error) {
          console.error('Failed to load products:', error);
        });

      // Load product variants
      _productVariantService.getAllProductVariantsForSelect()
        .done(function (variants) {
          var variantWhitelist = variants.map(function (variant) {
            var label = variant.productName;
            if (variant.color) label += ' - ' + variant.color;
            if (variant.ram) label += ' ' + variant.ram;
            if (variant.storage) label += ' ' + variant.storage;

            return { value: variant.id.toString(), label: label };
          });

          productVariantTagify = new Tagify(document.querySelector('#ProductVariantTags'), {
            whitelist: variantWhitelist,
            dropdown: {
              enabled: 0,
              maxItems: 50,
              classname: 'tagify__dropdown',
              searchKeys: ['label']
            },
            templates: {
              tag: function (tagData) {
                return `<tag title="${tagData.label}" contenteditable='false' spellcheck='false' tabIndex="-1" 
                        class="${this.settings.classNames.tag} ${tagData.class ? tagData.class : ''}" ${this.getAttributes(tagData)}>
                  <x title='Xóa' class="${this.settings.classNames.tagX}" role='button' aria-label='remove tag'></x>
                  <div><span class="${this.settings.classNames.tagText}">${tagData.label}</span></div>
                </tag>`;
              },
              dropdownItem: function (tagData) {
                return `<div ${this.getAttributes(tagData)} class='${this.settings.classNames.dropdownItem} ${tagData.class ? tagData.class : ''}' 
                        tabindex="0" role="option">${tagData.label}</div>`;
              }
            }
          });
        })
        .fail(function (error) {
          console.error('Failed to load variants:', error);
        });
    }

    // Get Tagify values as integer array
    function getTagifyValues(tagify) {
      if (!tagify || !tagify.value || tagify.value.length === 0) {
        return null;
      }
      return tagify.value.map(function (tag) {
        return parseInt(tag.value);
      });
    }

    // Generate random voucher code
    function generateVoucherCode() {
      var prefix = 'SALE';
      var chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
      var length = 6;
      var code = prefix;

      for (var i = 0; i < length; i++) {
        code += chars.charAt(Math.floor(Math.random() * chars.length));
      }

      return code;
    }

    // Validate ApplyTo configuration
    function validateApplyToConfiguration(applyTo, categoryIds, productIds, variantIds) {
      switch (applyTo) {
        case 1: // Categories
          if (!categoryIds || categoryIds.length === 0) {
            abp.notify.error('Phải chọn ít nhất một danh mục khi áp dụng theo danh mục');
            return false;
          }
          break;
        case 2: // Products
          if (!productIds || productIds.length === 0) {
            abp.notify.error('Phải chọn ít nhất một sản phẩm khi áp dụng theo sản phẩm');
            return false;
          }
          break;
        case 3: // Variants
          if (!variantIds || variantIds.length === 0) {
            abp.notify.error('Phải chọn ít nhất một biến thể khi áp dụng theo biến thể');
            return false;
          }
          break;
      }
      return true;
    }

    // Save function
    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      if (!_selectedDateRange.StartDate || !_selectedDateRange.EndDate) {
        abp.notify.error('Vui lòng chọn thời gian áp dụng');
        return;
      }

      // Get form data
      var discountType = parseInt($('#DiscountType').val());
      var applyTo = parseInt($('#ApplyTo').val());
      var categoryIds = getTagifyValues(categoryTagify);
      var productIds = getTagifyValues(productTagify);
      var variantIds = getTagifyValues(productVariantTagify);

      // Validate ApplyTo configuration
      if (!validateApplyToConfiguration(applyTo, categoryIds, productIds, variantIds)) {
        return;
      }

      var formData = {
        Name: $('#Name').val().trim(),
        Description: $('#Description').val().trim() || null,
        DiscountType: discountType,
        VoucherCode: discountType === 1 ? $('#VoucherCode').val().trim().toUpperCase() : null,
        UsageLimit: $('#UsageLimit').val() ? parseInt($('#UsageLimit').val()) : null,
        StartDate: _selectedDateRange.StartDate,
        EndDate: _selectedDateRange.EndDate,
        DiscountPercentage: parseFloat($('#DiscountPercentage').val()),
        MaximumDiscountAmount: parseCurrencyValue($('#MaximumDiscountAmount').val()),
        MinimumOrderValue: parseCurrencyValue($('#MinimumOrderValue').val()),
        ApplyTo: applyTo,
        CategoryIds: applyTo === 1 ? categoryIds : null,
        ProductIds: applyTo === 2 ? productIds : null,
        ProductVariantIds: applyTo === 3 ? variantIds : null,
        IsActive: $('#IsActive').is(':checked')
      };

      _modalManager.setBusy(true);

      _saleService.createSale(formData)
        .done(function (result) {
          abp.notify.success('Tạo sale thành công!');
          _modalManager.close();
          abp.event.trigger('sale.created', result);
        })
        .fail(function (error) {
          abp.notify.error(error.message || 'Có lỗi xảy ra khi tạo sale');
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})(jQuery);