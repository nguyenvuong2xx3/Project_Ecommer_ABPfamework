(function ($) {
  $(function () {
    // Disable AdminLTE treeview auto-collapse behavior
    // Keep all menu groups always open
    
    // Remove data-widget="treeview" to disable collapse behavior
    $('[data-widget="treeview"]').removeAttr('data-widget');
    
    // Ensure all treeview menus are always visible
    $('.nav-treeview').css('display', 'block');
    $('.nav-item.has-treeview').addClass('menu-open menu-is-opening');
    
    // Prevent click on parent menu from collapsing
    $('.nav-item.has-treeview > .nav-link').on('click', function(e) {
      e.preventDefault();
      e.stopPropagation();
      // Do nothing - keep menu always open
      return false;
    });
    
    // Scroll to active menu item
    function scrollToActiveMenuItem() {
      var activeItem = $('.nav-link.active');
      if (activeItem && activeItem.length) {
        activeItem[0].scrollIntoView({ 
          behavior: 'smooth',
          block: 'center' 
        });
      }
    }
    
    // Call after a short delay to ensure DOM is ready
    setTimeout(scrollToActiveMenuItem, 300);
  });
})(jQuery);
