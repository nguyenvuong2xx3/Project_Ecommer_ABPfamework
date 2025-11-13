(function ($) {
  app.modals.BannerDetailModal = function () {
    var _modalManager;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();

      // Edit button from detail modal
      $('#EditBannerFromDetail').on('click', function () {
        var bannerId = $(this).attr("data-banner-id");
        
        // Close detail modal
        _modalManager.close();
        
        // Open edit modal
        setTimeout(function () {
          var _editModal = new app.ModalManager({
            viewUrl: abp.appPath + 'Banners/EditModal',
            scriptUrl: abp.appPath + 'view-resources/Views/Banners/_EditModal.js',
            modalClass: 'BannerEditModal',
            modalSize: 'modal-lg'
          });
          _editModal.open({ id: bannerId });
        }, 300);
      });
    };
  };
})(jQuery);
