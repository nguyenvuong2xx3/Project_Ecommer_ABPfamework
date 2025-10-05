(function ($) {
  $.validator.setDefaults({
	errorElement: 'div',
	errorClass: 'invalid-feedback',
	focusInvalid: false,
	submitOnKeyPress: true,
	ignore: ':hidden',
	highlight: function (element) {
	  $(element).closest('.form-group').find('input:eq(0)').addClass('is-invalid');
	  $(element).closest('.form-group').find('textarea:eq(0)').addClass('is-invalid');
	  $(element).closest('.form-group').find('select:eq(0)').addClass('is-invalid');
	},

	unhighlight: function (element) {
	  $(element).closest('.form-group').find('input:eq(0)').removeClass('is-invalid');
	  $(element).closest('.form-group').find('textarea:eq(0)').removeClass('is-invalid');
	  $(element).closest('.form-group').find('select:eq(0)').removeClass('is-invalid');
	},

	errorPlacement: function (error, element) {
	  if (element.closest('.input-icon').length === 1) {
		error.insertAfter(element.closest('.input-icon'));
	  } else {
		error.insertAfter(element);
	  }
	},

	success: function (label) {
	  label.closest('.form-group').removeClass('has-danger');
	  label.remove();
	},

	submitHandler: function (form) {
	  $(form).find('.alert-danger').hide();
	},
  });

  $.validator.addMethod(
	'email',
	function (value, element) {
	  return /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
	},
	'Please enter a valid Email.'
  );

  $.validator.addMethod(
	'text',
	function (value, element) {
	  return new RegExp($(element).attr('pattern')).test(value);
	},
	'Please enter a well formatted value.'
  );

  $.validator.addMethod(
	'phoneNumberVN',
	function (value, element) {
	  if (value == '') {
		return true;
	  }
	  
	  return /^(0|\+84)(\s|\.)?((3[2-9])|(5[689])|(7[06-9])|(8[1-689])|(9[0-46-9]))(\d)(\s|\.)?(\d{3})(\s|\.)?(\d{3})$/.test(value);
	},
	'Hãy nhập đúng định dạng của số điện thoại'
	);

	$.validator.addMethod("customEmail", function (value, element) {
		return this.optional(element) || /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i.test(value);
	}, "Vui lòng nhập địa chỉ email hợp lệ.");

	$.validator.addMethod("customWebsite", function (value, element) {
		return this.optional(element) || /^(https?:\/\/)?(www\.)?[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}(\/[^\s]*)?$/i.test(value);
	}, "Vui lòng nhập URL hợp lệ.");

})(jQuery);
