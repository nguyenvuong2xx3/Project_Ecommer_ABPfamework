(function ($) {
  app.modals.SaleDetailModal = function () {
    var _modalManager;

    this.init = function (modalManager) {
      _modalManager = modalManager;

      // Handle Edit button from detail modal
      $('#EditSaleFromDetail').on('click', function () {
        var saleId = $(this).data('sale-id');
        
        // Close detail modal
        _modalManager.close();
        
        // Open edit modal
        var _editModal = new app.ModalManager({
          viewUrl: abp.appPath + 'Sales/EditModal',
          scriptUrl: abp.appPath + 'view-resources/Views/Sales/_EditModal.js',
          modalClass: 'SaleEditModal',
          modalSize: 'modal-xl'
        });
        
        _editModal.open({ id: saleId });
      });
    };
  };
})(jQuery);
