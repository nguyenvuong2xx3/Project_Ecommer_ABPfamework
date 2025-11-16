(function ($) {
  app.modals.SaleEditModal = function () {
    var _saleService = abp.services.app.sale;
    var _categoryService = abp.services.app.category;
    var _productService = abp.services.app.product;
    var _productVariantService = abp.services.app.productVariant;

    var _modalManager;
    var _$saleEditForm = null;

    // Tagify instances
    var categoryTagify, productTagify, productVariantTagify;

    // DateRangePicker data
    var _selectedDateRange = {
      StartDate: null,
      EndDate: null
    };

    this.init = function (modalManager) {
      _modalManager = modalManager;
      _$saleEditForm = _modalManager.getModal().find('form[name=SaleEditForm]');

      // Get initial dates from hidden inputs
      var startDate = $('#StartDate').val();
      var endDate = $('#EndDate').val();

      // Initialize form validation
      initFormValidation();

      // Initialize DateRangePicker with existing dates
      initDateRangePicker(startDate, endDate);

      // Initialize event handlers
      initEventHandlers();

      // Initialize Tagify with existing data
      initTagify();
    };

    // Initialize form validation
    function initFormValidation() {
      _$saleEditForm.validate({
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
            number: true,
            min: 0,
            max: 100
          },
          MaximumDiscountAmount: {
            number: true,
            min: 0
          },
          MinimumOrderValue: {
            number: true,
            min: 0
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
            min: 'Phần trăm giảm giá phải lớn hơn hoặc bằng 0',
            max: 'Phần trăm giảm giá không được vượt quá 100'
          },
          ApplyTo: {
            required: 'Vui lòng chọn phạm vi áp dụng'
          }
        },
        errorPlacement: function (error, element) {
          error.addClass('text-danger');
          error.insertAfter(element);
        }
      });
    }

    // Initialize DateRangePicker with existing dates
    function initDateRangePicker(startDate, endDate) {
      var start = moment(startDate);
      var end = moment(endDate);

      $('#DateRange').daterangepicker({
        autoUpdateInput: true,
        startDate: start,
        endDate: end,
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

      // Set initial value
      $('#DateRange').val(start.format('DD/MM/YYYY HH:mm') + ' - ' + end.format('DD/MM/YYYY HH:mm'));
      _selectedDateRange.StartDate = start.format('YYYY-MM-DDTHH:mm:ss');
      _selectedDateRange.EndDate = end.format('YYYY-MM-DDTHH:mm:ss');

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

    // Initialize Tagify with existing data
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

          // Add existing category tags
          addExistingCategoryTags(categories);
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

          addExistingProductTags(products);
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

          addExistingVariantTags(variants);
        })
        .fail(function (error) {
          console.error('Failed to load variants:', error);
        });
    }

    // Add existing category tags from hidden input
    function addExistingCategoryTags(allCategories) {
      // Try to get from modal args first
      var modalArgs = _modalManager.getArgs();
      var saleId = modalArgs ? modalArgs.id : null;
      
      if (!saleId) return;

      // Get sale data to extract existing IDs
      _saleService.getSaleById(saleId)
        .done(function (sale) {
          if (sale.categoryIds && sale.categoryIds.length > 0) {
            var tagsToAdd = [];
            sale.categoryIds.forEach(function (id) {
              var cat = allCategories.find(function (c) { return c.id === id; });
              if (cat) {
                tagsToAdd.push({ value: cat.id.toString(), label: cat.name });
              }
            });
            
            if (tagsToAdd.length > 0) {
              categoryTagify.addTags(tagsToAdd);
            }
          }
        });
    }

    // Add existing product tags
    function addExistingProductTags(allProducts) {
      var modalArgs = _modalManager.getArgs();
      var saleId = modalArgs ? modalArgs.id : null;
      
      if (!saleId) return;

      _saleService.getSaleById(saleId)
        .done(function (sale) {
          if (sale.productIds && sale.productIds.length > 0) {
            var tagsToAdd = [];
            sale.productIds.forEach(function (id) {
              var prod = allProducts.find(function (p) { return p.id === id; });
              if (prod) {
                tagsToAdd.push({ value: prod.id.toString(), label: prod.name });
              }
            });
            
            if (tagsToAdd.length > 0) {
              productTagify.addTags(tagsToAdd);
            }
          }
        });
    }

    // Add existing variant tags
    function addExistingVariantTags(allVariants) {
      var modalArgs = _modalManager.getArgs();
      var saleId = modalArgs ? modalArgs.id : null;
      
      if (!saleId) return;

      _saleService.getSaleById(saleId)
        .done(function (sale) {
          if (sale.productVariantIds && sale.productVariantIds.length > 0) {
            var tagsToAdd = [];
            sale.productVariantIds.forEach(function (id) {
              var variant = allVariants.find(function (v) { return v.id === id; });
              if (variant) {
                var label = variant.productName;
                if (variant.color) label += ' - ' + variant.color;
                if (variant.ram) label += ' ' + variant.ram;
                if (variant.storage) label += ' ' + variant.storage;
                
                tagsToAdd.push({ value: variant.id.toString(), label: label });
              }
            });
            
            if (tagsToAdd.length > 0) {
              productVariantTagify.addTags(tagsToAdd);
            }
          }
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
      if (!_$saleEditForm.valid()) {
        return;
      }

      if (!_selectedDateRange.StartDate || !_selectedDateRange.EndDate) {
        abp.notify.error('Vui lòng chọn thời gian áp dụng');
        return;
      }

      // Get form data
      var saleId = parseInt($('input[name="Id"]').val());
      var discountType = parseInt($('#DiscountType').val());
      var applyTo = parseInt($('#ApplyTo').val());
      var categoryIds = getTagifyValues(categoryTagify);
      var productIds = getTagifyValues(productTagify);
      var variantIds = getTagifyValues(productVariantTagify);

      // Validate ApplyTo configuration
      if (!validateApplyToConfiguration(applyTo, categoryIds, productIds, variantIds)) {
        return;
      }

      // Build request data
      var formData = {
        Id: saleId,
        Name: $('#Name').val().trim(),
        Description: $('#Description').val().trim() || null,
        DiscountType: discountType,
        VoucherCode: discountType === 1 ? $('#VoucherCode').val().trim().toUpperCase() : null,
        UsageLimit: $('#UsageLimit').val() ? parseInt($('#UsageLimit').val()) : null,
        StartDate: _selectedDateRange.StartDate,
        EndDate: _selectedDateRange.EndDate,
        DiscountPercentage: parseFloat($('#DiscountPercentage').val()),
        MaximumDiscountAmount: $('#MaximumDiscountAmount').val() ? parseFloat($('#MaximumDiscountAmount').val()) : null,
        MinimumOrderValue: $('#MinimumOrderValue').val() ? parseFloat($('#MinimumOrderValue').val()) : null,
        ApplyTo: applyTo,
        CategoryIds: applyTo === 1 ? categoryIds : null,
        ProductIds: applyTo === 2 ? productIds : null,
        ProductVariantIds: applyTo === 3 ? variantIds : null,
        IsActive: $('#IsActive').is(':checked')
      };

      _modalManager.setBusy(true);

      _saleService.updateSale(formData)
        .done(function (result) {
          abp.notify.success('Cập nhật sale thành công!');
          _modalManager.close();
          abp.event.trigger('sale.updated', result);
        })
        .fail(function (error) {
          abp.notify.error(error.message || 'Có lỗi xảy ra khi cập nhật sale');
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})(jQuery);
