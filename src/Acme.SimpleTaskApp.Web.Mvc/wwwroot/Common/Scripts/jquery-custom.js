/* Here, there are some custom plug-ins.
 * Developed for ASP.NET Iteration Zero (http://aspnetzero.com). */
(function ($) {
  if (!$) {
    return;
  }

  /* A simple jQuery plug-in to make a button busy. */
  $.fn.buttonBusy = function (isBusy) {
    return $(this).each(function () {
      var $button = $(this);
      var $icon = $button.find('i');
      var $buttonInnerSpan = $button.find('span');

      if (isBusy) {
        if ($button.hasClass('button-busy')) {
          return;
        }

        $button.attr('disabled', 'disabled');

        //change icon
        if ($icon.length) {
          $button.data('iconOriginalClasses', $icon.attr('class'));
          $icon.removeClass();
          $icon.addClass('fa fa-spin fa-spinner');
        }

        //change text
        if ($buttonInnerSpan.length && $button.attr('busy-text')) {
          $button.data('buttonOriginalText', $buttonInnerSpan.html());
          $buttonInnerSpan.html($button.attr('busy-text'));
        }

        $button.addClass('button-busy');
      } else {
        if (!$button.hasClass('button-busy')) {
          return;
        }

        //enable button
        $button.removeAttr('disabled');

        //restore icon
        if ($icon.length && $button.data('iconOriginalClasses')) {
          $icon.removeClass();
          $icon.addClass($button.data('iconOriginalClasses'));
        }

        //restore text
        if ($buttonInnerSpan.length && $button.data('buttonOriginalText')) {
          $buttonInnerSpan.html($button.data('buttonOriginalText'));
        }

        $button.removeClass('button-busy');
      }
    });
  };

  $.fn.serializeFormToObject = function () {
    var $form = $(this);
    var fields = $form.find('[disabled]');
    fields.prop('disabled', false);
    var json = $form.serializeJSON();
    fields.prop('disabled', true);
    return json;
  };

  $.fn.inputMaskForm = function () {
    $(".currency-input").inputmask({
      'alias': 'decimal',
      'groupSeparator': '.',
      'autoGroup': true,
      'digits': 0
    });

    $(".website-input").inputmask({
      //mask: "(http|https)://*{1,50}[.*]{0,50}",
      placeholder: "",
      showMaskOnHover: false,
      showMaskOnFocus: false,
      definitions: {
        '*': {
          validator: "[0-9A-Za-z-._~:/?#\\[\\]@!$&'()*+,;=]",
          cardinality: 1,
          casing: "lower"
        }
      }
    });

    $(".email-input").inputmask({
      'alias': 'email',
    });

    $(".numeric-input").inputmask({
      'alias': 'numeric',
      'groupSeparator': '.',
      'autoGroup': true,
      'radixPoint': ',',
      digits: 2,
    });

    $(".percentage-input").inputmask({
      'alias': 'percentage',
      allowMinus: false,
      max: null,
      digits: 2,
    });
  };

})(jQuery);
